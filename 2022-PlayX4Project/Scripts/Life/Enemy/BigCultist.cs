using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BigCultist : Enemy, IEnemyMove, IEnemyAttack
{
    private Enemystate Enemystate;

    private float _attackDelay;

    private EnemyAttack _enemyAttack;

    private UnityEngine.AI.NavMeshAgent _EnemyNav;

    public float Attackcrossroad;

    protected override void Awake()
    {
        base.Awake();
        Enemystate = Enemystate.Attack;
        _enemyAttack = this.GetComponentInChildren<EnemyAttack>();
        _EnemyNav = this.GetComponent<NavMeshAgent>();
        _EnemyNav.stoppingDistance = Attackcrossroad;
    }

    // Update is called once per frame
    void Update()
    {
        if(Enemystate != Enemystate.Stop) {
            _anim.SetBool("Hitstop", false);
            if (Enemystate != Enemystate.Dead )
            {
                if (_attackDelay > 0)
                    _attackDelay -= Time.deltaTime;

                FindPlayer();
                Fieldofview();
                Moving();
            }
            if (_EnemyNav.enabled)
               _rigid.velocity = Vector3.zero;
        }
       
    }

    public void FindPlayer()
    {
        if (Vector3.Distance(PlayerManager.Instance.Player.transform.position, this.transform.position) < 5f)
        {
            if (Enemystate != Enemystate.Attack && _attackDelay <= 0)
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
                _anim.SetBool("isRun", false);
                if (_attackDelay <= 0)
                {
                    _attackDelay = 4f;
                    Enemystate = Enemystate.Attack;
                    _anim.SetTrigger("AttackTrigger");
                }
            }
            else
            {
                _anim.SetBool("isRun", true);
            }
          
        }

        if (Enemystate == Enemystate.Attack)
        {
            if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Big-Cultist_Attack")
                && _anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f)
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
        GameObject obj = Instantiate(CachingManager.Instance().BloodObj, this.transform.position, Quaternion.identity);
        obj.transform.localScale = new Vector3(1f, 1f, 1f);
        yield return new WaitForSeconds(0.7f);
        Destroy(obj);
    }

    public void Attack(float dmg)
    {
        if (_enemyAttack.IshitPlayer )
        {
            Debug.LogFormat("{0},{1}", this.name, "hit");
           PlayerManager.Instance.Player.KnockBack(this.transform.position);
           PlayerManager.Instance.Player.GetDamage(Power,dmg);
        }
    }

    public void Moving()
    {
        if (Enemystate == Enemystate.Find && _attackDelay <= 0)
        {
            _EnemyNav.isStopped = false;
            _anim.SetBool("isRun", true);
            _EnemyNav.speed = Speed;
            _EnemyNav.SetDestination(PlayerManager.Instance.Player.transform.position);
            
        }
        else
        {
            _anim.SetBool("isRun", false);
            _EnemyNav.isStopped = true;
            _EnemyNav.path.ClearCorners();

        }


        //적 보는 방향 전환라인
        Vector3 thisScale = new Vector3(2.5f, 2.5f, 1);
        if (_EnemyNav.pathEndPosition.x > this.transform.position.x)
        {

            this.transform.GetChild(0).localScale = new Vector3(-thisScale.x, thisScale.y, thisScale.z);
        }
        else if(_EnemyNav.pathEndPosition.x < this.transform.position.x)
        {
            this.transform.GetChild(0).localScale = new Vector3(thisScale.x, thisScale.y, thisScale.z);
        }
    }

    protected override void CallDamageEvent()
    {
        if (_attackDelay < 0.06f)
        {
            _attackDelay += 0.06f;
        }
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
            if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Big-Cultist_Death")
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
