using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;
using UniRx;

public abstract class Enemy : Creature
{
    [SerializeField] private string _name;
    [SerializeField] private PoolType _curEnemyType;
    [SerializeField] private EnemyStatObj _statObj;
    [SerializeField] private Hpbar _hpBar;
    [Header("스폰지점으로 돌아가기 전 까지 거리")]
    [SerializeField] private float _backDistance;
    [Header("스폰이나 사망때 사용 될 디졸브")]
    [SerializeField] private Dissolve _dissolve;

    private EnemySpawnArea _spawnArea;
    private bool _isFollow;

    private CancellationTokenSource _moveToken;
    private CancellationTokenSource _attackToken;
    private CancellationTokenSource _backToken;

    protected virtual void Awake()
    {
        Stat = new EnemyStat(_statObj);
    }

    protected override void Start()
    {
        base.Start();
        Stat.CurHp.Subscribe(curHp => _hpBar.SetHpBar(Stat.MaxHp, curHp, _name));
    }

    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        Clear();
    }

    public void ChangeStat(EnemyStatObj enemyStatData)
    {
        EnemyStat stat = Stat as EnemyStat;
        stat.Set(enemyStatData);
    }

    protected override void Init()
    {
        base.Init();

        _moveToken = new CancellationTokenSource();
        _attackToken = new CancellationTokenSource();
        _backToken = new CancellationTokenSource();
        this.gameObject.layer = LayerMask.NameToLayer("Enemy"); 
        _isFollow = false;

        ExecuteWaitPattern();
    }

    protected override void Clear()
    {
        base.Clear();

        CancelInvoke(nameof(ExecuteWaitPattern));
        CheckAndCancelToken(_moveToken);
        CheckAndCancelToken(_attackToken);
        CheckAndCancelToken(_backToken);
    }

    private void FixedUpdate()
    {
        if (!IsDead)
        {
            FreezeVelocity();
        }
    }

    private void ExecuteWaitPattern()
    {
        _nav.ResetPath();
        int state = Random.Range(1, 3);
        switch (state)
        {
            //정지
            case 1:
                Invoke(nameof(ExecuteWaitPattern), Random.Range(2, 6));
                break;
            //이동
            case 2:
                MoveAsync(_moveToken).ContinueWith(ExecuteWaitPattern).Forget();
                break;
        }
    }
   
    private async UniTask MoveAsync(CancellationTokenSource cancel)
    {
        await UniTask.NextFrame(cancellationToken: cancel.Token);
        _nav.SetDestination(_spawnArea.GetRandomSpawnPos());
        while (true)
        {
            if(cancel.IsCancellationRequested || IsDead)
            {
                cancel.Dispose();
                break;
            }

            if(Vector3.Distance(_nav.destination, this.transform.position) <=0.5f)
            {
                _anim.CrossFade(Global.Idle, 0.5f);
                _nav.ResetPath();
                await UniTask.Delay(1000, cancellationToken: cancel.Token);
                break;
            }

            _anim.Play(Global.Walk);
            await UniTask.NextFrame(cancellationToken: cancel.Token);
        }
    }

    /// <summary>
    /// outTr 부터 거리를 측정
    /// </summary>
    private async UniTask MeasureDistAsync(Vector3 outVec)
    {
        while (true)
        {
            if(IsDead)
            {
                break;
            }

            if(_backDistance < Vector3.Distance(this.transform.position, outVec))
            {
                break;
            }
            
            await UniTask.NextFrame();
        }
    }


    /// <summary>
    /// 스폰지역에서 멀어질때 OR 적이 없어질때 다시 제자리로 돌아감
    /// </summary>
    private async UniTask BackAreaAsync(CancellationTokenSource cancel)
    {
        _nav.ResetPath();
        _attackToken.Cancel();
        _hpBar.CloseHpBar();
        _nav.SetDestination(_spawnArea.GetRandomSpawnPos());
        while (true)
        {
            if (cancel.IsCancellationRequested || IsDead)
            {
                cancel.Dispose();
                break;
            }

            if (_nav.remainingDistance <= 0.5f)
            {
                _isFollow = false;
                _attackToken = new CancellationTokenSource();
                _moveToken = new CancellationTokenSource();
                UniTask.Delay(1000).ContinueWith(ExecuteWaitPattern).Forget();
                Stat.InitHp();
                break;
            }
            await UniTask.NextFrame(cancellationToken: cancel.Token);
        }
    }

    protected abstract UniTask AttackAsync();

    private async UniTask ExecuteAttackPatternAsync(CancellationTokenSource cancel)
    {
        _nav.ResetPath();
        this.transform.LookAt(DataManager.Instance.Player.transform);
        while(true)
        {
            await UniTask.NextFrame(cancellationToken: cancel.Token);
            _nav.SetDestination(DataManager.Instance.Player.transform.position);
            if (cancel.IsCancellationRequested || IsDead)
            {
                _nav.ResetPath();
                cancel.Dispose();
                break;
            }

            if(_attackRadius >= Vector3.Distance(this.transform.position, DataManager.Instance.Player.transform.position))
            {
                _nav.ResetPath();
                await AttackAsync();
                ExecuteAttackPatternAsync(cancel).Forget();
                break;
            }
            _anim.Play(Global.Walk);
        }
    }

    protected override void TakeDamage(int amount, int pureDamage)
    {
        if (!_isFollow)
        {
            //피격시 공격패턴 수행
            _hpBar.ShowHpBar();
            CancelInvoke(nameof(ExecuteWaitPattern));
            _moveToken.Cancel();
            ExecuteAttackPatternAsync(_attackToken).Forget();
            _isFollow = true;
        }

        base.TakeDamage(amount, pureDamage);
    }


    protected override void Die()
    {
        base.Die();
        _nav.ResetPath();
        _anim.Play(Global.Dead);
        _hpBar.CloseHpBar();
        _spawnArea.ReturnEnemy(this);
        QuestManager.Instance.CheckEnemyQuest(_curEnemyType);
        DataManager.Instance.Player.DeleteTarget(this.transform);
        CheckAndCancelToken(_moveToken);
        CheckAndCancelToken(_attackToken);
        CheckAndCancelToken(_backToken);

        Invoke(nameof(Hide), _anim.GetCurrentAnimatorStateInfo(0).length);
    }

    /// <summary>
    /// Nav 이동 방해 예방
    /// </summary>
    private void FreezeVelocity()
    {
        _rigid.angularVelocity = Vector3.zero;
        _rigid.velocity = Vector3.zero;
    }

    public void Show()
    {
        _dissolve.DissolveIn();
    }

    public void Hide()
    {
        _dissolve.DissolveOutAsync().ContinueWith(
            ()=> ObjectPoolManager.Instance.ReturnObject(_curEnemyType, this.gameObject)
        ).Forget();
    }    

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("SpawnArea")) //스폰 지역에서 벗어났을때
        {
            if (_isFollow && !IsDead)
            {
                //만약 돌아가는중에 공격을 다시 받는상황이면?
                MeasureDistAsync(this.transform.position).ContinueWith(() => BackAreaAsync(_backToken)).Forget();
            }
        }
    }

    public void SetArea(EnemySpawnArea area)
    {
        _spawnArea = area;
    }
    
    public PoolType GetEnemyType()
    {
        return _curEnemyType;
    }

    private void CheckAndCancelToken(CancellationTokenSource token)
    {
        if(token != null)
        {
            token.Cancel();
        }
    }

}
