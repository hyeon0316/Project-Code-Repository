using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Spider : Enemy, IGold
{
    [SerializeField] private Attack _attack;
    [SerializeField] private float _attackDelay;
    public int RewardGold { get; set; }

    protected override void Init()
    {
        base.Init();
        _attack.SetStat(Stat);
    }

    protected override async UniTask AttackAsync()
    {
        _anim.Play(Global.Attack);
        await UniTask.NextFrame(); //제대로 Attack 애니의 length를 불러오기 위한 딜레이
        var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length * 0.6f);
        await UniTask.Delay(animDelay);
        DataManager.Instance.Player.TryGetDamage(Stat, _attack);
        _anim.CrossFade(Global.Idle, 0.3f);
        await UniTask.Delay(TimeSpan.FromSeconds(_attackDelay));
    }

    protected override void Die()
    {
        base.Die();
        DataManager.Instance.SetGold(RewardGold);
    }

}
