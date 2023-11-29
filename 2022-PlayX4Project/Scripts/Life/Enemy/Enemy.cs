using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : Life
{
    [SerializeField] protected bool _isCheckClear = true;
    [SerializeField] private Transform _bloodEffectPos;


    private void OnDestroy()
    {
        if (_isCheckClear)
        {
            if (transform.parent.childCount == 1)
            {
                transform.parent.GetComponent<EnemyStage>().CallEndEvent();
            }
        }
    }


    public void KnockBackRight(Vector3 EnemyPos, float Power)
    {
        Debug.Log("knockright");
        Vector3 nomal = EnemyPos.x > 0 ? Vector3.right : Vector3.left;
        Vector3 vector3 = new Vector3(nomal.x, 0.4f, 0);
        _rigid.velocity = Vector3.zero;
        _rigid.AddForce(vector3 * Power, ForceMode.Impulse);
    }

    public void KnockBackUp(Vector3 EnemyPos, float Power)
    {
        Debug.Log("knockup");
        Vector3 nomal = EnemyPos.x > 0 ? Vector3.right : Vector3.left;
        Vector3 vector3 = new Vector3(nomal.x * 0.3f, 1.2f, 0);
        Debug.Log(vector3);
        _rigid.velocity = Vector3.zero;
        _rigid.AddForce(vector3 * Power, ForceMode.Impulse);
    }

    public void KnockBackRightUp(Vector3 EnemyPos, float Power)
    {
        Debug.Log("knockrightup");
        Vector3 nomal = EnemyPos.x > 0 ? Vector3.right : Vector3.left;
        Vector3 vector3 = new Vector3(nomal.x * 0.5f, 0.8f, 0);
        _rigid.velocity = Vector3.zero;
        _rigid.AddForce(vector3 * Power, ForceMode.Impulse);
    }


    public void NA_stop(float time)
    {
        StartCoroutine(StopNavCo(0.065f * time));
        StartCoroutine(StopGravityCo(0.03f * time));
    }


    public IEnumerator StopNavCo(float time)
    {
        //GetComponent<I_EnemyControl>()._enemystate = Enemystate.Stop;
        GetComponent<NavMeshAgent>().enabled = false;

        yield return new WaitForSeconds(time);
        if (Hp > 0)
        {
            GetComponent<NavMeshAgent>().enabled = true;
            //GetComponent<I_EnemyControl>()._enemystate = Enemystate.Idle;
        }
    }

    public IEnumerator StopAnim(float time)
    {
        _rigid.velocity = Vector3.zero;
        yield return new WaitForSeconds(0.023f);

        _anim.speed = 0;
        Debug.LogFormat("{0},애니멈춤", time);
        yield return new WaitForSeconds(time);

        _anim.speed = 1;
    }

    public IEnumerator StopGravityCo(float time)
    {
        yield return new WaitForEndOfFrame();

        _rigid.useGravity = false;
        yield return new WaitForSeconds(time);
        _rigid.useGravity = true;
    }

    public override bool GetDamage(float dmg, float addDmg)
    {
        if(_hp > 0)
        {
            CallDamageEvent();
            _anim.SetTrigger("Hit");
            _hp -= dmg * addDmg;
            return CheckLiving();
        }
        return false;
    }

    protected abstract void CallDamageEvent();
}

public enum Enemystate
{
    Idle,
    Find,
    Attack,
    Skill,
    Skill2,
    Range,
    Stop,
    Dead
}

