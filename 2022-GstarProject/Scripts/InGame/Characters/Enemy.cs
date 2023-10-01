using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;


public abstract class Enemy : Creature
{
    [SerializeField] private EnemyStatData _enemyStatData;
    public BoxCollider SpawnArea { get; set; }
    
    [Header("스폰지점으로 돌아가기 전 까지 거리")]
    [SerializeField] private float _backDistance;

    [Header("스폰이나 사망때 사용 될 디졸브")]
    [SerializeField] private Dissolve _dissolve;
    
    [SerializeField] private string _name;

    public int gold;

    /// <summary>
    /// 스폰 지점에서 나왔을때의 위치
    /// </summary>
    private Vector3 _outVector; 

    /// <summary>
    /// 스폰 지점에서 나왔는지
    /// </summary>
    private bool _isOutArea; 
    
    /// <summary>
    /// 되돌아 가는 중인지
    /// </summary>
    private bool _isGoBack; 
    
    public PoolType CurEnemyType;
    
    /// <summary>
    /// 플레이어를 추적해야 하는지에 대한 변수
    /// </summary>
    protected bool _isFollow; 
    
    protected bool _isAttack;

    /// <summary>
    /// 플레이어가 공격하지 않았을때 상태
    /// </summary>
    private bool _isWait; 

    protected override void Awake()
    {
        base.Awake();
        Stat = new Stat();
        Stat.SetEnemyStat(_enemyStatData);
    }
    public void SetStat(EnemyStatData enemyStatData)
    {
        _enemyStatData = enemyStatData;
        Stat.SetEnemyStat(_enemyStatData);
        Stat.Hp = Stat.MaxHp;
    }
    protected virtual void OnEnable()
    {
        Init();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _nav.enabled = false;
        CancelInvoke("SetRandomMove");
        StopCoroutine(BackToArea());
        StopCoroutine(NextMoveCo());
    }

    protected override void Init()
    {
        Stat.SetEnemyStat(_enemyStatData);
        base.Init();
        
        this.gameObject.layer = LayerMask.NameToLayer("Enemy"); 
        _isFollow = false;
        _isAttack = false;
        _isGoBack = false;
        _isOutArea = false;
        _isWait = true;
       
        _hpbar.SetHpBar(Stat.MaxHp, _name);
    }

    protected virtual void Start()
    {
        _targets.Add(DataManager.Instance.Player.transform);
    }

    private void Update()
    {
        WaitPattern();
    }
    

    private void FixedUpdate()
    {
        if (!IsDead)
        {
            DoPattern();
            FreezeVelocity();
        }
    }

    /// <summary>
    /// 공격받기 전 대기모드 상태의 패턴
    /// </summary>
    private void WaitPattern()
    {
        if (!IsDead)
        {
            if (_isOutArea)
            {
                if (_backDistance < Vector3.Distance(transform.position, _outVector)) //일정 거리 이상 스폰지역 밖으로 나왔을때
                {
                    StartCoroutine(BackToArea());
                }
            }

            if (_isWait)
            {
                _nav.enabled = true;
                SetRandomMove();
                _isWait = false;
            }
        }
    }

    /// <summary>
    /// 스폰될때 모습을 서서히 나타냄
    /// </summary>
    public void ShowAppearance()
    {
        _dissolve.DissolveIn();
    }
    
    /// <summary>
    /// 대기상태일때 랜덤 패턴 설정(이동, 멈춤)
    /// </summary>
    private void SetRandomMove()
    {
        int state = Random.Range(1, 3);
        switch (state)
        {
            case 1:
                _nav.isStopped = true;
                SetAnimations(0, 0, 0, 0,0,0,0);
                Invoke("SetRandomMove", Random.Range(3, 7));
                break;
            case 2:
                StartCoroutine(NextMoveCo());
                break;
        }
    }

    private IEnumerator NextMoveCo()
    {
        _nav.isStopped = false;
        _nav.SetDestination(RandomBackPos());
        SetAnimations(3, 1, 1, 1,1,1,1);
        while (true)
        {
            if (IsDead)
                break;
            
            if (_nav.remainingDistance <= 0.1f)
            {
                Invoke("SetRandomMove", 1);
                SetAnimations(0, 0, 0, 0,0,0,0);
                break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 스폰지점 안의 범위에서 랜덤 위치 반환
    /// </summary>
    private Vector3 RandomBackPos()
    {
        Vector3 originPos = SpawnArea.transform.position;
        float width = SpawnArea.bounds.size.x;
        float height = SpawnArea.bounds.size.z;

        float randomX = Random.Range((width / 2) * -1, width / 2);
        float randomZ = Random.Range((height / 2) * -1, height / 2);

        Vector3 randomPos = new Vector3(randomX, 0, randomZ);

        Vector3 backPos = randomPos + originPos;
        return backPos;
    }

    /// <summary>
    /// 스폰지역에서 멀어질때 OR 적이 없어질때 다시 제자리로 돌아감
    /// </summary>
    private IEnumerator BackToArea()
    {
        _isFollow = false;
        _isOutArea = false;
        _nav.SetDestination(RandomBackPos()); 
        _nav.isStopped = false;
        _isGoBack = true;
        _hpbar.CloseHpBar();
        while (true)
        {
            if (IsDead) //도착하기 전에 죽게되는 경우
                break;
            
            if (_nav.remainingDistance <= 0.5f) //도착했을때
            {
                if(CurEnemyType is PoolType.ForestGolem1 or PoolType.ForestGolem2 or  PoolType.ForestGolem3 or PoolType.SpecialGolem)
                    _animator.SetInteger(Global.EnemyAttackInteger, -1);

                SetAnimations(0, 0, 0, 0,0,0,0);
                _isWait = true;
                _nav.isStopped = true;
                _isGoBack = false;
                if(CurEnemyType != PoolType.Boss)
                    Stat.Hp = Stat.MaxHp; //복귀시 체력 전체회복
                break;
            }

            SetAnimations(3, 0, 1, 1,1,1,1);
            yield return null;
        }
    }
    
    /// <summary>
    /// Rigidbody의 Speed나 다른 값의 변화 방지
    /// </summary>
    private void FreezeVelocity()
    {
        _rigid.angularVelocity = Vector3.zero;
        _rigid.velocity = Vector3.zero;
    }

    private void DoPattern()
    {
        if (!_isAttack) //공격중이 아닐때에만 행동
        {
            if (_isFollow)
            {
                transform.LookAt(_targets[0]);
                if (_attackRadius < Vector3.Distance(transform.position, _targets[0].position)) //타겟이 공격사거리 밖에있을때
                {
                    SetAnimations(1, 1, 1, 1,1,1,1);
                    _nav.isStopped = false;
                    _nav.SetDestination(_targets[0].transform.position);
                }
                else
                {
                    SetAnimations(0, 0, 0, 0,0,0,0);
                    _nav.isStopped = true;
                    Attack();
                }
            }
        }
    }
    
    protected abstract void Attack();
    

    public override void TryGetDamage(Stat stat, Attack attack)
    {
        if(!IsDead)
            _hpbar.ShowHpBar();
        
        base.TryGetDamage(stat, attack);
        if (!_isFollow && !_isGoBack) //피격 당했을 때, 되돌아 가는 중이 아닐 때 추적 시작
        {
            if ((transform.position - _targets[0].position).sqrMagnitude < Math.Pow(11, 2)) //던전에서 플레이어가 나갔을때 던전 몹이 플레이어를 찾지 못하는 경우 방지
            {
                _nav.enabled = true;
                _nav.isStopped = true;
                CancelInvoke("SetRandomMove");
                _isFollow = true;
            }
        }
        
        _hpbar.UpdateHpBar(Stat.Hp);
    }
    

    protected override void Die()
    {
        _hpbar.CloseHpBar();
        base.Die();
        CancelInvoke("SetRandomMove");
        if (_nav.enabled)
        {
            _nav.isStopped = true;
        }

        _dissolve.gameObject.layer = LayerMask.NameToLayer("Enemy");
        SpawnArea.GetComponent<EnemySpawnArea>().SpawnRandomEnemy(this.gameObject);
        _animator.SetTrigger(Global.EnemyDeadTrigger);
        QuestManager.Instance.CheckEnemyQuest(CurEnemyType);
        DataManager.Instance.Player.DeleteTarget(transform);
        DataManager.Instance.SetGold(gold);
    }

    /// <summary>
    /// 죽었을때 풀링 반환
    /// </summary>
    private void DisableEnemy()
    {
        _nav.enabled = false;
        ObjectPoolManager.Instance.ReturnObject(CurEnemyType,this.gameObject);
    }

    public void ActiveDeadEffect()
    {
        _dissolve.DissolveOut();
        Invoke("DisableEnemy", 1);
    }

    /// <summary>
    /// 각 적 타입마다 애니메이션 셋팅
    /// </summary>
    private void SetAnimations(int spider, int frightFly, int forestGolem, int specialGolem, int goblinWarrior, int goblinArcher, int boss)
    {
        switch (CurEnemyType)
        {
            case PoolType.Spider:
                _animator.SetInteger(Global.EnemyStateInteger,spider);
                break;
            case PoolType.FrightFly:
                _animator.SetInteger(Global.EnemyStateInteger,frightFly);
                break;
            case PoolType.ForestGolem1:
            case PoolType.ForestGolem2:
            case PoolType.ForestGolem3:
                _animator.SetInteger(Global.EnemyStateInteger, forestGolem);
                break;
            case PoolType.SpecialGolem:
                _animator.SetInteger(Global.EnemyStateInteger, specialGolem);
                break;
            case PoolType.GoblinWarrior:
            case PoolType.Goblin:
                _animator.SetInteger(Global.EnemyStateInteger,goblinWarrior);
                break;
            case PoolType.GoblinArcher:
                _animator.SetInteger(Global.EnemyStateInteger,goblinArcher);
                break;
            case PoolType.Boss:
                _animator.SetInteger(Global.EnemyStateInteger,boss);
                break;
        }
    }
    

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("SpawnArea")) //스폰 지역에서 벗어났을때
        {
            if (_isFollow)
            {
                _isOutArea = true;
                _outVector = transform.position;
            }
        }
    }
    

    
}
