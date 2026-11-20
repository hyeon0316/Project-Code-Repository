
[System.Serializable]
public class ItemBaseEntry
{
    public int ItemID;
    public EItemType Type;
    public string NameKey;
    public string FlavorTextKey;
    public string SpriteAddress;
    public string[] DropSources;
    public string Param;

    public ERarityType Rarity => (ERarityType)global::ItemID.GetRarity(ItemID);
    public string Name => Localize.Get(NameKey);
    public string FlavorText => Localize.Get(FlavorTextKey);
}

[System.Serializable]
public class WeaponItemEntry
{
    public int ItemID;
    public string CurveID;
    public EBonusStatType SubStatType;
    public string[] Effects;
}


[System.Serializable]
public class EffectEntry
{
    public string EffectID;
    public EffectType EffectType;
    public float Value;
    public int Stack;
    public int MaxStack;
    public string DescFormatKey;

    public string DescFormat => Localize.Get(DescFormatKey);
}


[System.Serializable]
public class WeaponGrowthCurveEntry
{
    public string CurveID;
    public int ModPhase;
    public int StartLevel;
    public int EndLevel;
    public int StartATK;
    public int EndATK;
    public float ATKPercent;
    public float CRITRate;
    public float DEFPercent;
    public float EnergyRegen;
    public float HPPercent;
    public float MPPercent;
}

[System.Serializable]
public class WearableSetsEffectEntry
{
    public string SetsID;
    public string SetsName;
    public string[] Effects;
    /// <summary>Effects와 같은 길이. 인덱스가 대응하며 None이면 장착 즉시 적용되는 상시 효과.</summary>
    public EffectTriggerType[] TriggerTypes;
    public int TotalCount;
}

[System.Serializable]
public class ExpEntry
{
    public int Level;
    public long ExpToNext;
    public long TotalExp;
    public int ModPhase;
}

[System.Serializable]
public class LevelRewardEntry
{
    public int Level;
    public int ItemID;
    public int Amount;
}

[System.Serializable]
public class CharacterBaseStatEntry
{
    public ECharacterType CharacterType;
    public int HP;
    public int MP;
    public int ATK;
    public int DEF;
    public int Speed;
}

[System.Serializable]
public class CharacterStatGrowthEntry
{
    public string GrowthID;
    public int Level;
    public int ModPhase;
    public float HPScale;
    public float MPScale;
    public float ATKScale;
    public float DEFScale;
}

[System.Serializable]
public class CharacterAscensionCostEntry
{
    public string AscensionID;
    public int ModPhase;
    public int ItemID;
    public int Amount;
}

[System.Serializable]
public class EnemyBaseStatEntry
{
    public EEnemyType EnemyType;
    public int HP;
    public int ATK;
    public int DEF;
    public int Speed;
    public float CriticalRate;
    public float StunResistRate;
    public float BleedResistRate;
    public float BlightResistRate;
}

[System.Serializable]
public class EnemyStatScaleEntry
{
    public string ScalingID;
    public float HPScale;
    public float ATKScale;
    public float DEFScale;
    public float CriticalRateBonus;
    public float StunResistRateBonus;
    public float BleedResistRateBonus;
    public float BlightResistRateBonus;
}

[System.Serializable]
public class SkillGrowthEntry
{
    public string SkillID;
    public int Level;
    public float ScalingValue;
}

[System.Serializable]
public class WearableTypeMainEntry
{
    public EItemType ItemType;
    public EBonusStatType MainStat;
    public float[] RarityStartValues;
    public float[] RarityGrowthValues;
}


[System.Serializable]
public class WearableSubStatEntry
{
    public EBonusStatType Type;
    public float[] RarityGrowthValues;
}

[System.Serializable]
public class EquipExpEntry
{
    public int Level;
    public int S_ExpToNext;
    public int S_TotalExp;
    public int A_ExpToNext;
    public int A_TotalExp;
    public int B_ExpToNext;
    public int B_TotalExp;
}

[System.Serializable]
public class InventoryConfigEntry
{
    public string ConfigID;
    public int Value;
}

[System.Serializable]
public class ShopEntry
{
    /// <summary>CurrencyID가 0이면 현금 결제 상품(인앱)</summary>
    public bool IsIAP
    {
        get
        {
            return CurrencyID == 0;
        }
    }

    public EShopRefreshType RefreshType;
    public string ProductID;
    public string ShopID;
    public string ProductName;
    public string ProductSpritePath;
    public int CurrencyID;
    public int Price;
    public int LimitCount;
    public TransactionItemSpec[] RewardSpecs;
}

[System.Serializable]
public class MissionEntry
{
    public string MissionID;
    public string Name;
    public EMissionCategory Category;
    public EMissionType MissionType;
    public EGameEventType EventType;
    /// <summary>빈 문자열 = 대상 무관. 채우면 특정 대상(던전ID, 퀘스트ID 등)만 매칭.</summary>
    public string TargetID;
    public EMissionConditionType ConditionType;
    public string Objective;
    public int GoalValue;
    public int RewardValue;
}

[System.Serializable]
public class DailyMissionRewardEntry
{
    public int ScoreThreshold;
    public TransactionItemSpec[] Items;
}

[System.Serializable]
public class BattlePassSeasonEntry
{
    public string SeasonID;
    public int WeeklyMaxXp;
    public int XpPerTier;
    public System.DateTime StartDate;
    public System.DateTime EndDate;
    public int CostPerBuyLevel;
}

[System.Serializable]
public class BattlePassTierEntry
{
    public int Tier;
    public int FreeReward;
    public int FreeAmount;
    public int PremiumReward1;
    public int PremiumAmount1;
    public int PremiumReward2;
    public int PremiumAmount2;
}

[System.Serializable]
public class QuestRewardEntry
{
    public string QuestID;
    public TransactionItemSpec[] RewardSpecs;
}


[System.Serializable]
public class DungeonWearableDropEntry
{
    public ERarityType Rarity;
    public int Min;
    public int Max;
}

/// <summary>
/// 던전 클리어 보상 한 줄. 장비 아이템은 Min/Max를 쓰지 않고
/// DungeonWearableDropEntry의 WorldModPhase 구간을 따라 수량이 결정된다.
/// </summary>
[System.Serializable]
public class DungeonRewardEntry
{
    public int ItemID;
    public int Min;
    public int Max;
}

[System.Serializable]
public class GachaBannerEntry
{
    public int BannerID;
    public System.DateTime StartDate;
    public System.DateTime EndDate;
    public string PickupCharacterID;
}

[System.Serializable]
public class GachaRuleEntry
{
    public float MaxPity;
    public float BaseRate;
    public int SoftPityStartCount;
    public float RateIncreasePerCount;
    public float OffBannerRate;
    public int DupeMileageAmount;
    public int OverflowMileageAmount;
    public float AWeaponRate;
    public int AWeaponMileageAmount;
    public int WeaponPityMax;
}
