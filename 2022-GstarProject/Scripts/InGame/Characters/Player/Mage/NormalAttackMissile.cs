using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class NormalAttackMissile : LongAttack
{
   [SerializeField] private float _missileSpeed;
   private Transform _target;
 

   private void FixedUpdate()
   {
      MoveToTarget();
   }

   private void MoveToTarget()
   {
      if ((transform.position - _target.position).sqrMagnitude <= 1)
      {
         _target.transform.GetComponent<Creature>().TryGetDamage(_stat, this);
         CreateEffect();
         CallDisableEvent();
         return;
      }

      var pos = _target.transform.position - transform.position;
      transform.rotation = Quaternion.LookRotation(pos);
      transform.Translate(Vector3.forward * _missileSpeed * Time.fixedDeltaTime);
   }

   public void Init(Transform target)
   {
      _target = target;
      //Invoke("DisableObject", 1f);
   }

   private void CreateEffect()
   {
      GameObject effect = ObjectPoolManager.Instance.GetObject(PoolType.NormalAttackEffect);
      effect.transform.position = this.transform.position;
      effect.GetComponent<NormalAttackEffect>().DelayDisable();
   }
}
