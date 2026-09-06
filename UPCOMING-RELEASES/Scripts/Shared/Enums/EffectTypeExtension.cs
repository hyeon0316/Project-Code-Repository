using System.Collections.Generic;

public static class EffectTypeExtension
{
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

    private static readonly Dictionary<EBonusStatType, string> ICON_KEY_MAP = new()
    {
        { EBonusStatType.ATKPercent, "ATK" },
        { EBonusStatType.ATKFlat, "ATK" },
        { EBonusStatType.DEFPercent, "DEF" },
        { EBonusStatType.DEFFlat, "DEF" },
        { EBonusStatType.SpeedFlat, "Speed" },
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

    public static bool IsTriggerDot(this EffectType type)
    {
        return type == EffectType.Bleed || type == EffectType.Blight;
    }

    /// <summary>
    /// 스택 상한. Bleed/Blight는 모든 효과가 공통 상수를 공유하고,
    /// Stun은 풀리기 전 재부여가 막혀 있어 중첩되지 않으므로 Stack이 곧 상한이다.
    /// 그 외는 테이블의 MaxStack을 따른다. 0이면 무한 중첩.
    /// </summary>
    public static int GetMaxStack(this EffectEntry effect)
    {
        switch (effect.EffectType)
        {
            case EffectType.Bleed:
                return GlobalVariable.MAX_STACK_BLEED;
            case EffectType.Blight:
                return GlobalVariable.MAX_STACK_BLIGHT;
            case EffectType.Stun:
                return effect.Stack;
            default:
                return effect.MaxStack;
        }
    }

    public static bool IsPercentValue(this EffectType type)
    {
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

    public static float GetDisplayValue(this EffectEntry effect)
    {
        return effect.EffectType.IsPercentValue() ? effect.Value * 100f : effect.Value;
    }
}
