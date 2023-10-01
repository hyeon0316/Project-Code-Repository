using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpikeAttack : SkillAttack
{
   private AudioSource _audioSource;

   private void Awake()
   {
      _audioSource = GetComponent<AudioSource>();
   }

   public void DelayDisable()
   {
      SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.SpikeAttack);
      Invoke("DisableObject", 3f);
   }
   
   
   private void OnTriggerEnter(Collider other)
   {
      if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
      {
         other.GetComponent<Creature>().TryGetDamage(DataManager.Instance.Player.Stat,this);
      }
   }
}
