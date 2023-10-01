using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Assassin : Life, I_hp, I_EnemyControl
{
    static public GameObject PlayerObj;
    public GameObject BloodPrefab;

    public Enemystate Enemystate;

    public Enemystate _enemystate
    {
        get { return Enemystate; }
        set { Enemystate = value; }
    }

    private float _attackDelay;

    public Animator Animator;

    private EnemyAttack _enemyAttack;

    private NavMeshAgent _EnemyNav;

    public float Attackcrossroad;

    private bool SkillOneChance;

    private float _findDistance;

    public void Awake()
    {
        Initdata(1,DataManager.Instance().Data.AssassinHp, DataManager.Instance().Data.AssassinPower, DataManager.Instance().Data.AssassinSpeed);//데이터 입력
        Enemystate = Enemystate.Attack;
        PlayerObj = GameObject.Find("Player");
        Animator = this.GetComponentInChildren<Animator>();
        _enemyAttack = this.GetComponentInChildren<EnemyAttack>();
        _EnemyNav = this.GetComponent<NavMeshAgent>();
        _EnemyNav.stoppingDistance = Attackcrossroad;
        SkillOneChance = true;
        _findDistance = 5f;
    }

    public void Update()
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
                LookPlayer();
            }

            if(_EnemyNav.enabled)
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        else
        {

        }
    }

    public void FindPlayer()
    {
        if (Vector3.Distance(PlayerObj.transform.position, this.transform.position) < _findDistance)
        {
            if (Enemystate != Enemystate.Attack)
            {
                Enemystate = Enemystate.Find;
                Animator.SetBool("isRun", true);
            }
        }
        else
        {
            Enemystate = Enemystate.Idle;
            Animator.SetBool("isRun", false);
        }
    }

    public void Fieldofview()
    {
        if (Enemystate == Enemystate.Find)
        {
            if (Vector3.Distance(PlayerObj.transform.position, this.transform.position) < Attackcrossroad + 0.25f)
            {
                if (_attackDelay <= 0)
                {
                    _attackDelay = 2f;
                    Enemystate = Enemystate.Attack;
                    Animator.SetTrigger("AttackTrigger");
                }
            }

            if (SkillOneChance)
            {
                StartCoroutine(SkillOne());
                SkillOneChance = false;
            }
        }

        if (Enemystate == Enemystate.Attack)
        {
            if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Assassin_Attack")
                && Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f)
            {
                Enemystate = Enemystate.Find;
            }

            if(_attackDelay <= 0)
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
        Enemystate = Enemystate.Dead;
        Living = false;
        _EnemyNav.path.ClearCorners();
        _EnemyNav.enabled = false;
        while (true)
        {
            _EnemyNav.path.ClearCorners();
            _EnemyNav.enabled = false;
            if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Assassin_Death")
                && Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        _EnemyNav.enabled = false;
        Destroy(this.transform.gameObject,Time.deltaTime);
    }

    /// <summary>
    /// 적 공격관련 스크립트
    /// </summary>
    public void EnemyAttack(float coefficient)
    {
        if (_enemyAttack.IshitPlayer )
        {
            PlayerObj.GetComponent<I_hp>().Gethit(Power,coefficient);
        }
    }

    IEnumerator SkillOne()
    {
        _attackDelay = 10f;
        _enemystate = Enemystate.Attack;
        Animator.SetBool("Skill", true);//순간이동
        _EnemyNav.isStopped = true;
        yield return new WaitForSecondsRealtime(0.64f);
       
        if (PlayerObj.transform.GetChild(0).localScale.x < 0)
        {
            this.transform.position = PlayerObj.transform.position + Vector3.right * Attackcrossroad;
        }
        else
        {
            this.transform.position = PlayerObj.transform.position + Vector3.left * Attackcrossroad;
        }
        Animator.SetTrigger("SkillOneTrigger");//공격
        Animator.SetBool("Skill", false);

        yield return new WaitForSecondsRealtime(0.15f);
        _EnemyNav.isStopped = false;
        _attackDelay = 0.4f;
        _enemystate = Enemystate.Idle;
        _findDistance = 10;
    }

    /// <summary>
    /// 적 행동 관련 스크립트
    /// </summary>
    public void EnemyMove()
    {
        if (Enemystate == Enemystate.Find)
        {
            _EnemyNav.isStopped = false;
            if (_attackDelay <= 0)
            {
                _EnemyNav.speed = Speed;
                _EnemyNav.SetDestination(PlayerObj.transform.position);
            }
            else
            {
                Vector3 position = new Vector3(
                    PlayerObj.transform.GetChild(0).localScale.x == 2.5f
                        ? this.transform.position.x - 2f
                        : this.transform.position.x + 2f
                    , this.transform.position.y,
                    this.transform.position.z);
                _EnemyNav.speed = 1.8f; 
                _EnemyNav.SetDestination(position);
            }
        }
        else
        {
            _EnemyNav.isStopped = true;
        }
    }

    private void LookPlayer()
    {
        Vector3 thisScale = new Vector3(2.5f, 2.5f, 1);
        if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Assassin_Ambush"))
        {
            if (_EnemyNav.pathEndPosition.x > this.transform.position.x)
            {
                this.transform.GetChild(0).localScale = new Vector3(-thisScale.x, thisScale.y, thisScale.z);
            }
            else
            {
                this.transform.GetChild(0).localScale = new Vector3(thisScale.x, thisScale.y, thisScale.z);
            }
        }
        else
        {
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
}
