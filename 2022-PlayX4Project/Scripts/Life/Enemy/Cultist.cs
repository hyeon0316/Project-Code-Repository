using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cultist : Enemy, IEnemyMove, IEnemyAttack
{
    private Enemystate Enemystate;

    private float _attackDelay;

    private EnemyAttack _enemyAttack;

    private UnityEngine.AI.NavMeshAgent _EnemyNav;

    public float Attackcrossroad;

    public GameObject[] FireballMem = new GameObject[10];

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        Enemystate = Enemystate.Attack;
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
                Moving();
                if(Enemystate != Enemystate.Attack)
                    LookPlayer();
            }
            if (_EnemyNav.enabled)
                GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    public void FindPlayer()
    {
        if (Vector3.Distance(PlayerManager.Instance.Player.transform.position, this.transform.position) < 5f)
        {
            if (Enemystate != Enemystate.Attack)
            {
                if (_attackDelay <= 0) { 
                
                    Enemystate = Enemystate.Find;
                    _anim.SetBool("isWalk", true);
                }
                else
                {
                    Enemystate = Enemystate.Idle;
                    _anim.SetBool("isWalk", false);
                }
            }
        }
        else
        {
            Enemystate = Enemystate.Idle;
            _anim.SetBool("isWalk", false);
        }
    }

    public void Fieldofview()
    {
        if (Enemystate == Enemystate.Find)
        {
            if (Mathf.Abs(PlayerManager.Instance.Player.transform.position.z - this.transform.position.z) < 0.3f)
            {
                if (_attackDelay <= 0)
                {
                    _attackDelay = 5f;
                    Enemystate = Enemystate.Attack;
                    _anim.SetTrigger("AttackTrigger");
                }
            }
        }
        if (Enemystate == Enemystate.Attack)
        {
            if ((_anim.GetCurrentAnimatorStateInfo(0).IsName("Cultist_Attack") || _anim.GetCurrentAnimatorStateInfo(0).IsName("Cultist_farawayAttack"))
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
        obj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        yield return new WaitForSeconds(0.7f);
        Destroy(obj);
    }
    
    public void Attack(float dmg)
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
        InsFireball.GetComponent<FireBall>().Power = Power * dmg;
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
    public void Moving()
    {
        if (Enemystate == Enemystate.Find)
        {
            _EnemyNav.isStopped = false;
            if (_attackDelay <= 0)
            {
                _EnemyNav.speed = Speed;
                Vector3 navPosition;
                if (Vector3.Distance(this.transform.position,PlayerManager.Instance.Player.transform.position) > 3f)
                {
                    navPosition =PlayerManager.Instance.Player.transform.position;
                }
                else
                {
                    navPosition = new Vector3(this.transform.position.x, this.transform.position.y,
                       PlayerManager.Instance.Player.transform.position.z);
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
        if (PlayerManager.Instance.Player.transform.position.x > this.transform.position.x)
        {
            this.transform.GetChild(0).localScale = new Vector3(-thisScale.x, thisScale.y, thisScale.z);
        }
        else
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
            if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Cultist_Death")
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
