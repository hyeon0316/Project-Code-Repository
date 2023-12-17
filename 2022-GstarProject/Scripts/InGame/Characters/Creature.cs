using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Creature : MonoBehaviour
{
    public Stat Stat;
    public bool IsDead { get; protected set; }

    [Header("기본공격 범위")]
    [SerializeField] protected float _attackRadius; //공격 범위
    [SerializeField] protected FloatingText _floatingText;

    [SerializeField] protected Animator _anim;
    [SerializeField] protected NavMeshAgent _nav;
    [SerializeField] protected Rigidbody _rigid;

    protected virtual void Start()
    {
        Stat.CurHp.Where(curHp => curHp == 0).Subscribe(_ =>
        {
            Die();
        });
    }

    protected virtual void Init()
    {
        IsDead = false;
        Stat.InitHp();
    }

    protected virtual void Clear()
    {
        _floatingText.ClearText();
    }

    public void Heal(int amount)
    {
        Stat.Heal(amount);
    }


    /// <summary>
    /// 공격을 받았을때
    /// </summary>
    public void TryGetDamage(Stat stat, Attack attack)
    {
        if (!IsDead)
        {
            if (IsHit(stat.HitPercent))
            {
                TakeDamage(attack.CalculateDamage(stat), stat.Attack);
            }
        }
    }


    /// <summary>
    /// 최종적으로 데미지를 받음
    /// </summary>
    /// <param name="amount">계산된 데미지</param>
    /// <param name="pureDamage">순수 공격력</param>
    protected virtual void TakeDamage(int amount, int pureDamage)
    {
        int resultDamage = Mathf.Clamp((pureDamage - Stat.Defense) / 2, 0, 100) * amount / 100;
        Stat.TakeDamage(resultDamage);
        _floatingText.CreateFloatingText(resultDamage.ToString());
    }


    /// <summary>
    /// 상대의 명중률과 자신의 회피율을 통해 데미지를 받아야 하는지에 대한 계산
    /// </summary>
    private bool IsHit(int hitPercent)
    {
        //데미지 계산
        int n = hitPercent - Stat.Dodge + 10;
        n = Math.Clamp(n, 0, 60);
        double result = Math.Pow(0.91f, n);
        result = result * -1;
        result = (result + 1) * 100;

        if (Random.Range(0f, 100f) < result)
        {
            return true;
        }
        else
        {
            _floatingText.CreateFloatingText(Global.MissText);
            return false;
        }
    }

    protected virtual void Die()
    {
        _floatingText.ClearText();
        IsDead = true;
        this.gameObject.layer = LayerMask.NameToLayer("Dead");
    }

}
