using Amazon.Lambda.Core;
using BackEnd;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BackendSharedLib;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace BackendFunction
{
    /// <summary>
    /// 전투 중 소모품 사용/아이템 버리기 등 인벤토리에서 아이템을 소비하는 행위 검증 및 차감.
    /// items: Stackable 전용 (TransactionItemSpec[] 직렬화 문자열)
    /// equipInstanceIDs: Weapon/Wearable 전용 (long[] 직렬화 문자열)
    /// </summary>
    public class BFunc
    {
        public Stream Function(Stream stream, ILambdaContext context)
        {
            try
            {
                Backend.Initialize(ref stream);

                List<TransactionItemSpec> requestedItems = new();
                if (Backend.Content.ContainsKey("items"))
                {
                    var raw = Backend.Content["items"].ToString();
                    if (!string.IsNullOrEmpty(raw))
                        requestedItems = JsonConvert.DeserializeObject<List<TransactionItemSpec>>(raw) ?? new();
                }

                List<long> equipInstanceIDs = new();
                if (Backend.Content.ContainsKey("equipInstanceIDs"))
                {
                    var raw = Backend.Content["equipInstanceIDs"].ToString();
                    if (!string.IsNullOrEmpty(raw))
                        equipInstanceIDs = JsonConvert.DeserializeObject<List<long>>(raw) ?? new();
                }

                if (requestedItems.Count == 0 && equipInstanceIDs.Count == 0)
                    return ReturnObject.Error(5000, "empty request");

                var requiredStackables = new Dictionary<int, int>();
                foreach (var spec in requestedItems)
                {
                    if (spec.Amount <= 0)
                        return SuspectReason("non-positive amount",
                            ("itemID", spec.ID), ("amount", spec.Amount));

                    requiredStackables[spec.ID] =
                        requiredStackables.GetValueOrDefault(spec.ID) + spec.Amount;
                }

                bool needStack = requiredStackables.Count > 0;
                bool needEquip = equipInstanceIDs.Count > 0;

                if (needStack) ReadBatcher.EnqueueRead("Inventory_Stack");
                if (needEquip) ReadBatcher.EnqueueRead("Inventory_Equip");

                var readResult = ReadBatcher.Flush();
                if (readResult.ContainsKey("error"))
                    return ReturnObject.Error(readResult["error"].ToString());

                Dictionary<int, int> stackRecords = null;
                Dictionary<long, string> weaponRecords = null;
                Dictionary<long, string> wearableRecords = null;

                if (needStack)
                    stackRecords = JsonConvert.DeserializeObject<Dictionary<int, int>>(
                        readResult["Inventory_Stack"]["items"].ToJson()) ?? new();
                if (needEquip)
                {
                    weaponRecords = JsonConvert.DeserializeObject<Dictionary<long, string>>(
                        readResult["Inventory_Equip"]["weapons"].ToJson()) ?? new();
                    wearableRecords = JsonConvert.DeserializeObject<Dictionary<long, string>>(
                        readResult["Inventory_Equip"]["wearables"].ToJson()) ?? new();
                }

                var consumed = new List<TransactionItemSpec>();

                foreach (var kv in requiredStackables)
                {
                    int owned = stackRecords.GetValueOrDefault(kv.Key);
                    if (owned < kv.Value)
                    {
                        var p = new Param();
                        p.Add("itemID", kv.Key);
                        p.Add("ownedAmount", owned);
                        p.Add("requiredAmount", kv.Value);
                        return ReturnObject.Suspect(p);
                    }
                    int remain = owned - kv.Value;
                    if (remain == 0)
                        stackRecords.Remove(kv.Key);
                    else
                        stackRecords[kv.Key] = remain;
                    consumed.Add(new TransactionItemSpec { ID = kv.Key, Amount = kv.Value });
                }

                bool weaponsChanged = false;
                bool wearablesChanged = false;
                foreach (var instanceID in equipInstanceIDs)
                {
                    if (weaponRecords.TryGetValue(instanceID, out var raw))
                    {
                        int itemID = int.Parse(raw.Split('|')[0]);
                        weaponRecords.Remove(instanceID);
                        weaponsChanged = true;
                        consumed.Add(new TransactionItemSpec { ID = itemID, Amount = 1 });
                    }
                    else if (wearableRecords.TryGetValue(instanceID, out raw))
                    {
                        int itemID = int.Parse(raw.Split('|')[0]);
                        wearableRecords.Remove(instanceID);
                        wearablesChanged = true;
                        consumed.Add(new TransactionItemSpec { ID = itemID, Amount = 1 });
                    }
                    else
                    {
                        var p = new Param();
                        p.Add("msg", "equipInstanceID not owned");
                        p.Add("instanceID", instanceID);
                        return ReturnObject.Suspect(p);
                    }
                }

                if (needStack)
                {
                    var iParam = new Param();
                    iParam.Add("items", stackRecords);
                    WriteBatcher.EnqueueWrite("Inventory_Stack", iParam);
                }
                if (weaponsChanged)
                {
                    var eParam = new Param();
                    eParam.Add("weapons", weaponRecords);
                    WriteBatcher.EnqueueWrite("Inventory_Equip", eParam);
                }
                if (wearablesChanged)
                {
                    var eParam = new Param();
                    eParam.Add("wearables", wearableRecords);
                    WriteBatcher.EnqueueWrite("Inventory_Equip", eParam);
                }

                var writeResult = WriteBatcher.Flush();
                if (!string.IsNullOrEmpty(writeResult))
                    return ReturnObject.Error(writeResult);

                return ReturnObject.Success(JsonConvert.SerializeObject(consumed));
            }
            catch (Exception e)
            {
                return ReturnObject.Error(8000, e.Message);
            }
        }

        private static Stream SuspectReason(string msg, params (string k, object v)[] extras)
        {
            var p = new Param();
            p.Add("msg", msg);
            foreach (var (k, v) in extras) p.Add(k, v);
            return ReturnObject.Suspect(p);
        }
    }
}
