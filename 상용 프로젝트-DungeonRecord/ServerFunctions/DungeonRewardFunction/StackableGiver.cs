using BackEnd;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using BackendSharedLib;

namespace BackendFunction
{
    public static class StackableGiver
    {
        public static Stream Give(string rewardID, string rewardTableID, string inventoryConfigTableID)
        {
            var rewardTableResult = DungeonRewardTable.Set(rewardTableID);
            if (!string.IsNullOrEmpty(rewardTableResult))
                return ReturnObject.Error(rewardTableResult);

            if (!DungeonRewardTable.Entries.TryGetValue(rewardID, out var candidates) || candidates.Count == 0)
                return ReturnObject.Error(5000, $"No reward candidates for rewardID {rewardID}");

            foreach (var c in candidates)
            {
                if (!ItemID.IsStackable(c.ItemID) && !ItemID.IsExp(c.ItemID))
                    return ReturnObject.Error(5001, $"Non-stackable/exp item mixed in reward pool: {c.ItemID}");
            }

            bool isOneTime = !Backend.HasKey("stamina");

            var userBro = Backend.GameData.GetMyData("UserData", new Where());
            if (!userBro.IsSuccess())
                return ReturnObject.Error($"Get UserData failed: {userBro.ErrorCode}");

            var userData = JsonConvert.DeserializeObject<UserData>(userBro.FlattenRows()[0]["userData"].ToJson());

            if (isOneTime)
            {
                if (userData.ClaimedDungeonRewardIDs.Contains(rewardID))
                {
                    var p = new Param();
                    p.Add("msg", "일회성 던전 보상 중복 수령 시도");
                    p.Add("rewardID", rewardID);
                    return ReturnObject.Suspect(p);
                }

                userData.ClaimedDungeonRewardIDs.Add(rewardID);
            }

            var rand = new Random();
            long gainedExp = 0;
            var rewards = new List<TransactionItemSpec>(candidates.Count);
            foreach (var c in candidates)
            {
                int amt = rand.Next(c.Min, c.Max + 1);
                if (ItemID.IsExp(c.ItemID))
                {
                    gainedExp += amt;
                    userData.TotalExp += amt;
                }
                else
                {
                    rewards.Add(new TransactionItemSpec { ID = c.ItemID, Amount = amt });
                }
            }

            var changeResult = RewardService.GiveReward(rewards, RewardFullPolicy.Overflow, inventoryConfigTableID, userData);

            if (rewards.Count == 0)
            {
                var p = new Param();
                p.Add("userData", userData);
                var uBro = Backend.GameData.Update("UserData", new Where(), p);
                if (!uBro.IsSuccess())
                    return ReturnObject.Error($"Update UserData failed: {uBro.ErrorCode}");
            }

            var result = new RewardResult { Inventory = changeResult, GainedExp = gainedExp };
            return ReturnObject.Success(JsonConvert.SerializeObject(result));
        }
    }
}
