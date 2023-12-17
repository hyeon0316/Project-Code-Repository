using System.Collections;
using System.Collections.Generic;

public sealed class PlayerStat : Stat
{
    public int Mp { get; private set; }
    public int MaxMp { get; private set; }
    public int MaxPostion { get; private set; }

    public PlayerStat(PlayerStatObj statData)
    {
        MaxHp = statData.MaxHp;
        _hp = new UniRx.ReactiveProperty<int>(MaxHp);
        MaxMp = statData.MaxMp;
        Mp = MaxMp;
        Defense = statData.Defense;
        RecoveryHp = statData.RecoveryHp;
        Dodge = statData.Dodge;
        HitPercent = statData.HitPercent;
        ReduceDamage = statData.ReduceDamage;
        MoveSpeed = statData.MoveSpeed;
        Attack = statData.Attack;
        AllDamge = statData.AllDamage;
        SkillDamage = statData.SkillDamage;
        MaxPostion = statData.MaxPostion;
    }

    public void Equip(ItemStat statData)
    {
        MaxHp += statData.MaxHp;
        MaxMp += statData.MaxMp;
        Mp += MaxMp;
        Defense += statData.Defense;
        RecoveryHp += statData.RecoveryHp;
        Dodge += statData.Dodge;
        HitPercent += statData.HitPercent;
        ReduceDamage += statData.ReduceDamage;
        MoveSpeed += statData.MoveSpeed;
        Attack += statData.Attack;
        AllDamge += statData.AllDamge;
        SkillDamage += statData.SkillDamage;
        MaxPostion += statData.MaxPostion;
    }

    public void UnEquip(ItemStat statData)
    {
        MaxHp -= statData.MaxHp;
        MaxMp -= statData.MaxMp;
        Mp -= MaxMp;
        Defense -= statData.Defense;
        RecoveryHp -= statData.RecoveryHp;
        Dodge -= statData.Dodge;
        HitPercent -= statData.HitPercent;
        ReduceDamage -= statData.ReduceDamage;
        MoveSpeed -= statData.MoveSpeed;
        Attack -= statData.Attack;
        AllDamge -= statData.AllDamge;
        SkillDamage -= statData.SkillDamage;
        MaxPostion -= statData.MaxPostion;
    }
}
