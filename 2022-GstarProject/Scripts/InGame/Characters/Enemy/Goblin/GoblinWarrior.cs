using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GoblinWarrior : Enemy
{
    [SerializeField] private Attack _attack;
    [SerializeField] private float _attackDelay;

    protected override void Init()
    {
        base.Init();
        _attack.SetStat(Stat);
    }

    protected override async UniTask AttackAsync()
    {
        _anim.Play(Global.Attack);
        await UniTask.NextFrame(); //����� Attack �ִ��� length�� �ҷ����� ���� ������
        var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length * 0.6f);
        await UniTask.Delay(animDelay);
        DataManager.Instance.Player.TryGetDamage(Stat, _attack);
        _anim.CrossFade(Global.Idle, 0.3f);
        await UniTask.Delay(TimeSpan.FromSeconds(_attackDelay));
    }

}
