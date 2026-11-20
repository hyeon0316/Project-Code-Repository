using System;

[Flags]
public enum EBonusStatType
{
    HPPercent = 1 << 0,
    HPFlat = 1 << 1,
    MPPercent = 1 << 2,
    MPFlat = 1 << 3,
    ATKPercent = 1 << 4,
    ATKFlat = 1 << 5,
    DEFPercent = 1 << 6,
    DEFFlat = 1 << 7,
    SpeedFlat = 1 << 8,
    EnergyRegen = 1 << 9,
    CRITRate = 1 << 10,
    StunResistRate = 1 << 11,
    BleedResistRate = 1 << 12,
    BlightResistRate = 1 << 13,

    Percent = HPPercent | MPPercent | ATKPercent | DEFPercent | EnergyRegen
        | CRITRate | StunResistRate | BleedResistRate | BlightResistRate,
}
