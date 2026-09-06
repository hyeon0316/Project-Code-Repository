using LitJson;
using BackEnd;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BackendSharedLib
{
    public class InventoryChangeResult
    {
        public Dictionary<int, int> StackableDic = new();
        public Dictionary<long, string> EquipItemDic = new();
        public Dictionary<int, int> CostDic = new();
    }

    public enum RewardFullPolicy
    {
        Abort, //사전차단
        Mail, //우편발송
        Overflow //강제수령
    }

    /// <summary>
    /// 재화 지급/차감은 GiveReward 내부에서 처리.
    /// userData 인자: 호출부가 이미 UserData를 읽었으면 인스턴스를 넘긴다(중복 읽기 방지). 호출부가 UserData에 다른 변경(예: LastReceivedRewardLevel)을 가했다면 GiveReward 호출 전에 미리 적용해 둘 것.
    /// userData == null이면 GiveReward가 ReadBatcher로 함께 읽는다.
    /// 어느 경우든 UserData write 큐잉은 GiveReward가 담당한다(userData가 넘어왔거나, currency가 있어 내부에서 읽었을 때).
    /// costs 인자: 재화/스택형 차감 대상. 부족 시 GameLog에 "Potential Cheater" 기록 후 throw.
    /// </summary>
    public static class RewardService
    {
        public static InventoryChangeResult GiveReward(List<TransactionItemSpec> rewards,
            RewardFullPolicy policy,
            string inventoryConfigTableID,
            UserData userData = null,
            List<TransactionItemSpec> costs = null)
        {
            var result = new InventoryChangeResult();
            costs ??= new List<TransactionItemSpec>();
            if (rewards.Count == 0 && costs.Count == 0)
                return result;

            var weapons = rewards.Where(r => ItemID.IsWeapon(r.ID)).ToList();
            var stackables = rewards.Where(r => ItemID.IsStackable(r.ID)).ToList();
            var currencies = rewards.Where(r => ItemID.IsCurrency(r.ID)).ToList();

            var costStackables = costs.Where(c => ItemID.IsStackable(c.ID)).ToList();
            var costCurrencies = costs.Where(c => ItemID.IsCurrency(c.ID)).ToList();
            var invalidCost = costs.FirstOrDefault(c => !ItemID.IsStackable(c.ID) && !ItemID.IsCurrency(c.ID));
            if (invalidCost != null)
                throw new RewardException(5000, $"Unsupported cost item type: {invalidCost.ID}");

            bool hasCurrency = currencies.Count > 0 || costCurrencies.Count > 0;
            bool hasStackable = stackables.Count > 0 || costStackables.Count > 0;

            if (weapons.Count == 0 && !hasStackable && !hasCurrency)
            {
                var earlyFlush = WriteBatcher.Flush();
                if (!string.IsNullOrEmpty(earlyFlush))
                    throw new RewardException(8000, earlyFlush);
                return result;
            }

            bool needLoadUserData = hasCurrency && userData == null;

            if (weapons.Count != 0)
                ReadBatcher.EnqueueRead("Inventory_Equip");
            if (hasStackable)
                ReadBatcher.EnqueueRead("Inventory_Stack");
            if (needLoadUserData)
                ReadBatcher.EnqueueRead("UserData");

            var readJsonData = ReadBatcher.Flush();
            if (readJsonData.ContainsKey("error"))
                throw new RewardException(8000, readJsonData["error"].ToString());

            if (policy != RewardFullPolicy.Abort)
            {
                var inventoryConfig = InventoryConfigTable.Set(inventoryConfigTableID);
                if (!string.IsNullOrEmpty(inventoryConfig))
                    throw new RewardException(8000, inventoryConfig);
            }

            if (hasCurrency && userData == null)
            {
                userData = JsonConvert.DeserializeObject<UserData>(
                    readJsonData["UserData"]["userData"].ToJson());
            }

            Dictionary<int, int> ownedStackables = null;
            if (hasStackable)
            {
                ownedStackables = JsonConvert.DeserializeObject<Dictionary<int, int>>(
                    readJsonData["Inventory_Stack"]["items"].ToJson());
            }

            foreach (var c in costCurrencies)
            {
                int owned = userData.Balances.GetValueOrDefault(c.ID);
                if (owned < c.Amount)
                {
                    var p = new Param();
                    p.Add("msg", "재화 부족");
                    p.Add("costID", c.ID);
                    p.Add("required", c.Amount);
                    p.Add("owned", owned);
                    Backend.GameLog.InsertLogV2("Potential Cheater", p);
                    throw new RewardException(6001, $"InsufficientCurrency: {c.ID} need {c.Amount} owned {owned}");
                }
                userData.Balances[c.ID] = owned - c.Amount;
                result.CostDic[c.ID] = result.CostDic.GetValueOrDefault(c.ID) + c.Amount;
            }
            foreach (var s in costStackables)
            {
                int owned = ownedStackables.GetValueOrDefault(s.ID);
                if (owned < s.Amount)
                {
                    var p = new Param();
                    p.Add("msg", "스택 부족");
                    p.Add("costID", s.ID);
                    p.Add("required", s.Amount);
                    p.Add("owned", owned);
                    Backend.GameLog.InsertLogV2("Potential Cheater", p);
                    throw new RewardException(6001, $"InsufficientStack: {s.ID} need {s.Amount} owned {owned}");
                }
                ownedStackables[s.ID] = owned - s.Amount;
                result.CostDic[s.ID] = result.CostDic.GetValueOrDefault(s.ID) + s.Amount;
            }
            foreach (var c in currencies)
            {
                userData.Balances[c.ID] = userData.Balances.GetValueOrDefault(c.ID) + c.Amount;
                result.StackableDic[c.ID] = result.StackableDic.GetValueOrDefault(c.ID) + c.Amount;
            }

            if (userData != null)
            {
                var userParam = new Param();
                userParam.Add("userData", userData);
                WriteBatcher.EnqueueWrite("UserData", userParam);
            }

            if (weapons.Count > 0)
            {
                var param = new Param();
                JsonData jsonData = readJsonData["Inventory_Equip"]["weapons"];
                var ownedItems = JsonConvert.DeserializeObject<Dictionary<long, string>>(jsonData.ToJson());
                if (policy != RewardFullPolicy.Overflow)
                {
                    //TODO: Mail의 경우 처리
                    if (ownedItems.Count + weapons.Count > InventoryConfigTable.GetValue("Weapon"))
                        throw new RewardException(6004, "InventoryFull: Not enough weapon slot");
                }

                int n = 0;
                foreach (var w in weapons)
                {
                    bool isLock = ItemID.GetRarity(w.ID) >= 1;
                    long instanceID = DateTime.UtcNow.Ticks + n++;
                    string newItem = $"{w.ID}|0|{isLock}|0";
                    ownedItems.Add(instanceID, newItem);
                    result.EquipItemDic.Add(instanceID, newItem);
                }

                param.Add("weapons", ownedItems);
                WriteBatcher.EnqueueWrite("Inventory_Equip", param);
            }

            if (hasStackable)
            {
                foreach (var s in stackables)
                {
                    if (ownedStackables.ContainsKey(s.ID))
                        ownedStackables[s.ID] += s.Amount;
                    else
                        ownedStackables[s.ID] = s.Amount;

                    if (policy != RewardFullPolicy.Overflow)
                    {
                        //TODO: Mail의 경우 처리
                        if (ownedStackables[s.ID] > InventoryConfigTable.GetValue("MaxStack"))
                            throw new RewardException(6004, $"InventoryFull: Not enough {s.ID}'s stack");
                    }

                    if (result.StackableDic.ContainsKey(s.ID))
                        result.StackableDic[s.ID] += s.Amount;
                    else
                        result.StackableDic[s.ID] = s.Amount;
                }

                var param = new Param();
                param.Add("items", ownedStackables);
                WriteBatcher.EnqueueWrite("Inventory_Stack", param);
            }

            var writeResult = WriteBatcher.Flush();
            if (!string.IsNullOrEmpty(writeResult))
                throw new RewardException(8000, writeResult);

            return result;
        }
    }
}
