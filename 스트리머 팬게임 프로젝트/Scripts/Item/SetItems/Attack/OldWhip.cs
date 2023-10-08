using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// 낡은 채찍
/// </summary>
public class OldWhip : Item 
{
   private int _damage;

   public override void Active(BattleCharacter player, BattleCharacter opponent)
   {
      player.AttackEnemy();
   }


   public override void ApplyUpgrade()
   {
      switch (Upgrade)
      {
         case 0:
            _damage = 5;
            break;
         case 1:
            _damage = 8;
            break;
         case 2:
            _damage = 12;
            break;
      }
   }
   public override void SetEquipEffectText()
   {
      _effect = $"<color=yellow>{_damage}</color> 데미지를 가합니다.";
   }
   
}
