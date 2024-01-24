using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalAttackEffect : MonoBehaviour, IPoolable
{
    public int InstanceId { get; set; }

    public void DelayDisable()
   {
      Invoke(nameof(DisableEffect), 1.5f);
   }
   
   private void DisableEffect()
   {
      ObjectPoolManager.Instance.ReturnObject(this.gameObject);
   }
}
