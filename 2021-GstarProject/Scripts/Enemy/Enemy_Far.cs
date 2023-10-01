using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class Enemy_Far : LivingEntity
{

    public float audioVol;
    public AudioClip beAttackSound;
    public Vector3 nameOffset = new Vector3(0f, 5f, 0);
    public GameObject nameText;
    public TextMeshProUGUI nameObject;

    public Vector3 DamageOffset = new Vector3(-0.5f, 5f, 0);
    public GameObject damageText;

    public GameObject[] _item;
    public float[] _dropP;
    public GameObject hpBarPrefab;
    public Vector3 hpBarOffset = new Vector3(-0.5f, 2.4f, 0);

    public Canvas enemyHpBarCanvas;
    public Canvas nameCanvas;
    public Slider enemyHpBarSlider;

    public LayerMask whatIsTarget; //추적대상 레이어

    private LivingEntity targetEntity;//추적대상
    private NavMeshAgent pathFinder; //경로 계산 AI 에이전트
    private float dist; //적과 추적대상과의 거리

    

    //스태프
    public GameObject firePoint; //매직미사일이 발사될 위치
    public GameObject magicMissilePrefab; //사용할 매직미사일 할당
    GameObject magicMissile; //Instantiate()메서드로 생성하는 매직미사일을 담는 게임오브젝트


    private Animator enemyAnimator;
    

    public float damage = 30f; //공격력
    public float attackSpeed = 10f; //공격력
    public float attackDelay = 2.5f; //공격 딜레이
    private float lastAttackTime; //마지막 공격 시점

    public Transform tr;
    private float attackRange = 15f;

    public float LookatSpeed = 1f; //0~1

    public int enemyExp;

    //추적 대상이 존재하는지 알려주는 프로퍼티
    private bool hasTarget
    {
        get
        {
            //추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (targetEntity != null && !targetEntity.dead)
            {
                return true;
            }

            //그렇지 않다면 false
            return false;
        }
    }

    private bool canMove;
    private bool canAttack;

    void SetNaviStop(bool val)
    {
        pathFinder.isStopped = val;
    }

    private void Awake()
    {
        //게임 오브젝트에서 사용할 컴포넌트 가져오기
        pathFinder = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
        //enemyAudioPlayer = GetComponent<AudioSource>();
    }

    void Start()
    {
        SetHpBar();
        SetName();
        //게임 오브젝트 활성화와 동시에 AI의 탐지 루틴 시작
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();

        //추적 대상과의 멈춤 거리 랜덤하게 설정하기(7~10사이), 적이 뭉쳐있는 것보다 산개된 모습을 주기 위해서
        pathFinder.stoppingDistance = Random.Range(7, 11);
    }

    // Update is called once per frame
    void Update()
    {
        //추적 대상의 존재 여부에 따라 다른 애니메이션 재생
        enemyAnimator.SetBool("CanMove", canMove);
        enemyAnimator.SetBool("CanAttack", canAttack);

        if (hasTarget)
        {
            //추적 대상이 존재할 경우 거리 계산은 실시간으로 해야하니 Update()
            dist = Vector3.Distance(tr.position, targetEntity.transform.position);
        }

        if (!dead && hasTarget)
        {
            //추적 대상 바라보기
            Vector3 dir = targetEntity.transform.position - this.transform.position;

            this.transform.rotation = Quaternion.Lerp(this.transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * LookatSpeed);
        }
    }

    void SetHpBar()
    {
        enemyHpBarCanvas = GameObject.Find("EnemyHpBarCanvas").GetComponent<Canvas>();
        GameObject hpBar = Instantiate<GameObject>(hpBarPrefab, transform.position, Quaternion.identity, enemyHpBarCanvas.transform);

        var _hpbar = hpBar.GetComponent<EnemyHpBar>();

        _hpbar.enemyTr = this.gameObject.transform;
        _hpbar.offset = hpBarOffset;
        enemyHpBarSlider = _hpbar.GetComponent<Slider>(); //체력감소시키기위해 getcomponent(게임 실행 시 연결이 안되었던 문제 해결)
    }

    void SetName()
    {
        nameCanvas = GameObject.Find("NameCanvas").GetComponent<Canvas>();
        GameObject namePrefab = Instantiate<GameObject>(nameText, transform.position, Quaternion.identity, nameCanvas.transform);
        var _namePrefab = namePrefab.GetComponent<EnemyHpBar>();

        _namePrefab.enemyTr = this.gameObject.transform;
        _namePrefab.offset = nameOffset;

        nameObject = _namePrefab.GetComponent<TextMeshProUGUI>();
    }
    //추적할 대상의 위치를 주기적으로 찾아 경로 갱신, 대상이 있으면 공격한다.
    private IEnumerator UpdatePath()
    {
        //살아 있는 동안 무한 루프
        while (!dead)
        {
            if (hasTarget)
            {
                Attack();
            }
            else
            {
                //추적 대상이 없을 경우, AI 이동 정지
                SetNaviStop(true);
                canAttack = false;
                canMove = false;

                //반지름 20f의 콜라이더로 whatIsTarget 레이어를 가진 콜라이더 검출하기
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

                //모든 콜라이더를 순회하면서 살아 있는 LivingEntity 찾기
                for (int i = 0; i < colliders.Length; i++)
                {
                    //콜라이더로부터 LivingEntity 컴포넌트 가져오기
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    //LivingEntity 컴포넌트가 존재하며, 해당 LivingEntity가 살아 있다면
                    if (livingEntity != null && !livingEntity.dead)
                    {
                        //추적 대상을 해당 LivingEntity로 설정
                        targetEntity = livingEntity;
                        //for문 루프 즉시 정지
                        break;
                    }
                }
            }

            //0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    //적과 플레이어 사이의 거리 측정, 거리에 따라 공격 메서드 실행
    public virtual void Attack()
    {
        //자신이 사망X, 최근 공격 시점에서 attackDelay 이상 시간이 지났고, 플레이어와의 거리가 공격 사거리안에 있다면 공격 가능
        if (!dead && dist <= attackRange)
        {
            SetNaviStop(true);
            //공격 반경 안에 있으면 움직임을 멈춘다.
            canMove = false;

            

            //공격 딜레이가 지났다면 공격 애니 실행
            if (lastAttackTime + attackDelay <= Time.time)
            {
                canAttack = true;
                lastAttackTime = Time.time; //최근 공격시간 초기화
            }

            //공격 반경 안에 있지만, 딜레이가 남아있을 경우
            else
            {
                canAttack = false;
            }
        }

        //공격 반경 밖에 있을 경우 추적하기
        else
        {
            //추적 대상이 존재 && 추적 대상이 공격 반경 밖에 있을 경우, 경로를 갱신하고 AI 이동을 계속 진행
            canMove = true;
            canAttack = false;
            SetNaviStop(false); //계속 이동
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    //유니티 애니메이션 이벤트로 지팡이를 앞으로 휘두를 떄 메서드 실행(미사일 발사)
    public void ShamanFire()
    {
        magicMissile = Instantiate(magicMissilePrefab, firePoint.transform.position, firePoint.transform.rotation);
        magicMissile.GetComponent<EnemyAttack>().dmg = damage;
        magicMissile.GetComponent<EnemyAttack>().speed = attackSpeed;

        Destroy(magicMissile, 2f);

    }  

    //데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage)
    {
        if (!dead)
        {
            SoundManager.inst.SFXPlay("beAttack", beAttackSound, audioVol);
        }


        //피격 애니메이션 재생
        enemyAnimator.SetTrigger("Hit");

        GameObject hubText = Instantiate(damageText, transform.position, Quaternion.identity, enemyHpBarCanvas.transform);
        var _hubText = hubText.GetComponent<DamageText>();

        _hubText.enemyTr = this.gameObject.transform;
        _hubText.offset = DamageOffset;
        _hubText.damage = damage;

        //LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage);

        enemyHpBarSlider.value = health;
    }

    //사망 처리
    public override void Die()
    {
        Player.inst.ExpPlus(enemyExp);
        enemyHpBarSlider.gameObject.SetActive(false);
        nameObject.gameObject.SetActive(false);
        //다른 AI를 방해하지 않도록 자신의 모든 콜라이더를 비활성화
        Collider[] enemyColliders = GetComponents<Collider>();
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }

        //AI추적을 중지하고 네비메쉬 컴포넌트를 비활성화
        SetNaviStop(true);
        pathFinder.enabled = false;

        //사망 애니메이션 재생
        enemyAnimator.ResetTrigger("Hit");
        enemyAnimator.SetTrigger("doDie");
       
        GameObject[] newItem;
        newItem = new GameObject[_item.Length];
        for (int i = 0; i < _item.Length; i++)
        {
            float k = Random.Range(0, 100);
            if (_dropP[i] > k)
                newItem[i] = Instantiate(_item[i], transform.position + transform.up , Quaternion.identity);
        }
        //LivingEntity의 DIe()를 실행하여 기본 사망 처리 실행
        base.Die();

        Invoke("DestroyEnemy", 2f);
    }

    //enemyHpBarSlider 활성화
    protected override void OnEnable()
    {
        //LivingEntity의 OnEnable() 실행(상태초기화)
        base.OnEnable();

        //체력 슬라이더 활성화
        enemyHpBarSlider.gameObject.SetActive(true);
        nameObject.gameObject.SetActive(true);
        //체력 슬라이더의 최댓값을 기본 체력값으로 변경
        enemyHpBarSlider.maxValue = startingHealth;
        //체력 슬라이더의 값을 현재 체력값으로 변경
        enemyHpBarSlider.value = health;
    }

    private void DestroyEnemy()
    {
        GameObject.Destroy(gameObject);
    }
}

