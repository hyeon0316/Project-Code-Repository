using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStat : Stat
{
    
    public int MaxPostion { get; set; }
    public int RecoveryMp { get; set; }

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
}
