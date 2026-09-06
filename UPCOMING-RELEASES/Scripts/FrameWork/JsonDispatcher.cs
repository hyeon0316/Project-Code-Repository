using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JsonDispatcher
{
    private static Dictionary<string, Action<string>> m_Handlers = new()
    {
        { "ItemBaseTable", json => BDatabase.ItemTable.SetItemBaseEntry(json) },
        { "WeaponItemTable", json => BDatabase.ItemTable.SetWeaponItemEntry(json) },
        { "WeaponExpTable", json => BDatabase.ItemTable.SetWeaponExpEntry(json) },
        { "EffectTable", json => BDatabase.SetEffectData(json) },
        { "WearableSetsEffectTable", json => BDatabase.ItemTable.SetWearableSetsEffectEntry(json) },
        { "WearableTypeMainTable", json => BDatabase.ItemTable.SetWearableTypeMain(json) },
        { "WearableExpTable", json => BDatabase.ItemTable.SetWearableExpEntry(json) },
        { "WeaponGrowthCurveTable", json => BDatabase.ItemTable.SetWeaponGrowthCurveEntry(json) },
        { "WearableSubStatTable", json => BDatabase.ItemTable.SetWearableSubStat(json) },
        { "ShopTable", json => BDatabase.SetShopEntry(json) },
        { "SkillGrowthTable", json => BDatabase.SetSkillGrowthEntry(json) },
        { "SkillLevelUpCostTable", json => BDatabase.SetSkillLevelUpCostEntry(json) },
        { "StaminaConfigTable", json => BDatabase.SetStaminaConfigEntry(json) },
        { "ExpTable", json => BDatabase.SetExpEntry(json) },
        { "CharacterExpTable", json => BDatabase.CharacterTable.SetExpEntry(json) },
        { "CharacterBaseStatTable", json => BDatabase.CharacterTable.SetBaseStatEntry(json) },
        { "CharacterStatGrowthTable", json => BDatabase.CharacterTable.SetStatGrowthEntry(json) },
        { "CharacterAscensionCostTable", json => BDatabase.CharacterTable.SetAscensionCostEntry(json) },
        { "LevelRewardTable", json => BDatabase.SetLevelRewardEntry(json) },
        { "EnemyBaseStatTable", json => BDatabase.SetEnemyBaseStatEntry(json) },
        { "EnemyStatScaleTable", json => BDatabase.SetEnemyStatScaleEntry(json) },
        { "InventoryConfigTable", json => BDatabase.SetInventoryConfigEntry(json) },
        { "MissionTable", json => BDatabase.SetMissionEntry(json) },
        { "DailyMissionRewardTable", json => BDatabase.SetDailyMissionRewardTableEntry(json) },
        { "BattlePassSeasonTable", json => BDatabase.SetBattlePassSeasonEntry(json) },
        { "BattlePassTierTable", json => BDatabase.SetBattlePassTierEntry(json) },
        { "QuestRewardTable", json => BDatabase.SetQuestRewardEntry(json) },
        { "DungeonWearableDropTable", json => BDatabase.SetDungeonWearableDropEntry(json) },
        { "DungeonRewardTable", json => BDatabase.SetDungeonRewardEntry(json) },
        { "GachaBannerTable", json => BDatabase.SetGachaBannerEntry(json) },
        { "GachaRuleTable", json => BDatabase.SetGachaRuleEntry(json) },
        { "GachaCharacterPoolTable", json => BDatabase.SetGachaCharacterPoolEntry(json) },
        { "GachaAWeaponPoolTable", json => BDatabase.SetGachaAWeaponPoolEntry(json) },
        { "GachaBWeaponPoolTable", json => BDatabase.SetGachaBWeaponPoolEntry(json) },
    };

    public static void Dispatch(string key, string json)
    {
        if (m_Handlers.TryGetValue(key, out var handler))
        {
            try
            {
                handler(json);
            }
            catch (Exception ex)
            {
                HDebug.LogError($"[JsonDispatcher] key: {key}, {ex}");
                throw;
            }
        }
        else
            Debug.LogWarning($"No handler found for key: {key}");
    }
}
