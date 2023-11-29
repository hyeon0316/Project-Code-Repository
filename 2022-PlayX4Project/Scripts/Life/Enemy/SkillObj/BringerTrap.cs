using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BringerTrap : MonoBehaviour
{
   private int _power = 10;

   private float _timer;

   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         other.GetComponent<Life>().GetDamage(_power , 1);
      }
   }
   
   private void OnEnable()
   {
      _timer = 0;
      transform.position =PlayerManager.Instance.Player.transform.position + new Vector3(0,0.7f,0);
      SoundManager.Instance.Play("Enemy/Bringer/BringerTrap",SoundType.Effect);
   }
   
   private void Update()
   {
      _timer += Time.deltaTime;
      if(_timer >=1.5f)
         gameObject.SetActive(false);
   }
}
