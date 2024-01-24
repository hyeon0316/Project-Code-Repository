using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public sealed class Boss : Enemy
{
    [SerializeField] private GameObject _rockPrefab;
    [SerializeField] private FixedHpBar _fixedHpBar;
    [SerializeField] private Attack _attack;
    [SerializeField] private LongAttackCreator _rockPos;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _attackDelay;

    private int _attackOrder;
    private int _patternOrder;
    private bool _isNextPattern;

    protected override void Awake()
    {
        base.Awake();
        ObjectPoolManager.Instance.Init(_rockPrefab, 1);
    }

    protected override void Start()
    {
        base.Start();
        Stat.CurHp.Subscribe(curHp => _fixedHpBar.SetHpBar(Stat.MaxHp, curHp));
    }

    protected override void Init()
    {
        _isNextPattern = false;
        _attackOrder = 1;
        _patternOrder = 0;
        _attack.SetStat(Stat);
        base.Init();
    }

    protected override async UniTask AttackAsync()
    {
        if (Stat.CurHp.Value < Stat.MaxHp * 0.5f && !_isNextPattern)
        {
            _patternOrder++;
            SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BossRoar);
            _anim.Play(Global.Scream);
            await UniTask.NextFrame();
            var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);
            await UniTask.Delay(animDelay);
            _isNextPattern = true;
        }
        else
        {
            await SelectPatternAsync();
        }
    }

    private async UniTask SelectPatternAsync()
    {
        int randPattern = UnityEngine.Random.Range(0, _patternOrder + 1);

        switch(randPattern)
        {
            case 0:
                await NormalAttack();
                break;
            case 1:
                await LongAttack();
                break;
        }
    }

    private async UniTask NormalAttack()
    {
        PlayNormalAttackAnim();
        SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BossAttack);
        await UniTask.NextFrame();
        var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length * 0.6f);
        await UniTask.Delay(animDelay);
        DataManager.Instance.Player.TryGetDamage(Stat, _attack);
        _anim.CrossFade(Global.Idle, 0.3f);
        await UniTask.Delay(TimeSpan.FromSeconds(_attackDelay));
    }

    private async UniTask LongAttack()
    {
        _anim.Play(Global.LongAttack);
        SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BossThrowing);
        await UniTask.NextFrame();
        var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length * 0.3f);
        await UniTask.Delay(animDelay);
        _rockPos.CreateLongAttack(_rockPrefab, Stat);
        _anim.CrossFade(Global.Idle, 0.3f);
        await UniTask.Delay(TimeSpan.FromSeconds(_attackDelay));
    }


    private void PlayNormalAttackAnim()
    {
        switch (_attackOrder)
        {
            case 1:
                _anim.Play(Global.Attack1);
                break;
            case 2:
                _anim.Play(Global.Attack2);
                break;
        }
        _attackOrder = (_attackOrder % 2) + 1;
    }

    protected override void Die()
    {
        base.Die();
        SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BossDie);
    }
}
