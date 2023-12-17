using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public sealed class Bringer : Enemy, IEnemyMove, IEnemyAttack
{
    [SerializeField] private EnemyAttack _enemyAttack;
    [SerializeField] private NavMeshAgent _EnemyNav;
    [SerializeField] private float _attackRange;

    private Enemystate Enemystate;
    private GameObject _skillObj;
    private float _attackDelay;
    private bool _canSkill;

    protected override void Awake()
    {
        base.Awake();
        Enemystate = Enemystate.Idle;
        _EnemyNav.stoppingDistance = _attackRange;
        _canSkill = true;
        _skillObj = CachingManager.Instance().BringerSkillObj;
    }

    private void Update()
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

                if (_canSkill && _hp <= MaxHp / 2)
                {
                    ActiveSkill();
                }
            }

            if (_EnemyNav.enabled)
            {
                _rigid.velocity = Vector3.zero;
            }
        }
    }

    /// <summary>
    /// 플레이어를 따라다니느 트랩스킬
    /// </summary>
    private void ActiveSkill()
    {
        _attackDelay += 2;
        _anim.SetBool("IsWalk", false);
        _anim.SetTrigger("Skill");
        Invoke(nameof(StartTracking), 2f);
        _canSkill = false;
    }

    private void StartTracking()
    {
        StartCoroutine(SkillTrackingCo());
    }

    private void FindPlayer()
    {
        if (Vector3.Distance(PlayerManager.Instance.Player.transform.position, this.transform.position) < 8f)
        {
            if (Enemystate != Enemystate.Attack)
            {
                if (_attackDelay <= 0)
                {
                    Enemystate = Enemystate.Find;
                    _anim.SetBool("IsWalk", true);
                }
                else
                {
                    Enemystate = Enemystate.Idle;
                    _anim.SetBool("IsWalk", false);
                }
            }
        }
        else
        {
            Enemystate = Enemystate.Idle;
            _anim.SetBool("IsWalk", false);
        }
    }

    private void Fieldofview()
    {
        if (Enemystate == Enemystate.Find)
        {
            if (Vector3.Distance(PlayerManager.Instance.Player.transform.position, this.transform.position) < _attackRange + 0.25f)
            {
                if (_attackDelay <= 0)
                {
                    _attackDelay = 1f;
                    Enemystate = Enemystate.Attack;
                    _anim.SetBool("IsWalk", false);
                    _anim.SetTrigger("Attack");
                }
            }
        }
        else if (Enemystate == Enemystate.Attack)
        {
            if ((_anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
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
        obj.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        yield return new WaitForSeconds(0.7f);
        Destroy(obj);
    }

    private IEnumerator SkillTrackingCo()
    {
        for (int i = 0; i < 8; i++)
        {
            _skillObj.SetActive(true);
            yield return new WaitForSeconds(2f);
            _skillObj.SetActive(false);
        }
    }

    protected override IEnumerator DeadEventCo()
    {
        Enemystate = Enemystate.Dead;
      
        _EnemyNav.enabled = false;
        while (true)
        {
            _EnemyNav.path.ClearCorners();
            if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Death")
                && _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        Destroy(this.gameObject, Time.deltaTime);
    }

    public void Attack(float dmg)
    {
        //적의 공격범위 콜리더가 플레이어 콜리더 범위에 들어왔을때 플레이어에게 데미지를 줌
        if (_enemyAttack.IshitPlayer)
        {
           PlayerManager.Instance.Player.GetDamage(Power, dmg);
        }
    }

    public void Moving()
    {
        if (Enemystate == Enemystate.Find)
        {
            if (_attackDelay <= 0)
            {
                _EnemyNav.isStopped = false;
                _EnemyNav.speed = Speed;
                _EnemyNav.SetDestination(PlayerManager.Instance.Player.transform.position);
            }
            else
            {
                _EnemyNav.isStopped = true;
                _EnemyNav.path.ClearCorners();
                Enemystate = Enemystate.Idle;
            }
        }
        else
        {
            _EnemyNav.isStopped = true;
            _EnemyNav.path.ClearCorners();
        }

        //적 보는 방향 전환라인, 공격 중일 때는 방향 전환x
        if (!_anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
            !_anim.GetCurrentAnimatorStateInfo(0).IsName("Skill"))
        {
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
    }

    protected override void CallDamageEvent()
    {
        if (_attackDelay < 0.06f)
        {
            _attackDelay += 0.06f;
        }
    }
}