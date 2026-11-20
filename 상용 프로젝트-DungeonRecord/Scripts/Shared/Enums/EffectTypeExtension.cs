using System.Collections.Generic;

public static class EffectTypeExtension
{
    private const string TURN_DURATION_KEY = "COMMON_TURN_DURATION";

    private static readonly Dictionary<EffectType, EBonusStatType> STAT_MAP = new()
    {
        { EffectType.Buff_ATKPercent, EBonusStatType.ATKPercent },
        { EffectType.Buff_ATKFlat, EBonusStatType.ATKFlat },
        { EffectType.Buff_DEFPercent, EBonusStatType.DEFPercent },
        { EffectType.Buff_DEFFlat, EBonusStatType.DEFFlat },
        { EffectType.Buff_SpeedFlat, EBonusStatType.SpeedFlat },
        { EffectType.Buff_CRITRate, EBonusStatType.CRITRate },
        { EffectType.Buff_StunResistRate, EBonusStatType.StunResistRate },

        { EffectType.Debuff_ATKPercent, EBonusStatType.ATKPercent },
        { EffectType.Debuff_ATKFlat, EBonusStatType.ATKFlat },
        { EffectType.Debuff_DEFPercent, EBonusStatType.DEFPercent },
        { EffectType.Debuff_DEFFlat, EBonusStatType.DEFFlat },
        { EffectType.Debuff_SpeedFlat, EBonusStatType.SpeedFlat },
        { EffectType.Debuff_CRITRate, EBonusStatType.CRITRate },
    };

    private static readonly HashSet<EffectType> DEBUFF_SET = new()
    {
        EffectType.Debuff_ATKPercent,
        EffectType.Debuff_ATKFlat,
        EffectType.Debuff_DEFPercent,
        EffectType.Debuff_DEFFlat,
        EffectType.Debuff_SpeedFlat,
        EffectType.Debuff_CRITRate,
    };

    private static readonly HashSet<EffectType> PERCENT_VALUE_SET = new()
    {
        EffectType.Hp_Recovery_Percent,
        EffectType.Mp_Recovery_Percent,
        EffectType.Hp_Loss_Percent,
    };

    private static readonly Dictionary<EBonusStatType, string> ICON_KEY_MAP = new()
    {
        { EBonusStatType.ATKPercent, "ATK" },
        { EBonusStatType.ATKFlat, "ATK" },
        { EBonusStatType.DEFPercent, "DEF" },
        { EBonusStatType.DEFFlat, "DEF" },
        { EBonusStatType.SpeedFlat, "SPEED" },
        { EBonusStatType.CRITRate, "CRITRate" },
        { EBonusStatType.StunResistRate, "StunResistRate" },
    };

    private static readonly Dictionary<EffectType, string> STATUS_ICON_KEY_MAP = new()
    {
        { EffectType.Stun, "Stun" },
        { EffectType.Bleed, "Bleed" },
        { EffectType.Blight, "Blight" },
    };

    private static readonly Dictionary<EffectType, EBonusStatType> RESIST_MAP = new()
    {
        { EffectType.Stun, EBonusStatType.StunResistRate },
        { EffectType.Bleed, EBonusStatType.BleedResistRate },
        { EffectType.Blight, EBonusStatType.BlightResistRate },
    };

    public static bool TryGetStatType(this EffectType type, out EBonusStatType statType)
    {
        return STAT_MAP.TryGetValue(type, out statType);
    }

    public static bool TryGetResistType(this EffectType type, out EBonusStatType resistType)
    {
        return RESIST_MAP.TryGetValue(type, out resistType);
    }

    public static bool IsDebuff(this EffectType type)
    {
        return DEBUFF_SET.Contains(type);
    }

    public static bool IsPercentValue(this EffectType type)
    {
        if (PERCENT_VALUE_SET.Contains(type))
            return true;

        return STAT_MAP.TryGetValue(type, out var statType)
            && (statType & EBonusStatType.Percent) != 0;
    }

    public static string GetIconKey(this EffectType type)
    {
        if (STAT_MAP.TryGetValue(type, out var statType)
            && ICON_KEY_MAP.TryGetValue(statType, out var statKey))
        {
            return type.IsDebuff() ? $"Debuff_{statKey}" : $"Buff_{statKey}";
        }

        if (STATUS_ICON_KEY_MAP.TryGetValue(type, out var statusKey))
            return statusKey;

        return string.Empty;
    }

    public static string GetName(this EffectType type)
    {
        return Localize.Get(type.ToString());
    }

    /// <summary>스탯 보정에 더할 값. 디버프는 테이블 값이 양수여도 부호를 뒤집는다.</summary>
    public static float GetSignedValue(this EffectEntry effect)
    {
        return effect.EffectType.IsDebuff() ? -effect.Value : effect.Value;
    }

    public static float GetDisplayValue(this EffectEntry effect)
    {
        return effect.EffectType.IsPercentValue() ? effect.Value * 100f : effect.Value;
    }

    /// <summary>{0}=표시 값, {1}=Stack. Duration이 있으면 지속 턴 문구를 뒤에 붙인다.</summary>
    public static string GetDescription(this EffectEntry effect)
    {
        string desc = string.Format(effect.DescFormat, effect.GetDisplayValue(), effect.Stack);
        if (effect.Duration > 0)
            desc += " " + string.Format(Localize.Get(TURN_DURATION_KEY), effect.Duration);

        return desc;
    }
}
