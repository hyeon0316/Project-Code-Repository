using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class FrightFly : Enemy, IGold
{
    public int RewardGold { get; set; }

    [SerializeField] private LongAttackCreator _attackPos;
    [SerializeField] private float _attackDelay;

    protected override void Die()
    {
        base.Die();
        DataManager.Instance.SetGold(RewardGold);
    }

    protected override async UniTask AttackAsync()
    {
        _anim.Play(Global.Attack);
        await UniTask.NextFrame(); 
        var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length * 0.6f);
        await UniTask.Delay(animDelay);
        _attackPos.CreateLongAttack(PoolType.FrightFlyMissile, Stat);
        _anim.CrossFade(Global.Idle, 0.3f);
        await UniTask.Delay(TimeSpan.FromSeconds(_attackDelay));
    }

}
