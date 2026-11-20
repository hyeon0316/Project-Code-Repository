using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Character
{
    public Stat MaxHp = new FlatStat();
    public Stat MaxMp = new FlatStat();
    public Stat Attack = new FlatStat();
    public Stat Defense = new FlatStat();
    public Stat Speed = new FlatStat();
    public Stat CriticalRate = new PercentageStat();
    public Stat StunResistRate = new PercentageStat();
    public Stat BleedResistRate = new PercentageStat();
    public Stat BlightResistRate = new PercentageStat();
    public Stat EnergyRegen = new PercentageStat();

    public void AddStatValue(EBonusStatType statType, float value, object source)
    {
        var valueType = (statType & EBonusStatType.Percent) != 0 ?
            EStatValueType.Percent : EStatValueType.FixedFlat;
        GetStatByType(statType).AddModifier(new StatModifier(value, valueType, source));
    }

    public void RemoveStatValue(EBonusStatType statType, object source)
    {
        GetStatByType(statType).RemoveAllModifiersFromSource(source);
    }

    public Stat GetStatByType(EBonusStatType statType)
    {
        switch (statType)
        {
            case EBonusStatType.HPPercent:
            case EBonusStatType.HPFlat:
                return MaxHp;
            case EBonusStatType.MPPercent:
            case EBonusStatType.MPFlat:
                return MaxMp;
            case EBonusStatType.ATKPercent:
            case EBonusStatType.ATKFlat:
                return Attack;
            case EBonusStatType.DEFPercent:
            case EBonusStatType.DEFFlat:
                return Defense;
            case EBonusStatType.SpeedFlat:
                return Speed;
            case EBonusStatType.EnergyRegen:
                return EnergyRegen;
            case EBonusStatType.CRITRate:
                return CriticalRate;
            case EBonusStatType.StunResistRate:
                return StunResistRate;
            case EBonusStatType.BleedResistRate:
                return BleedResistRate;
            case EBonusStatType.BlightResistRate:
                return BlightResistRate;
            default:
                {
                    HDebug.LogError($"This stat is not available. {statType}");
                    return null;
                }
        }
    }
}
