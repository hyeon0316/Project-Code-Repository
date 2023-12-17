using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ForestGolem : Enemy
{
    [SerializeField] private Attack _attack;
    [SerializeField] private float _attackDelay;

    private int _attackOrder;

    protected override void Init()
    {
        base.Init();
        _attack.SetStat(Stat);
        _attackOrder = 1;
    }

    protected override async UniTask AttackAsync()
    {
        PlayAnim();
        await UniTask.NextFrame(); //제대로 Attack 애니의 length를 불러오기 위한 딜레이
        var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length * 0.6f);
        await UniTask.Delay(animDelay);
        DataManager.Instance.Player.TryGetDamage(Stat, _attack);
        _anim.CrossFade(Global.Idle, 0.3f);
        await UniTask.Delay(TimeSpan.FromSeconds(_attackDelay));
    }

    private void PlayAnim()
    {
        switch (_attackOrder)
        {
            case 1:
                _anim.Play(Global.Attack1);
                break;
            case 2:
                _anim.Play(Global.Attack2);
                break;
            case 3:
                _anim.Play(Global.Attack3);
                break;
        }
        _attackOrder = (_attackOrder % 3) + 1;
    }

}
