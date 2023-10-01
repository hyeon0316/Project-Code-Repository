using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cultist : Life, I_hp, I_EnemyControl
{
    static public GameObject PlayerObj;

    public GameObject BloodPrefab;

    private Enemystate Enemystate;

    public Enemystate _enemystate
    {
        get { return Enemystate; }
        set { Enemystate = value; }
    }

    private float _attackDelay;

    public Animator Animator;

    private EnemyAttack _enemyAttack;

    private UnityEngine.AI.NavMeshAgent _EnemyNav;

    public float Attackcrossroad;

    public GameObject[] FireballMem = new GameObject[10];

    // Start is called before the first frame update
    private void Awake()
    {
        Initdata(2,DataManager.Instance().Data.CultistHp, DataManager.Instance().Data.CultistPower, DataManager.Instance().Data.CultistSpeed);//데이터 입력
        Enemystate = Enemystate.Attack;
        PlayerObj = GameObject.Find("Player");
        Animator = this.GetComponentInChildren<Animator>();
        _enemyAttack = this.GetComponentInChildren<EnemyAttack>();
        _EnemyNav = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        _EnemyNav.stoppingDistance = Attackcrossroad;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Enemystate != Enemystate.Stop)
        {
            if (Enemystate != Enemystate.Dead)
            {
                if (_attackDelay > 0)
                    _attackDelay -= Time.deltaTime;

                FindPlayer();
                Fieldofview();
                EnemyMove();
                if(Enemystate != Enemystate.Attack)
                    LookPlayer();
            }
            if (_EnemyNav.enabled)
                GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    public void FindPlayer()
    {
        if (Vector3.Distance(PlayerObj.transform.position, this.transform.position) < 5f)
        {
            if (Enemystate != Enemystate.Attack)
            {
                if (_attackDelay <= 0) { 
                
                    Enemystate = Enemystate.Find;
                    Animator.SetBool("isWalk", true);
                }
                else
                {
                    Enemystate = Enemystate.Idle;
                    Animator.SetBool("isWalk", false);
                }
            }
        }
        else
        {
            Enemystate = Enemystate.Idle;
            Animator.SetBool("isWalk", false);
        }
    }

    public void Fieldofview()
    {
        if (Enemystate == Enemystate.Find)
        {
            if (Mathf.Abs(PlayerObj.transform.position.z - this.transform.position.z) < 0.3f)
            {
                if (_attackDelay <= 0)
                {
                    _attackDelay = 5f;
                    Enemystate = Enemystate.Attack;
                    Animator.SetTrigger("AttackTrigger");
                }
            }
        }
        if (Enemystate == Enemystate.Attack)
        {
            if ((Animator.GetCurrentAnimatorStateInfo(0).IsName("Cultist_Attack") || Animator.GetCurrentAnimatorStateInfo(0).IsName("Cultist_farawayAttack"))
                && Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f)
            {
                Enemystate = Enemystate.Find;
            }
            if (_attackDelay <= 0)
            {
                Enemystate = Enemystate.Find;
            }
        }
    }

    private IEnumerator BloodCo()
    {
        GameObject obj = Instantiate(BloodPrefab, this.transform.position, Quaternion.identity);
        obj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        yield return new WaitForSeconds(0.7f);
        Destroy(obj);
    }
    
    public void SelectHit(AttackHitSoundType type)
    {
        switch (type)
        {
            case AttackHitSoundType.ZHit1:
                FindObjectOfType<SoundManager>().Play("Player/ZAttackHit1",SoundType.Effect);
                break;
            case AttackHitSoundType.ZHit2:
                FindObjectOfType<SoundManager>().Play("Player/ZAttackHit2",SoundType.Effect);
                break;
            case AttackHitSoundType.XHit:
                FindObjectOfType<SoundManager>().Play("Player/XAttackHit",SoundType.Effect);
                break;
            case AttackHitSoundType.AHit:
                FindObjectOfType<SoundManager>().Play("Player/BulletHit",SoundType.Effect);
                break;
            case AttackHitSoundType.SHit:
                FindObjectOfType<SoundManager>().Play("Player/DashAttackHit",SoundType.Effect);
                StartCoroutine(BloodCo());
                break;
        }
    }

    public bool Gethit(float Cvalue, float coefficient)
    {
        if (HP > 0)
        {
            if (Cvalue > 0)
            {
                if (_attackDelay < 0.06f)
                    _attackDelay += 0.06f;
                Animator.SetTrigger("Hitstart");
            }
            HP -= Cvalue * coefficient;
            return CheckLiving();
        }
        return false;
    }

    public bool CheckLiving()
    {
        if (HP <= 0)
        {
            Animator.SetBool("Dead", true);
            StartCoroutine(DeadAniPlayer());
            return true;
        }
        else
            return false;
    }

    public IEnumerator DeadAniPlayer()
    {
        Living = false;
        Enemystate = Enemystate.Dead;
        _EnemyNav.path.ClearCorners();
        _EnemyNav.enabled = false;
        while (true)
        {
            _EnemyNav.path.ClearCorners();
            _EnemyNav.enabled = false;
            if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Cultist_Death")
                && Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        _EnemyNav.enabled = false;
        Destroy(this.transform.gameObject, Time.deltaTime);
    }

    public void EnemyAttack(float coefficient)
    {
        int index = FireballMemorypool();
        GameObject InsFireball = FireballMem[index];
        InsFireball.SetActive(true);
        InsFireball.transform.position = this.transform.position;
        if(this.transform.GetChild(0).localScale.x < 0)
        {
            InsFireball.transform.rotation = new Quaternion(0,180,0,0);
        }
        else
        {
            InsFireball.transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        InsFireball.GetComponent<FireBall>().Power = Power * coefficient;
    }

    /// <summary>
    /// 메모리풀 사용준비
    /// </summary>
    public int FireballMemorypool()
    {
        int index = -1;

        for(int i = 0; i < FireballMem.Length; ++i) { 
            if(!FireballMem[i].activeSelf)
            {
                index = i;
                break;
            }
        }
        return index;
    }
    public void EnemyMove()
    {
        if (Enemystate == Enemystate.Find)
        {
            _EnemyNav.isStopped = false;
            if (_attackDelay <= 0)
            {
                _EnemyNav.speed = Speed;
                Vector3 navPosition;
                if (Vector3.Distance(this.transform.position, PlayerObj.transform.position) > 3f)
                {
                    navPosition = PlayerObj.transform.position;
                }
                else
                {
                    navPosition = new Vector3(this.transform.position.x, this.transform.position.y,
                        PlayerObj.transform.position.z);
                }
                _EnemyNav.SetDestination(navPosition);
            }
            else
            {
                _EnemyNav.isStopped = true;
                Enemystate = Enemystate.Idle;
            }
        }
        else
        {
            _EnemyNav.isStopped = true;
        }
    }

    private void LookPlayer()
    {
        //적 보는 방향 전환라인
        Vector3 thisScale = new Vector3(2.5f, 2.5f, 1);
        if (PlayerObj.transform.position.x > this.transform.position.x)
        {
            this.transform.GetChild(0).localScale = new Vector3(-thisScale.x, thisScale.y, thisScale.z);
        }
        else
        {
            this.transform.GetChild(0).localScale = new Vector3(thisScale.x, thisScale.y, thisScale.z);
        }
    }
    
}
