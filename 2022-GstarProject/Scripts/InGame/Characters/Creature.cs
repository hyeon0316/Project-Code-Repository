using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public enum LayerType
{
    Player,
    Enemy
}

public abstract class Creature : MonoBehaviour
{
  public Stat Stat;

  protected Animator _animator;
  protected NavMeshAgent _nav;

  [Header("자신의 체력 바")]
  [SerializeField] protected HpbarController _hpbar;
  
  [Header("기본공격 범위")]
  [SerializeField] protected float _attackRadius; //실제 멈춰서서 공격하는 범위
  
  
  [SerializeField] protected FloatingText _floatingText;
  
  protected List<Transform> _targets = new List<Transform>(); //탐색된 적의 정보

  public List<Transform> Targets => _targets;

  public bool IsDead { get; protected set; }
  
  protected Rigidbody _rigid;
  
  protected virtual void Awake()
  {
      _animator = GetComponent<Animator>();
      _nav = GetComponent<NavMeshAgent>();
      _rigid = GetComponent<Rigidbody>();
  }

  protected virtual void OnDisable()
  {
      IsDead = true;
      _floatingText.ClearText();
  }


  /// <summary>
  /// 플레이어가 죽었을때 등 초기화 해야할 상황이 생길 때 사용
  /// </summary>
  protected virtual void Init()
  {
      IsDead = false;
      Stat.Hp = Stat.MaxHp;
      UpdateHpBar();
  }

  public void UpdateHpBar()
  {
      _hpbar.UpdateHpBar(Stat.Hp, $"{Stat.Hp} / {Stat.MaxHp}", Stat.MaxHp);
  }
  
  public void Heal(int amount)
  {
      Stat.Hp += amount;

      if (Stat.MaxHp < Stat.Hp)
          Stat.Hp = Stat.MaxHp;
      
      UpdateHpBar();
  }
  
  
  /// <summary>
  /// 공격을 받았을때
  /// </summary>
  public virtual void TryGetDamage(Stat stat, Attack attack)
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
 private void TakeDamage(int amount, int pureDamage)
 {
     int resultDamage = Mathf.Clamp((pureDamage - Stat.Defense) / 2, 0, 100) * amount / 100;
     Stat.Hp -= resultDamage;
     _floatingText.CreateFloatingText(resultDamage.ToString());

     if (Stat.Hp <= 0)
     {
         Stat.Hp = 0;
         Die();
     }
 }
 

  /// <summary>
  /// 상대의 명중률과 자신의 회피율을 통해 데미지를 받아야 하는지에 대한 계산
  /// </summary>
  private bool IsHit(int hitPercent)
  {
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
