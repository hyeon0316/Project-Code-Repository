using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStat : Stat
{
    public int MaxPostion;
    public int MaxMp;

    public ItemStat DeepCopy()
    {
        ItemStat newCopy = new ItemStat();
        newCopy.Attack = this.Attack;
        newCopy.HitPercent = this.HitPercent;
        newCopy.SkillDamage = this.SkillDamage;
        newCopy.AllDamge = this.AllDamge;

        newCopy.Defense = this.Defense;
        newCopy.Dodge = this.Dodge;
        newCopy.ReduceDamage = this.ReduceDamage;

        newCopy.MaxHp = this.MaxHp;
        newCopy.MaxMp = this.MaxMp;
        newCopy.MaxPostion = this.MaxPostion;
        newCopy.RecoveryHp = this.RecoveryHp;
        newCopy.RecoveryMp = this.RecoveryMp;
        return newCopy;
    }

    public void Set(int damage, int hit, int skillDmg, int allDmg, int defense, int evasion, int allDefense, int hp, int mp, int postion, int hpRe, int mpRe)
    {
        Attack = damage;
        HitPercent = hit;
        SkillDamage = skillDmg;
        AllDamge = allDmg;
        Defense = defense;
        Dodge = evasion;
        ReduceDamage = allDefense;
        MaxHp = hp;
        MaxMp = mp;
        MaxPostion = postion;
        RecoveryHp = hpRe;
        RecoveryMp = mpRe;
    }

    public void SetAttack(int value)
    {
        Attack += value;
    }

    public void SetHitPercent(int value)
    {
        HitPercent += value;
    }

    public void SetSkillDamage(int value)
    {
        SkillDamage += value;
    }

    public void SetAllDamage(int value)
    {
        AllDamge += value;
    }

    public void SetDodge(int value)
    {
        Dodge += value;
    }

    public void SetDefense(int value)
    {
        Defense += value;
    }

    public void SetReduceDamage(int value)
    {
        ReduceDamage+= value;
    }

    public void SetMaxHp(int value)
    {
        MaxHp += value;
    }

    public void SetRecoveryHp(int value)
    {
        RecoveryHp += value;
    }

    public void SetRecoveryMp(int value)
    {
        RecoveryMp += value;
    }
}
