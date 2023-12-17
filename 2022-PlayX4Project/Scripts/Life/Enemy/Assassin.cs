using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class Assassin : Enemy, IEnemyMove, IEnemyAttack
{
    public Enemystate Enemystate;

    private float _attackDelay;

    private EnemyAttack _enemyAttack;

    private NavMeshAgent _EnemyNav;

    public float Attackcrossroad;

    private bool SkillOneChance;

    private float _findDistance;

    protected override void Awake()
    {
        base.Awake();
        Enemystate = Enemystate.Attack;
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
                Moving();
                LookPlayer();
            }

            if (_EnemyNav.enabled)
                _rigid.velocity = Vector3.zero;
        }
    }

    public void FindPlayer()
    {
        if (Vector3.Distance(PlayerManager.Instance.Player.transform.position, this.transform.position) < _findDistance)
        {
            if (Enemystate != Enemystate.Attack)
            {
                Enemystate = Enemystate.Find;
                _anim.SetBool("isRun", true);
            }
        }
        else
        {
            Enemystate = Enemystate.Idle;
            _anim.SetBool("isRun", false);
        }
    }

    public void Fieldofview()
    {
        if (Enemystate == Enemystate.Find)
        {
            if (Vector3.Distance(PlayerManager.Instance.Player.transform.position, this.transform.position) < Attackcrossroad + 0.25f)
            {
                if (_attackDelay <= 0)
                {
                    _attackDelay = 2f;
                    Enemystate = Enemystate.Attack;
                    _anim.SetTrigger("AttackTrigger");
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
            if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Assassin_Attack")
                && _anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f)
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
        GameObject obj = Instantiate(CachingManager.Instance().BloodObj, this.transform.position, Quaternion.identity);
        obj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        yield return new WaitForSeconds(0.7f);
        Destroy(obj);
    }
    
    /// <summary>
    /// 적 공격관련 스크립트
    /// </summary>
    public void Attack(float dmg)
    {
        if (_enemyAttack.IshitPlayer )
        {
           PlayerManager.Instance.Player.GetDamage(Power, dmg);
        }
    }

    IEnumerator SkillOne()
    {
        _attackDelay = 10f;
        Enemystate = Enemystate.Attack;
        _anim.SetBool("Skill", true);//순간이동
        _EnemyNav.isStopped = true;
        yield return new WaitForSecondsRealtime(0.64f);
       
        if (PlayerManager.Instance.Player.transform.GetChild(0).localScale.x < 0)
        {
            this.transform.position =PlayerManager.Instance.Player.transform.position + Vector3.right * Attackcrossroad;
        }
        else
        {
            this.transform.position =PlayerManager.Instance.Player.transform.position + Vector3.left * Attackcrossroad;
        }
        _anim.SetTrigger("SkillOneTrigger");//공격
        _anim.SetBool("Skill", false);

        yield return new WaitForSecondsRealtime(0.15f);
        _EnemyNav.isStopped = false;
        _attackDelay = 0.4f;
        Enemystate = Enemystate.Idle;
        _findDistance = 10;
    }

    /// <summary>
    /// 적 행동 관련 스크립트
    /// </summary>
    public void Moving()
    {
        if (Enemystate == Enemystate.Find)
        {
            _EnemyNav.isStopped = false;
            if (_attackDelay <= 0)
            {
                _EnemyNav.speed = Speed;
                _EnemyNav.SetDestination(PlayerManager.Instance.Player.transform.position);
            }
            else
            {
                Vector3 position = new Vector3(
                   PlayerManager.Instance.Player.transform.GetChild(0).localScale.x == 2.5f
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
        if (!_anim.GetCurrentAnimatorStateInfo(0).IsName("Assassin_Ambush"))
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
            if (PlayerManager.Instance.Player.transform.position.x > this.transform.position.x)
            {
                this.transform.GetChild(0).localScale = new Vector3(-thisScale.x, thisScale.y, thisScale.z);
            }
            else
            {
                this.transform.GetChild(0).localScale = new Vector3(thisScale.x, thisScale.y, thisScale.z);
            }
        }
    }

    protected override void CallDamageEvent()
    {
        throw new System.NotImplementedException();
    }

    protected override IEnumerator DeadEventCo()
    {
        Enemystate = Enemystate.Dead;
        _EnemyNav.path.ClearCorners();
        _EnemyNav.enabled = false;
        while (true)
        {
            _EnemyNav.path.ClearCorners();
            _EnemyNav.enabled = false;
            if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Assassin_Death")
                && _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        _EnemyNav.enabled = false;
        Destroy(this.transform.gameObject, Time.deltaTime);
    }
}
