using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GoblinArcher : Enemy
{
    [SerializeField] private GameObject _arrowPrefab;
    [SerializeField] private LongAttackCreator _attackPos;
    [SerializeField] private float _attackDelay;

    protected override void Awake()
    {
        base.Awake();
        ObjectPoolManager.Instance.Init(_arrowPrefab, 1);
    }

    protected override async UniTask AttackAsync()
    {
        _anim.Play(Global.Attack);
        await UniTask.NextFrame(); //제대로 Attack 애니의 length를 불러오기 위한 딜레이
        var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length * 0.6f);
        await UniTask.Delay(animDelay);
        _attackPos.CreateLongAttack(_arrowPrefab, Stat);
        _anim.CrossFade(Global.Idle, 0.3f);
        await UniTask.Delay(TimeSpan.FromSeconds(_attackDelay));
    }

}
