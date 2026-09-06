using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using BackEnd.BackndLitJson;
using UnityEngine;

public class ItemTable
{
    public Dictionary<int, ItemData> ItemDataDic = new();
    public List<ItemData> CharacterExpItems { get; private set; } = new();
    public Dictionary<int, WeaponItemEntry> WeaponItemDic = new();
    public Dictionary<string, WearableSetsEffectEntry> WearableSetsEffectDic = new();
    public Dictionary<EItemType, List<WearableTypeMainEntry>> WearableTypeMainDic = new();
    public Dictionary<EBonusStatType, WearableSubStatEntry> WearableSubStatDic = new();
    public Dictionary<string, List<WeaponGrowthCurveEntry>> WeaponGrowthDic = new();
    public Dictionary<string, Dictionary<int, int>> WeaponMaxLevelPerPhase = new();
    public List<EquipExpEntry> WeaponExpEntries;
    public List<EquipExpEntry> WearableExpEntries;

    private List<ItemBaseEntry> m_ItemBaseList;

    public void SetItemBaseEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        m_ItemBaseList = new List<ItemBaseEntry>();
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new ItemBaseEntry();
            var entryJson = jsonData[i];
            entry.ItemID = int.Parse(entryJson["ItemID"].ToString());
            entry.Type = Enum.Parse<EItemType>(entryJson["Type"].ToString());
            entry.NameKey = entryJson["Name"].ToString();
            entry.FlavorTextKey = entryJson["FlavorText"].ToString();
            entry.SpriteAddress = entryJson["SpriteAddress"].ToString();
            string dropSources = entryJson["DropSources"].ToString();
            entry.DropSources = string.IsNullOrEmpty(dropSources)
                ? Array.Empty<string>()
                : dropSources.Split(';');
            entry.Param = entryJson["Param"].ToString();
            m_ItemBaseList.Add(entry);
        }
    }

    public void SetWeaponItemEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new WeaponItemEntry();
            var entryJson = jsonData[i];
            entry.ItemID = int.Parse(entryJson["ItemID"].ToString());
            entry.CurveID = entryJson["CurveID"].ToString();
            entry.SubStatType = (EBonusStatType)Enum.Parse(typeof(EBonusStatType), entryJson["SubStatType"].ToString());
            entry.Effects = entryJson["Effects"].ToString().Split(';');
            WeaponItemDic[entry.ItemID] = entry;
        }
    }

    public void SetWeaponGrowthCurveEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new WeaponGrowthCurveEntry();
            var entryJson = jsonData[i];
            entry.CurveID = entryJson["CurveID"].ToString();
            entry.ModPhase = int.Parse(entryJson["ModPhase"].ToString());
            entry.StartLevel = int.Parse(entryJson["StartLevel"].ToString());
            entry.EndLevel = int.Parse(entryJson["EndLevel"].ToString());
            entry.StartATK = int.Parse(entryJson["StartATK"].ToString());
            entry.EndATK = int.Parse(entryJson["EndATK"].ToString());
            entry.ATKPercent = float.Parse(entryJson["ATKPercent"].ToString());
            entry.CRITRate = float.Parse(entryJson["CRITRate"].ToString());
            entry.DEFPercent = float.Parse(entryJson["DEFPercent"].ToString());
            entry.EnergyRegen = float.Parse(entryJson["EnergyRegen"].ToString());
            entry.HPPercent = float.Parse(entryJson["HPPercent"].ToString());
            entry.MPPercent = float.Parse(entryJson["MPPercent"].ToString());

            if (!WeaponGrowthDic.ContainsKey(entry.CurveID))
                WeaponGrowthDic[entry.CurveID] = new List<WeaponGrowthCurveEntry>();
            WeaponGrowthDic[entry.CurveID].Add(entry);

            if (!WeaponMaxLevelPerPhase.TryGetValue(entry.CurveID, out var phaseDic))
            {
                phaseDic = new Dictionary<int, int>();
                WeaponMaxLevelPerPhase[entry.CurveID] = phaseDic;
            }
            if (!phaseDic.TryGetValue(entry.ModPhase, out int cur) || entry.EndLevel > cur)
                phaseDic[entry.ModPhase] = entry.EndLevel;
        }
    }

    public void SetWeaponExpEntry(string json)
    {
        WeaponExpEntries = JsonConvert.DeserializeObject<List<EquipExpEntry>>(json);
    }

    public void SetWearableExpEntry(string json)
    {
        WearableExpEntries = JsonConvert.DeserializeObject<List<EquipExpEntry>>(json);
    }


    public void SetWearableSetsEffectEntry(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new WearableSetsEffectEntry();
            var entryJson = jsonData[i];
            entry.SetsID = entryJson["SetsID"].ToString();
            entry.SetsName = entryJson["SetsName"].ToString();
            entry.Effects = entryJson["Effects"].ToString().Split(';');
            //Effects와 인덱스로 대응한다. 상시 효과도 None을 명시해 칸을 비우지 않는다.
            entry.TriggerTypes = entryJson["TriggerTypes"].ToString().Split(';')
                .Select(Enum.Parse<EffectTriggerType>).ToArray();
            WearableSetsEffectDic[entry.SetsID] = entry;
        }
    }

    public void SetWearableTypeMain(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new WearableTypeMainEntry();
            var entryJson = jsonData[i];
            entry.ItemType = Enum.Parse<EItemType>(entryJson["Type"].ToString());
            entry.MainStat = Enum.Parse<EBonusStatType>(entryJson["MainStat"].ToString());
            entry.RarityStartValues = entryJson["RarityStartValues"].ToString().Split(';')
                .Select(float.Parse).ToArray();
            entry.RarityGrowthValues = entryJson["RarityGrowthValues"].ToString().Split(';')
                .Select(float.Parse).ToArray();
            if (!WearableTypeMainDic.ContainsKey(entry.ItemType))
                WearableTypeMainDic[entry.ItemType] = new List<WearableTypeMainEntry>();
            WearableTypeMainDic[entry.ItemType].Add(entry);
        }
    }

    public void SetWearableSubStat(string json)
    {
        var jsonData = JsonMapper.ToObject(json);
        for (int i = 0; i < jsonData.Count; i++)
        {
            var entry = new WearableSubStatEntry();
            var entryJson = jsonData[i];
            entry.Type = Enum.Parse<EBonusStatType>(entryJson["Type"].ToString());
            entry.RarityGrowthValues = entryJson["RarityGrowthValues"].ToString().Split(';')
                .Select(float.Parse).ToArray();
            WearableSubStatDic[entry.Type] = entry;
        }
    }

    public void Build(Dictionary<string, EffectEntry> effectDic)
    {
        foreach (var baseEntry in m_ItemBaseList)
        {
            ItemData itemData;

            if ((baseEntry.Type & EItemType.Weapon) != 0)
            {
                WeaponItemEntry weaponEntry = WeaponItemDic[baseEntry.ItemID];
                var effects = new EffectEntry[weaponEntry.Effects.Length];
                for (int i = 0; i < weaponEntry.Effects.Length; i++)
                {
                    if (effectDic.TryGetValue(weaponEntry.Effects[i], out var effect))
                        effects[i] = effect;
                }

                itemData = new WeaponItemData(baseEntry, weaponEntry, effects);
            }
            else if ((baseEntry.Type & EItemType.Wearable) != 0)
            {
                WearableSetsEffectEntry setsEffect = null;
                EffectEntry[] effects = null;
                if (!string.IsNullOrEmpty(baseEntry.Param)
                    && WearableSetsEffectDic.TryGetValue(baseEntry.Param, out setsEffect))
                {
                    effects = new EffectEntry[setsEffect.Effects.Length];
                    for (int i = 0; i < setsEffect.Effects.Length; i++)
                    {
                        if (effectDic.TryGetValue(setsEffect.Effects[i], out var effect))
                            effects[i] = effect;
                    }

                    setsEffect.TotalCount++;
                }
                itemData = new WearableItemData(baseEntry, setsEffect, effects);
            }
            else if ((baseEntry.Type & EItemType.Consumable) != 0)
            {
                effectDic.TryGetValue(baseEntry.Param, out var effectEntry);
                itemData = new ConsumableItemData(baseEntry, effectEntry);
            }
            else
            {
                itemData = new ItemData(baseEntry);
            }

            ItemDataDic[baseEntry.ItemID] = itemData;
        }

        m_ItemBaseList.Clear();
        m_ItemBaseList = null;

        foreach (var itemData in ItemDataDic.Values)
        {
            if (itemData.ExpAmount > 0)
                CharacterExpItems.Add(itemData);
        }
        CharacterExpItems.Sort((x, y) => x.Base.Rarity.CompareTo(y.Base.Rarity));
    }

    /// <summary>
    /// wearable은 동일 장비가 여러 등급으로 드랍되어 인스턴스 ID에 등급이 실려 있고,
    /// ItemBaseTable에는 등급 0인 원본 한 행만 존재하므로 등급 자릿수를 죽여 조회한다.
    /// 그 외 아이템은 등급별로 별개 행이므로 ID를 그대로 쓴다.
    /// </summary>
    public ItemData GetItemData(int itemID)
    {
        int key = ItemID.IsWearable(itemID) ? ItemID.GetBaseID(itemID) : itemID;
        return ItemDataDic[key];
    }

    public (int baseATK, float subStatValue) GetWeaponStat(string curveID, EBonusStatType subStatType, int level, int modPhase)
    {
        if (WeaponGrowthDic.TryGetValue(curveID, out var curves))
        {
            var curve = curves.Find(c => c.ModPhase == modPhase && c.StartLevel <= level && c.EndLevel >= level);
            if (curve != null)
            {
                float t = (level - curve.StartLevel) / (float)(curve.EndLevel - curve.StartLevel);
                int baseATK = Mathf.RoundToInt(Mathf.Lerp(curve.StartATK, curve.EndATK, t));

                float subStatValue = 0f;
                switch (subStatType)
                {
                    case EBonusStatType.ATKPercent:
                        subStatValue = curve.ATKPercent;
                        break;
                    case EBonusStatType.DEFPercent:
                        subStatValue = curve.DEFPercent;
                        break;
                    case EBonusStatType.HPPercent:
                        subStatValue = curve.HPPercent;
                        break;
                    case EBonusStatType.MPPercent:
                        subStatValue = curve.MPPercent;
                        break;
                    case EBonusStatType.CRITRate:
                        subStatValue = curve.CRITRate;
                        break;
                    case EBonusStatType.EnergyRegen:
                        subStatValue = curve.EnergyRegen;
                        break;
                }
                return (baseATK, subStatValue);
            }
            return (0, 0);
        }
        return (0, 0);
    }

    public void Clear()
    {
        ItemDataDic.Clear();
        CharacterExpItems.Clear();
        WeaponItemDic.Clear();
        WeaponGrowthDic.Clear();
        WeaponMaxLevelPerPhase.Clear();
        WearableSetsEffectDic.Clear();
        WearableTypeMainDic.Clear();
    }

}
