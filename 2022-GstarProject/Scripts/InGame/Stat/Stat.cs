using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

public class Stat 
{
    public IReadOnlyReactiveProperty<int> CurHp => _hp;
    protected ReactiveProperty<int> _hp;

    public int MaxHp { get; protected set; }
    public int RecoveryHp { get; protected set; }//자연 Hp회복량
    public int RecoveryMp { get; protected set; }//자연 Mp회복량
    public int Defense { get; protected set; }
    public int Dodge { get; protected set; }
    public int HitPercent { get; protected set; }
    public int ReduceDamage { get; protected set; } // 받는 모든 데미지 감소량
    public int MoveSpeed { get; protected set; }
    public int Attack { get; protected set; }
    public int SkillDamage { get; protected set; } //스킬데미지
    public int AllDamge { get; protected set; } //모든데미지

    public void InitHp()
    {
        _hp.Value = MaxHp;
    }

    public void Heal(int amount)
    {
        _hp.Value = Math.Min(_hp.Value + amount, MaxHp);
    }

    public void TakeDamage(int damage)
    {
        _hp.Value = Math.Max(0, _hp.Value - damage);
    }
}
