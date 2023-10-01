using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    [SerializeField] private ShortAttack _shortAttackArea;
    [SerializeField] private BoxCollider _attackCollider;
    
    [SerializeField] private LongAttack _rockPos;

    [SerializeField] private HpbarController _bossHpBar;

    private AudioSource _audioSource;
    
    /// <summary>
    /// 2번째 페이즈 진입시 공격 변경
    /// </summary>
    private int _nextPatternState;


    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _attackCollider.enabled = false;
        _nextPatternState = 0;
    }
    
    protected override void Start()
    {
        base.Start();
        _shortAttackArea.SetStat(Stat);
        _bossHpBar.SetHpBar(Stat.MaxHp);
    }

    protected override void Attack()
    {
        SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BossAttack);
        _isAttack = true;
        int attackState = 2 + _nextPatternState;
        int randomPattern = Random.Range(0, 1 + _nextPatternState);

        if (randomPattern == 0)
        {
            _animator.SetInteger(Global.EnemyStateInteger, attackState);
            SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BossAttack);
        }
        else
        {
            _animator.SetInteger(Global.EnemyStateInteger, 4);
            SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BossThrowing);
        }
    }

    public override void TryGetDamage(Stat stat, Attack attack)
    {
        _bossHpBar.ShowHpBar();
        base.TryGetDamage(stat, attack);

        if (Stat.Hp <= Stat.MaxHp / 2 && _nextPatternState == 0)
            EntryNextPattern();
        
        _bossHpBar.UpdateHpBar(Stat.Hp);
    }

    protected override void Die()
    {
        base.Die();
        _bossHpBar.CloseHpBar();
        SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BossDie);
    }
    public void CreateRock()
    {
        _rockPos.CreateProjectile(PoolType.BossRock, Stat);
    }
    
    private void EntryNextPattern()
    {
        SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BossRoar);
        _nextPatternState++;
        _animator.SetTrigger(Global.EnemyNextPattern);
        _attackCollider.enabled = false;
    }

    public void ActiveAttackCollider()
    {
        _attackCollider.enabled = true;
    }
    
    public void InActiveAttackCollider()
    {
        _attackCollider.enabled = false;
    }
    
    public void InActiveAttack()
    {
        _animator.SetInteger(Global.EnemyStateInteger, 0);
        _isAttack = false;
    }
}
