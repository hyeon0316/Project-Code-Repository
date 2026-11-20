using BackEnd.Content;
using BackEnd;
using BackEnd.ProbabilityContent;
using Cysharp.Threading.Tasks;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class BDatabase
{
    /// <summary>
    /// 서버 검증에 쓰이지 않는 클라 전용 테이블. CDN이 아닌 Addressables로 로드한다.
    /// Addressables 주소와 JsonDispatcher 키가 동일하다.
    /// </summary>
    private static readonly string[] LOCAL_TABLE_KEYS =
    {
        "EffectTable",
        "CharacterBaseStatTable",
        "CharacterStatGrowthTable",
        "SkillGrowthTable",
        "WeaponGrowthCurveTable",
        "EnemyBaseStatTable",
        "EnemyStatScaleTable",
        "WearableSetsEffectTable",
        "WeaponItemTable",
    };

    public static Dictionary<string, string> TableIDDic = new();
    public static ItemTable ItemTable = new();
    public static CharacterTable CharacterTable = new();
    public static Dictionary<string, List<ShopEntry>> ShopEntryDic = new();
    public static BattlePassSeasonEntry BattlePassSeaonEntry;
    public static List<BattlePassTierEntry> BattlePassTiers = new();
    public static Dictionary<string, List<SkillGrowthEntry>> SkillGrowthDic = new();
    public static Dictionary<string, Dictionary<int, List<TransactionItemSpec>>> SkillLevelUpCostDic = new();
    public static Dictionary<string, EffectEntry> EffectDic = new();
    public static Dictionary<EEnemyType, EnemyBaseStatEntry> EnemyBaseStatDic = new();
    public static Dictionary<string, EnemyStatScaleEntry> EnemyStatScaleDic = new();
    public static StaminaConfigEntry StaminaConfig = new();
    public static List<ExpEntry> ExpEntries;
    public static Dictionary<int, List<LevelRewardEntry>> LevelRewardDic = new();
    public static List<InventoryConfigEntry> InventoryConfigEntries;
    public static Dictionary<string, MissionEntry> MissionDic = new();
    public static List<DailyMissionRewardEntry> DailyMissionRewards = new();
    public static Dictionary<string, TransactionItemSpec[]> QuestRewardDic = new();
    public static Dictionary<int, List<DungeonWearableDropEntry>> DungeonWearableDropDic = new();
    public static Dictionary<string, List<DungeonRewardEntry>> DungeonRewardDic = new();
    public static GachaBannerEntry GachaBannerEntry;
    public static GachaRuleEntry GachaRuleEntry;
    public static List<string> GachaCharacterPool = new();
    public static List<int> GachaAWeaponPool = new();
    public static List<int> GachaBWeaponPool = new();

    private static bool m_IsInit = false;

    public static async UniTask Init()
    {
        if (m_IsInit)
            return;

        BackendContentTableReturnObject tableCallback = null;
        Backend.CDN.Content.Table.Get(callback =>
        {
            tableCallback = callback;
        });
        await UniTask.WaitUntil(() => tableCallback != null);
        if (!tableCallback.IsSuccess())
        {
            HDebug.LogError(tableCallback.ToString());
            throw new Exception($"CDN Table load failed: {tableCallback.GetErrorCode()}");
        }

        BackendContentReturnObject bro = null;
        Backend.CDN.Content.Local.Update(tableCallback.GetContentTableItemList(), null, callback =>
        {
            bro = callback;
        });
        await UniTask.WaitUntil(() => bro != null);
        if (!bro.IsSuccess())
        {
            HDebug.LogError(bro.ToString());
            throw new Exception($"CDN Content load failed: {bro.GetErrorCode()}");
        }

        var items = bro.GetContentList();
        foreach (var item in items)
        {
            TableIDDic[item.chartName] = item.selectedChartFileId;
            JsonDispatcher.Dispatch(item.chartName, item.contentString);
        }

        await LoadLocalTable();
        await SetProbability();

        BuildData();
        m_IsInit = true;
    }

    /// <summary>
    /// 클라 전용 테이블을 Addressables로 로드한다.
    /// ItemTable.Build가 EffectDic을 참조하므로 BuildData()보다 먼저 완료돼야 한다.
    /// </summary>
    private static async UniTask LoadLocalTable()
    {
        var tasks = new UniTask<string>[LOCAL_TABLE_KEYS.Length];
        for (int i = 0; i < LOCAL_TABLE_KEYS.Length; i++)
        {
            tasks[i] = LoadLocalTableText(LOCAL_TABLE_KEYS[i]);
        }

        var texts = await UniTask.WhenAll(tasks);

        for (int i = 0; i < LOCAL_TABLE_KEYS.Length; i++)
        {
            if (texts[i] == null)
                throw new Exception($"Local table load failed: {LOCAL_TABLE_KEYS[i]}");

            JsonDispatcher.Dispatch(LOCAL_TABLE_KEYS[i], texts[i]);
        }
    }

    private static async UniTask<string> LoadLocalTableText(string key)
    {
        var handle = Addressables.LoadAssetAsync<TextAsset>(key);
        try
        {
            var textAsset = await handle.Task;
            return textAsset != null ? textAsset.text : null;
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    private static async UniTask SetProbability()
    {
        BackendProbabilityTableReturnObject bro = null;
        Backend.CDN.Probability.Table.Get(callback =>
        {
            bro = callback;
        });
        await UniTask.WaitUntil(() => bro != null);
        if (!bro.IsSuccess())
        {
            HDebug.LogError(bro.ToString());
            throw new Exception($"Probability Table load failed: {bro.GetErrorCode()}");
        }

        BackendProbabilityContentReturnObject bro2 = null;
        Backend.CDN.Probability.Get(bro.GetProbabilityTableItemList(), null, callback =>
        {
            bro2 = callback;
        });
        await UniTask.WaitUntil(() => bro2 != null);
        if (!bro2.IsSuccess())
        {
            HDebug.LogError(bro2.ToString());
            throw new Exception($"Probability Content load failed: {bro2.GetErrorCode()}");
        }

        var dic = bro2.GetProbabilityContentDictionarySortByProbabilityName();
        bool hasError = false;
        foreach (var item in dic)
        {
            TableIDDic[item.Key] = item.Value.selectedProbabilityFileId;
            try
            {
                JsonDispatcher.Dispatch(item.Key, item.Value.contentString);
            }
            catch
            {
                hasError = true;
            }
        }

        if (hasError)
            throw new Exception("One or more probability tables failed to initialize.");
    }

    public static void SetEffectData(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new EffectEntry();
            var entryJson = jsonData[i];
            entry.EffectID = entryJson["EffectID"].ToString();
            entry.EffectType = Enum.Parse<EffectType>(entryJson["EffectType"].ToString());
            entry.Value = float.Parse(entryJson["Value"].ToString());
            entry.Stack = int.Parse(entryJson["Stack"].ToString());
            entry.MaxStack = int.Parse(entryJson["MaxStack"].ToString());
            entry.DescFormatKey = entryJson["DescFormatKey"].ToString();
            EffectDic[entry.EffectID] = entry;
        }
    }

    public static void SetShopEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            string shopID = jsonData[i]["ShopID"].ToString();
            if (!ShopEntryDic.TryGetValue(shopID, out var list))
            {
                list = new List<ShopEntry>();
                ShopEntryDic[shopID] = list;
            }
            var shopEntry = new ShopEntry();
            shopEntry.ShopID = shopID;
            shopEntry.ProductID = jsonData[i]["ProductID"].ToString();
            shopEntry.ProductName = jsonData[i]["ProductName"].ToString();
            shopEntry.ProductSpritePath = jsonData[i]["ProductSpritePath"].ToString();
            shopEntry.CurrencyID = jsonData[i]["CurrencyID"].ToString() != "" ? int.Parse(jsonData[i]["CurrencyID"].ToString()) : 0;
            shopEntry.Price = jsonData[i]["Price"].ToString() != "" ? int.Parse(jsonData[i]["Price"].ToString()) : 0;
            shopEntry.LimitCount = jsonData[i]["LimitCount"].ToString() != "" ? int.Parse(jsonData[i]["LimitCount"].ToString()) : 0;
            shopEntry.RefreshType = Enum.Parse<EShopRefreshType>(jsonData[i]["RefreshType"].ToString());
            string rewardSpecsStr = jsonData[i]["RewardSpecs"].ToString();
            if (string.IsNullOrEmpty(rewardSpecsStr))
            {
                shopEntry.RewardSpecs = Array.Empty<TransactionItemSpec>();
            }
            else
            {
                var parts = rewardSpecsStr.Split(';');
                shopEntry.RewardSpecs = new TransactionItemSpec[parts.Length];
                for (int j = 0; j < parts.Length; j++)
                    shopEntry.RewardSpecs[j] =
                JsonConvert.DeserializeObject<TransactionItemSpec>(parts[j]);
            }
            list.Add(shopEntry);
        }
    }

    public static void SetSkillGrowthEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new SkillGrowthEntry();
            entry.SkillID = jsonData[i]["SkillID"].ToString();
            entry.Level = int.Parse(jsonData[i]["Level"].ToString());
            entry.ScalingValue = float.Parse(jsonData[i]["ScalingValue"].ToString());

            if (!SkillGrowthDic.TryGetValue(entry.SkillID, out var growths))
            {
                growths = new List<SkillGrowthEntry>();
                SkillGrowthDic[entry.SkillID] = growths;
            }

            SkillGrowthDic[entry.SkillID].Add(entry);
        }
    }

    public static void SetSkillLevelUpCostEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            string id = jsonData[i]["CostID"].ToString();
            int level = int.Parse(jsonData[i]["Level"].ToString());
            int itemID = int.Parse(jsonData[i]["ItemID"].ToString());
            int amount = int.Parse(jsonData[i]["Amount"].ToString());

            if (!SkillLevelUpCostDic.TryGetValue(id, out var levelDic))
            {
                levelDic = new Dictionary<int, List<TransactionItemSpec>>();
                SkillLevelUpCostDic[id] = levelDic;
            }

            if (!levelDic.TryGetValue(level, out var itemList))
            {
                itemList = new List<TransactionItemSpec>();
                levelDic[level] = itemList;
            }

            itemList.Add(new TransactionItemSpec { ID = itemID, Amount = amount });
        }
    }

    public static void SetStaminaConfigEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        if (jsonData.Count > 0)
        {
            StaminaConfig.MaxStamina = int.Parse(jsonData[0]["MaxStamina"].ToString());
            StaminaConfig.RecoveryIntervalSeconds = int.Parse(jsonData[0]["RecoveryIntervalSeconds"].ToString());
            StaminaConfig.GemRechargeCost = int.Parse(jsonData[0]["GemRechargeCost"].ToString());
            StaminaConfig.GemRechargeAmount = int.Parse(jsonData[0]["GemRechargeAmount"].ToString());
            StaminaConfig.MaxDailyGemRechargeCount = int.Parse(jsonData[0]["MaxDailyGemRechargeCount"].ToString());
        }
    }

    public static void SetExpEntry(string json)
    {
        ExpEntries = JsonConvert.DeserializeObject<List<ExpEntry>>(json);
    }

    public static void SetLevelRewardEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new LevelRewardEntry();
            entry.Level = int.Parse(jsonData[i]["Level"].ToString());
            entry.ItemID = int.Parse(jsonData[i]["ItemID"].ToString());
            entry.Amount = int.Parse(jsonData[i]["Amount"].ToString());

            if (!LevelRewardDic.TryGetValue(entry.Level, out var list))
            {
                list = new List<LevelRewardEntry>();
                LevelRewardDic[entry.Level] = list;
            }
            list.Add(entry);
        }
    }

    public static void SetEnemyBaseStatEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new EnemyBaseStatEntry();
            entry.EnemyType = Enum.Parse<EEnemyType>(jsonData[i]["EnemyType"].ToString());
            entry.HP = int.Parse(jsonData[i]["HP"].ToString());
            entry.ATK = int.Parse(jsonData[i]["ATK"].ToString());
            entry.DEF = int.Parse(jsonData[i]["DEF"].ToString());
            entry.Speed = int.Parse(jsonData[i]["Speed"].ToString());
            entry.CriticalRate = float.Parse(jsonData[i]["CriticalRate"].ToString());
            entry.StunResistRate = float.Parse(jsonData[i]["StunResistRate"].ToString());
            entry.BleedResistRate = float.Parse(jsonData[i]["BleedResistRate"].ToString());
            entry.BlightResistRate = float.Parse(jsonData[i]["BlightResistRate"].ToString());
            EnemyBaseStatDic[entry.EnemyType] = entry;
        }
    }

    public static void SetEnemyStatScaleEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new EnemyStatScaleEntry();
            entry.ScalingID = jsonData[i]["ScalingID"].ToString();
            entry.HPScale = float.Parse(jsonData[i]["HPScale"].ToString());
            entry.ATKScale = float.Parse(jsonData[i]["ATKScale"].ToString());
            entry.DEFScale = float.Parse(jsonData[i]["DEFScale"].ToString());
            entry.CriticalRateBonus = float.Parse(jsonData[i]["CriticalRateBonus"].ToString());
            entry.StunResistRateBonus = float.Parse(jsonData[i]["StunResistRateBonus"].ToString());
            entry.BleedResistRateBonus = float.Parse(jsonData[i]["BleedResistRateBonus"].ToString());
            entry.BlightResistRateBonus = float.Parse(jsonData[i]["BlightResistRateBonus"].ToString());
            EnemyStatScaleDic[entry.ScalingID] = entry;
        }
    }

    public static void SetInventoryConfigEntry(string json)
    {
        InventoryConfigEntries = JsonConvert.DeserializeObject<List<InventoryConfigEntry>>(json);
    }

    public static void SetMissionEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new MissionEntry();
            entry.MissionID = jsonData[i]["MissionID"].ToString();
            entry.Name = jsonData[i]["Name"].ToString();
            entry.Category = Enum.Parse<EMissionCategory>(jsonData[i]["Category"].ToString());
            var missionTypeStr = jsonData[i]["MissionType"].ToString();
            entry.MissionType = string.IsNullOrEmpty(missionTypeStr)
                ? EMissionType.None
                : Enum.Parse<EMissionType>(missionTypeStr);
            entry.EventType = Enum.Parse<EGameEventType>(jsonData[i]["EventType"].ToString());
            entry.TargetID = jsonData[i]["TargetID"]?.ToString() ?? "";
            var conditionTypeStr = jsonData[i]["ConditionType"]?.ToString();
            entry.ConditionType = string.IsNullOrEmpty(conditionTypeStr)
                ? EMissionConditionType.None
                : Enum.Parse<EMissionConditionType>(conditionTypeStr);
            entry.Objective = jsonData[i]["Objective"].ToString();
            entry.GoalValue = int.Parse(jsonData[i]["GoalValue"].ToString());
            entry.RewardValue = int.Parse(jsonData[i]["RewardValue"].ToString());
            MissionDic[entry.MissionID] = entry;
        }
    }

    public static void SetQuestRewardEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            string questID = jsonData[i]["QuestID"].ToString();
            var parts = jsonData[i]["RewardSpecs"].ToString().Split(';');
            var specs = new TransactionItemSpec[parts.Length];
            for (int j = 0; j < parts.Length; j++)
                specs[j] = JsonConvert.DeserializeObject<TransactionItemSpec>(parts[j]);
            QuestRewardDic[questID] = specs;
        }
    }

    public static void SetDungeonWearableDropEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        DungeonWearableDropDic.Clear();
        for (int i = 0; i < jsonData.Count; i++)
        {
            int modPhase = int.Parse(jsonData[i]["WorldModPhase"].ToString());
            var entry = new DungeonWearableDropEntry();
            entry.Rarity = Enum.Parse<ERarityType>(jsonData[i]["Rarity"].ToString());
            entry.Min = int.Parse(jsonData[i]["Min"].ToString());
            entry.Max = int.Parse(jsonData[i]["Max"].ToString());

            if (!DungeonWearableDropDic.TryGetValue(modPhase, out var list))
            {
                list = new List<DungeonWearableDropEntry>();
                DungeonWearableDropDic[modPhase] = list;
            }
            list.Add(entry);
        }
    }

    public static void SetDungeonRewardEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        DungeonRewardDic.Clear();
        for (int i = 0; i < jsonData.Count; i++)
        {
            string rewardID = jsonData[i]["RewardID"].ToString();
            var entry = new DungeonRewardEntry();
            entry.ItemID = int.Parse(jsonData[i]["ItemID"].ToString());
            entry.Min = int.Parse(jsonData[i]["Min"].ToString());
            entry.Max = int.Parse(jsonData[i]["Max"].ToString());

            if (!DungeonRewardDic.TryGetValue(rewardID, out var list))
            {
                list = new List<DungeonRewardEntry>();
                DungeonRewardDic[rewardID] = list;
            }
            list.Add(entry);
        }
    }

    /// <summary>장비 보상이 섞여 있는지. 장비 롤링 테이블이 필요한지 판정하는 데 쓴다.</summary>
    public static bool HasWearableReward(string rewardID)
    {
        if (!DungeonRewardDic.TryGetValue(rewardID, out var rewards))
            return false;

        foreach (var reward in rewards)
        {
            if (ItemID.IsWearable(reward.ItemID))
                return true;
        }
        return false;
    }

    public static void SetDailyMissionRewardTableEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        DailyMissionRewards.Clear();
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new DailyMissionRewardEntry();
            entry.ScoreThreshold = int.Parse(jsonData[i]["ScoreThreshold"].ToString());
            var parts = jsonData[i]["RewardSpecs"].ToString().Split(';');
            entry.Items = new TransactionItemSpec[parts.Length];
            for (int j = 0; j < parts.Length; j++)
                entry.Items[j] = JsonConvert.DeserializeObject<TransactionItemSpec>(parts[j]);
            DailyMissionRewards.Add(entry);
        }
        DailyMissionRewards.Sort((a, b) => a.ScoreThreshold.CompareTo(b.ScoreThreshold));
    }

    public static void SetBattlePassSeasonEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        if (jsonData.Count == 0)
            return;

        var d = jsonData[0];
        BattlePassSeaonEntry = new BattlePassSeasonEntry
        {
            SeasonID = d["SeasonID"].ToString(),
            WeeklyMaxXp = int.Parse(d["WeeklyMaxXp"].ToString()),
            XpPerTier = int.Parse(d["XpPerTier"].ToString()),
            StartDate = DateTime.Parse(d["StartDate"].ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind),
            EndDate = DateTime.Parse(d["EndDate"].ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind),
            CostPerBuyLevel = int.Parse(d["CostPerBuyLevel"].ToString()),
        };
    }

    public static void SetGachaBannerEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        if (jsonData.Count == 0)
            return;

        var d = jsonData[0];
        GachaBannerEntry = new GachaBannerEntry
        {
            BannerID = int.Parse(d["BannerID"].ToString()),
            StartDate = DateTime.Parse(d["StartDate"].ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind),
            EndDate = DateTime.Parse(d["EndDate"].ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind),
            PickupCharacterID = d["PickupCharacterID"].ToString(),
        };
    }

    public static void SetGachaRuleEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        if (jsonData.Count == 0)
            return;

        var d = jsonData[0];
        GachaRuleEntry = new GachaRuleEntry
        {
            MaxPity = float.Parse(d["MaxPity"].ToString()),
            BaseRate = float.Parse(d["BaseRate"].ToString()),
            SoftPityStartCount = int.Parse(d["SoftPityStartCount"].ToString()),
            RateIncreasePerCount = float.Parse(d["RateIncreasePerCount"].ToString()),
            OffBannerRate = float.Parse(d["OffBannerRate"].ToString()),
            DupeMileageAmount = int.Parse(d["DupeMileageAmount"].ToString()),
            OverflowMileageAmount = int.Parse(d["OverflowMileageAmount"].ToString()),
            AWeaponRate = float.Parse(d["AWeaponRate"].ToString()),
            AWeaponMileageAmount = int.Parse(d["AWeaponMileageAmount"].ToString()),
            WeaponPityMax = int.Parse(d["WeaponPityMax"].ToString()),
        };
    }

    public static void SetGachaCharacterPoolEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        GachaCharacterPool.Clear();
        for (int i = 0; i < jsonData.Count; i++)
            GachaCharacterPool.Add(jsonData[i]["CharacterID"].ToString());
    }

    public static void SetGachaAWeaponPoolEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        GachaAWeaponPool.Clear();
        for (int i = 0; i < jsonData.Count; i++)
            GachaAWeaponPool.Add(int.Parse(jsonData[i]["ItemID"].ToString()));
    }

    public static void SetGachaBWeaponPoolEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        GachaBWeaponPool.Clear();
        for (int i = 0; i < jsonData.Count; i++)
            GachaBWeaponPool.Add(int.Parse(jsonData[i]["ItemID"].ToString()));
    }

    public static void SetBattlePassTierEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var d = jsonData[i];
            BattlePassTiers.Add(new BattlePassTierEntry
            {
                Tier = int.Parse(d["Tier"].ToString()),
                FreeReward = int.Parse(d["FreeReward"].ToString()),
                FreeAmount = int.Parse(d["FreeAmount"].ToString()),
                PremiumReward1 = int.Parse(d["PremiumReward1"].ToString()),
                PremiumAmount1 = int.Parse(d["PremiumAmount1"].ToString()),
                PremiumReward2 = ParseIntOrZero(d["PremiumReward2"].ToString()),
                PremiumAmount2 = ParseIntOrZero(d["PremiumAmount2"].ToString()),
            });
        }
        BattlePassTiers.Sort((a, b) => a.Tier.CompareTo(b.Tier));
    }

    private static int ParseIntOrZero(string s)
    {
        return int.TryParse(s, out var v) ? v : 0;
    }

    public static SkillGrowthEntry GetSkillGrowth(string skillID, int level)
    {
        if (SkillGrowthDic.TryGetValue(skillID, out var levels))
        {
            int levelIndex = level - 1;
            if (levelIndex >= levels.Count)
                levelIndex = levels.Count - 1;
            else if (levelIndex < 0)
                levelIndex = 0;

            return levels[levelIndex];
        }

        HDebug.LogError($"Failed to find skillGrowth. {skillID}");
        return null;
    }


    public static List<TransactionItemSpec> GetSkillLevelUpCost(string id, int level)
    {
        if (SkillLevelUpCostDic.TryGetValue(id, out var levelDic))
        {
            if (levelDic.TryGetValue(level, out var itemList))
                return itemList;
        }
        return new List<TransactionItemSpec>();
    }

    public static IReadOnlyList<LevelRewardEntry> GetLevelRewards(int level)
    {
        if (LevelRewardDic.TryGetValue(level, out var list))
            return list;

        return Array.Empty<LevelRewardEntry>();
    }

    public static EffectEntry GetEffect(string id)
    {
        if (EffectDic.TryGetValue(id, out var effect))
            return effect;

        HDebug.LogError($"Faild to find id : {id}");
        return null;
    }

    public static string GetEffectDescription(string effectID)
    {
        if (!EffectDic.TryGetValue(effectID, out var effect))
            return string.Empty;

        return string.Format(effect.DescFormat,
            effect.GetDisplayValue(),
            effect.Stack);
    }

    public static InventoryConfigEntry GetInventoryConfig(string id)
    {
        foreach (var entry in InventoryConfigEntries)
        {
            if (entry.ConfigID == id)
                return entry;
        }
        HDebug.LogError($"Failed to find inventory config. {id}");
        return null;
    }

    public static ShopEntry GetShopEntry(string shopID, string productID)
    {
        if (ShopEntryDic.TryGetValue(shopID, out var list))
        {
            var target = list.Find(p => p.ProductID == productID);
            if (target == null)
            {
                HDebug.LogError($"Failed to shopEntry. {productID}");
                return null;
            }
            return target;
        }
        HDebug.LogError($"Failed to shopEntry. {shopID}");
        return null;
    }


    public static void BuildData()
    {
        ItemTable.Build(EffectDic);
    }
}
