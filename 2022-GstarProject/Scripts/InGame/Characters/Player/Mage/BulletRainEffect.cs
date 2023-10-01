using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRainEffect : MonoBehaviour
{
   private AudioSource _audioSource;

   private void Awake()
   {
      _audioSource = GetComponent<AudioSource>();
   }

   public void DelayDisable()
   {
      SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BulletRainHit);
      Invoke("DisableEffect", 1.5f);
   }
   
   private void DisableEffect()
   {
      ObjectPoolManager.Instance.ReturnObject(PoolType.BulletRainEffect, this.gameObject);
   }
}
