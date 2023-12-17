using System.Collections;
using System.Collections.Generic;

public sealed class EnemyStat : Stat
{
    public EnemyStat(EnemyStatObj statData)
    {
        Set(statData);
    }

    public void Set(EnemyStatObj statData)
    {
        MaxHp = statData.MaxHp;
        _hp = new UniRx.ReactiveProperty<int>(MaxHp);
        Defense = statData.Defense;
        Dodge = statData.Dodge;
        HitPercent = statData.HitPercent;
        ReduceDamage = statData.ReduceDamage;
        MoveSpeed = statData.MoveSpeed;
        Attack = statData.Attack;
        AllDamge = 100;
    }
}
