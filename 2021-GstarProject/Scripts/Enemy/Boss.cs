using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Boss : LivingEntity
{ 
    public Vector3 DamageOffset = new Vector3(-0.5f, 5f, 0);
    public GameObject damageText;

    private Text bossHpText;
    public GameObject bossHpBarPrefab;
    public Canvas enemyHpBarCanvas;
    public Slider bossHpBarSlider;

    public Vector3 hpBarOffset = new Vector3(0, 10f, 0);

    public LayerMask whatIsTarget; //추적대상 레이어

    private LivingEntity targetEntity;//추적대상
    private NavMeshAgent pathFinder; //경로 계산 AI 에이전트

    private Animator bossAnimator;

    public float damage = 20f; //공격력
    public float attackDelay = 2f; //공격 딜레이
    private float lastAttackTime; //마지막 공격 시점
    private float dist; //추적대상과의 거리

    public Transform tr;

    public float attackRange = 5f; //근접공격 범위

    public float skillRange = 20f; //첫번째 보스스킬 작동 범위

    public float LookatSpeed = 1f; //0~1

    public GameObject skill_First;
    public GameObject skill_FirstPos;

    public GameObject skill_Second;

    public GameObject skill_Trap;

    private bool NextPageOn;//보스 2페이지 시작
    private bool _IsInFirstSkill = false; //스킬 모션 재생할때 이동 제한
    private bool _StunOn = true;
    private bool AllStop; //스턴일때 행동제한 시키기 위한 변수
    private bool TrapOnTime;//트랩이 활성화 될 동안 플레이어를 추적

    public GameObject StunEffect;
    private float followTrap = 1;    
    private float timeTemp;

    public GameObject _item;
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
    private bool canStun;

    private void Awake()
    {
        //게임 오브젝트에서 사용할 컴포넌트 가져오기
        pathFinder = GetComponent<NavMeshAgent>();
        bossAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        SetHpBar();
        //게임 오브젝트 활성화와 동시에 AI의 탐지 루틴 시작
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();
    }

    void SetHpBar()
    {
        enemyHpBarCanvas = GameObject.Find("EnemyHpBarCanvas").GetComponent<Canvas>();
        GameObject bossHpBar = Instantiate<GameObject>(bossHpBarPrefab, enemyHpBarCanvas.transform);

        var _hpbar = bossHpBar.GetComponent<BossHpBar>();
        bossHpBarSlider = _hpbar.GetComponent<Slider>();
        bossHpText = GameObject.Find("EnemyHpBarCanvas").transform.GetChild(0).transform.GetChild(2).GetComponent<Text>();
        bossHpText.text = string.Format("{0}", health);
    }
    // Update is called once per frame
    void Update()
    {
        bossAnimator.SetBool("CanMove", canMove);
        bossAnimator.SetBool("CanAttack", canAttack);
        bossAnimator.SetBool("Stun", canStun);

        if (hasTarget)
        {
            //추적 대상이 존재할 경우 거리 계산은 실시간으로 해야하니 Update()
            dist = Vector3.Distance(tr.position, targetEntity.transform.position);
        }

        if (health <= 15000) //2페이즈 돌입(체력 조정)
        {
            NextPageOn = true;
            if (_StunOn)
                StartCoroutine(StunOn());
        }
        if (TrapOnTime)
        {
            if (!skill_Trap.activeSelf)//터지기 전에는 계속 플레이어 추적
            {
                followTrap = 1f;
                skill_Trap.transform.position = Vector3.Lerp(skill_Trap.transform.position, Player.inst.transform.position, Time.deltaTime * followTrap);
            }
            else if (skill_Trap.activeSelf) //트랩이 발동될 때는 제자리에서 발동
                followTrap = 0;
        }

        if(Player.inst.TrapTarget.activeSelf)
        {
            if (timeTemp < 3)
                timeTemp += Time.deltaTime * 2;
            else
                timeTemp = 3f;

            Player.inst.TrapTarget.transform.localScale = new Vector3(timeTemp * 1, timeTemp * 1, timeTemp * 1);
        }
    }

    void StopPathFinder(bool val)
    {
        //Debug.LogFormat("StopPathFinder : {0}", val);
        pathFinder.isStopped = val;
    }
  
    private IEnumerator UpdatePath()  //추적할 대상의 위치를 주기적으로 찾아 경로 갱신
    {
        //살아 있는 동안 무한 루프
        while (!dead)
        {
            if (hasTarget)
            {
                if (!AllStop) 
                {
                    LookAt();
                    Attack();
                }
            }
            else
            {
                //추적 대상이 없을 경우, AI 이동 정지
                StopPathFinder(true);
                canAttack = false;
                canMove = false;
                //반지름 20f의 콜라이더로 whatIsTarget 레이어를 가진 콜라이더 검출하기
                Collider[] colliders = Physics.OverlapSphere(transform.position, 35f, whatIsTarget);
              
                for (int i = 0; i < colliders.Length; i++) //모든 콜라이더를 순회하면서 살아 있는 LivingEntity 찾기
                {
                    
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();//콜라이더로부터 LivingEntity 컴포넌트 가져오기
             
                    if (livingEntity != null && !livingEntity.dead)//LivingEntity 컴포넌트가 존재하며, 해당 LivingEntity가 살아 있다면
                    {                  
                        targetEntity = livingEntity;//추적 대상을 해당 LivingEntity로 설정

                        //for문 루프 즉시 정지
                        break;
                    }
                }
            }
            //0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void LookAt() //추적 대상 바라보기
    {
        Vector3 dir = targetEntity.transform.position - this.transform.position;

        this.transform.rotation = Quaternion.Lerp(this.transform.rotation,
            Quaternion.LookRotation(dir), Time.deltaTime * LookatSpeed);
    }
  
    public virtual void Attack()//추적 대상과의 거리에 따라 공격 실행
    {
        //자신이 사망X, 추적 대상과의 거리이 공격 사거리 안에 있다면(기본공격)
        if (!dead && dist < attackRange)
        {
            StopPathFinder(true);
          
            canMove = false; //공격 반경 안에 있으면 움직임을 멈춘다.

            if (lastAttackTime + attackDelay <= Time.time) //최근 공격 시점에서 attackDelay 이상 시간이 지나면 공격 가능
            {
                canAttack = true;
            }
            else//공격 반경 안에 있지만, 딜레이가 남아있을 경우
            {
                canAttack = false;
            }
        }
        else//공격 반경 밖에 있을 경우 추적하기
        {
            int ranAction = Random.Range(0, 30);
            if (ranAction == 3)
            {
                StartCoroutine(FisrtSkill());
            }
            else if (ranAction == 10)
            {
                if (NextPageOn)
                    StartCoroutine(SecondSkill());
            }
            else if (ranAction == 15)
            {
                if (NextPageOn)
                    StartCoroutine(TrapOn());
            }
            else
            {
                canMove = true;
                canAttack = false;

                //계속 추적
                if (!_IsInFirstSkill)
                    StopPathFinder(false);
                pathFinder.SetDestination(targetEntity.transform.position);
            }
        }
    }

    IEnumerator TrapOn()
    {
        TrapOnTime = true;
        for(int i =0; i<8;i++)
        {          
            skill_Trap.SetActive(false);
            yield return new WaitForSeconds(1.2f);
            skill_Trap.SetActive(true);
            SoundManager.inst.SFXPlay("EKrEKr", SoundManager.inst.bossList[2]);
            yield return new WaitForSeconds(1.2f);
            skill_Trap.SetActive(false);
        }     
        TrapOnTime = false;        
    }

    IEnumerator StunOn()
    {
        StunEffect.SetActive(true);
        AllStop = true;
        StopPathFinder(true);
        canMove = false;
        canAttack = false;
        canStun = true;
        yield return new WaitForSeconds(5f);
        StunEffect.SetActive(false);
        canStun = false;
        _StunOn = false;
        AllStop = false;
        Player.inst.TrapTarget.SetActive(true);//기절시간이 끝난 뒤 플레이어에게 해골표시
    }

    IEnumerator FisrtSkill() //1페이지 첫 번째 스킬
    {
        _IsInFirstSkill = true;
        StopPathFinder(true);
        canMove = false;
        canAttack = false;
        bossAnimator.SetTrigger("FirstSkill");
        yield return new WaitForSeconds(1.25f);
        _IsInFirstSkill = false;
    }
    
    IEnumerator SecondSkill() //2페이지 돌입 시 첫 번째 스킬과 같이 쓰임
    {
        _IsInFirstSkill = true;
        StopPathFinder(true);
        canMove = false;
        canAttack = false;
        bossAnimator.SetTrigger("SecondSkill");
        yield return new WaitForSeconds(3.1f);
        _IsInFirstSkill = false;
    }

    //유니티 애니메이션 이벤트로 휘두를 때 데미지 적용시키기
    public void OnDamageEvent()
    {
        //공격 대상을 지정할 추적 대상의 LivingEntity 컴포넌트 가져오기
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();
        
        //공격 처리(플레이어에게)
        attackTarget.OnDamage(damage);
        SoundManager.inst.SFXPlay("attack", SoundManager.inst.bossList[0]);
        //최근 공격 시간 갱신
        lastAttackTime = Time.time;     
    }

    //첫번째 스킬 모션 발동 할때 이벤트로 함수 실행
    public void OnFirstSkillEvent() 
    {        
        GameObject firstSkill = Instantiate(skill_First, skill_FirstPos.transform.position, Quaternion.identity);
        firstSkill.transform.forward = skill_FirstPos.transform.forward;
        Destroy(firstSkill, 2f);
    }

    //두번째 스킬 모션 발동 할때 이벤트로 함수 실행
    public void OnSecondSkillEvent()
    {
        GameObject secondSkill = Instantiate(skill_Second, skill_FirstPos.transform.position, Quaternion.identity);
        Destroy(secondSkill, 2f);
    }

    //데미지를 입었을 때 실행할 처리(재정의)
    public override void OnDamage(float damage)
    {
        GameObject hubText = Instantiate(damageText, transform.position, Quaternion.identity, enemyHpBarCanvas.transform);
        var _hubText = hubText.GetComponent<DamageText>();

        _hubText.enemyTr = this.gameObject.transform;
        _hubText.offset = DamageOffset;
        _hubText.damage = damage;

        //LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage); //base, 부모클래스에 접근하는 기능   

        bossHpBarSlider.value = health;
        bossHpText.text = string.Format("{0}", health);
    }

    //사망 처리
    public override void Die()
    {
        bossHpBarSlider.gameObject.SetActive(false);
        Player.inst.TrapTarget.SetActive(false);
        //다른 AI를 방해하지 않도록 자신의 모든 콜라이더를 비활성화
        Collider[] enemyColliders = GetComponents<Collider>();
        GameObject hi = Instantiate(_item, transform.position + transform.up * 2, Quaternion.identity);
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }

        //AI추적을 중지하고 네비메쉬 컴포넌트를 비활성화
        StopPathFinder(true);
        pathFinder.enabled = false;

        canMove = false;
        canAttack = false;
        NextPageOn = false;
        NpcManager.inst.roomBoss = false;
        NpcManager.inst.PotalCheck();
        //사망 애니메이션 재생
        bossAnimator.SetTrigger("doDie");

        //LivingEntity의 DIe()를 실행하여 기본 사망 처리 실행
        base.Die();
    }

    //bossHpBarSlider 활성화
    protected override void OnEnable()
    {
        //LivingEntity의 OnEnable() 실행(상태초기화)
        base.OnEnable();

        bossHpBarSlider.gameObject.SetActive(true);
        bossHpBarSlider.maxValue = startingHealth;
        //체력 슬라이더의 값을 현재 체력값으로 변경
        bossHpBarSlider.value = health;        
    }
}