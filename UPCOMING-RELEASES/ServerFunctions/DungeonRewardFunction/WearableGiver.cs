using BackEnd;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BackendSharedLib;

namespace BackendFunction
{
    public static class WearableGiver
    {
        public static Stream Give(string rewardID, string rewardTableID, string inventoryConfigTableID)
        {
            int underscoreIdx = rewardID.IndexOf('_');
            if (underscoreIdx < 0 || !int.TryParse(rewardID.AsSpan(underscoreIdx + 1), out int worldModPhase))
                return ReturnObject.Error(5000, $"Invalid rewardID format (expected \"{{dungeonID}}_{{worldModPhase}}\"): {rewardID}");

            var wearableDropTableID = Backend.Content["wearableDropTableID"].ToString();
            var wearableTypeMainStatTableID = Backend.Content["wearableTypeMainStatTableID"].ToString();
            var wearableSubStatTableID = Backend.Content["wearableSubStatTableID"].ToString();

            var dropResult = DungeonWearableDropTable.Set(wearableDropTableID);
            if (!string.IsNullOrEmpty(dropResult))
                return ReturnObject.Error(dropResult);

            var rewardTableResult = DungeonRewardTable.Set(rewardTableID);
            if (!string.IsNullOrEmpty(rewardTableResult))
                return ReturnObject.Error(rewardTableResult);

            var mainStatResult = WearableTypeMainTable.Set(wearableTypeMainStatTableID);
            if (!string.IsNullOrEmpty(mainStatResult))
                return ReturnObject.Error(mainStatResult);

            var configResult = InventoryConfigTable.Set(inventoryConfigTableID);
            if (!string.IsNullOrEmpty(configResult))
                return ReturnObject.Error(configResult);

            if (!DungeonWearableDropTable.Entries.TryGetValue(worldModPhase, out var dropEntries) || dropEntries.Count == 0)
                return ReturnObject.Error($"No drop entries for worldModPhase {worldModPhase}");

            if (!DungeonRewardTable.Entries.TryGetValue(rewardID, out var candidates) || candidates.Count == 0)
                return ReturnObject.Error($"No reward candidates for rewardID {rewardID}");

            foreach (var c in candidates)
            {
                if (!ItemID.IsWearable(c.ItemID) && !ItemID.IsExp(c.ItemID))
                    return ReturnObject.Error(5001, $"Non-wearable/exp item mixed in reward pool: {c.ItemID}");
            }

            var wearableCands = candidates.Where(c => ItemID.IsWearable(c.ItemID)).ToList();
            var expCands = candidates.Where(c => ItemID.IsExp(c.ItemID)).ToList();
            if (wearableCands.Count == 0)
                return ReturnObject.Error(5000, $"No wearable candidates for rewardID {rewardID}");

            var rand = new Random();
            var rarityCounts = new List<(int rarity, int count)>(dropEntries.Count);
            int totalCount = 0;
            int totalSubStatCount = 0;
            foreach (var drop in dropEntries)
            {
                int count = rand.Next(drop.Min, drop.Max + 1);
                rarityCounts.Add((drop.Rarity, count));
                totalCount += count;
                totalSubStatCount += GetSubStatCount(drop.Rarity) * count;
            }

            var invenBro = Backend.GameData.GetMyData("Inventory_Equip", new Where());
            if (!invenBro.IsSuccess())
                return ReturnObject.Error($"Get inventory failed: {invenBro.ErrorCode}");

            var invenJsonData = invenBro.FlattenRows()[0];
            var ownedItems = JsonConvert.DeserializeObject<Dictionary<long, string>>(invenJsonData["wearables"].ToJson());

            if (ownedItems.Count + totalCount > InventoryConfigTable.GetValue("Wearable"))
                return ReturnObject.Error(6004, "No free slot available in inventory");

            var rewardItemIDs = new List<int>(totalCount);
            var rewardRarities = new List<int>(totalCount);
            var mainStatCountDic = new Dictionary<string, int>();
            foreach (var (rarity, count) in rarityCounts)
            {
                for (int i = 0; i < count; i++)
                {
                    int baseID = wearableCands[rand.Next(wearableCands.Count)].ItemID;
                    int itemID = baseID + rarity * 1000;
                    string subTypeName = ItemID.GetEquipSubTypeName(itemID);
                    if (!mainStatCountDic.ContainsKey(subTypeName))
                        mainStatCountDic.Add(subTypeName, 0);
                    mainStatCountDic[subTypeName]++;
                    rewardItemIDs.Add(itemID);
                    rewardRarities.Add(rarity);
                }
            }

            var newItemMainStatDic = new Dictionary<string, List<string>>();
            foreach (var pair in mainStatCountDic)
            {
                for (int i = 0; i < pair.Value; i++)
                {
                    var mainStats = WearableTypeMainTable.Entries[pair.Key];
                    string targetMainStat = mainStats[rand.Next(0, mainStats.Count)];
                    if (!newItemMainStatDic.ContainsKey(pair.Key))
                        newItemMainStatDic.Add(pair.Key, new List<string>());
                    newItemMainStatDic[pair.Key].Add(targetMainStat);
                }
            }

            var subBro = Backend.Probability.GetProbabilitys(wearableSubStatTableID, totalSubStatCount);
            if (!subBro.IsSuccess())
                return ReturnObject.Error("Get subStat probabilitys failed" + subBro.ToString());

            var subJsonData = subBro.GetFlattenJSON()["elements"];
            var rewardResult = new InventoryChangeResult();
            int subOffset = 0;
            for (int i = 0; i < totalCount; i++)
            {
                int itemID = rewardItemIDs[i];
                int rarity = rewardRarities[i];
                string subTypeName = ItemID.GetEquipSubTypeName(itemID);
                bool isLock = ItemID.IsRarityLocked(itemID);

                var subStats = new List<BonusStat>();
                int subCount = GetSubStatCount(rarity);
                for (int j = 0; j < subCount; j++)
                {
                    string statType = subJsonData[subOffset + j]["Type"].ToString();
                    subStats.Add(new BonusStat(statType));
                }
                subOffset += subCount;

                string mainStatType = newItemMainStatDic[subTypeName][0];
                newItemMainStatDic[subTypeName].RemoveAt(0);
                string subStatStr = JsonConvert.SerializeObject(subStats);
                string newItem = $"{itemID}|0|{isLock}|{mainStatType}|{subStatStr}";
                long newID = DateTime.UtcNow.Ticks + i;
                ownedItems.Add(newID, newItem);
                rewardResult.EquipItemDic.Add(newID, newItem);
            }

            var param = new Param();
            param.Add("wearables", ownedItems);
            WriteBatcher.EnqueueWrite("Inventory_Equip", param);

            long gainedExp = 0;
            if (expCands.Count > 0)
            {
                var userBro = Backend.GameData.GetMyData("UserData", new Where());
                if (!userBro.IsSuccess())
                    return ReturnObject.Error($"Get UserData failed: {userBro.ErrorCode}");

                var userData = JsonConvert.DeserializeObject<UserData>(userBro.FlattenRows()[0]["userData"].ToJson());
                foreach (var c in expCands)
                {
                    int amt = rand.Next(c.Min, c.Max + 1);
                    gainedExp += amt;
                    userData.TotalExp += amt;
                }

                var userParam = new Param();
                userParam.Add("userData", userData);
                WriteBatcher.EnqueueWrite("UserData", userParam);
            }

            var writeResult = WriteBatcher.Flush();
            if (!string.IsNullOrEmpty(writeResult))
                return ReturnObject.Error(writeResult);

            var result = new RewardResult { Inventory = rewardResult, GainedExp = gainedExp };
            return ReturnObject.Success(JsonConvert.SerializeObject(result));
        }

        private static int GetSubStatCount(int rarity)
        {
            return rarity switch
            {
                2 => 4, // S
                1 => 3, // A
                0 => 2, // B
                _ => 0
            };
        }
    }
}
