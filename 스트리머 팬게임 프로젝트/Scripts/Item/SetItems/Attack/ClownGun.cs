using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 광대의 권총
/// </summary>
public class ClownGun : Item
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
                _damage = 10;
                break;
            case 1:
                _damage = 15;
                break;
            case 2:
                _damage = 20;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_damage}</color> 데미지를 가합니다.\n상대방에게 <color=#4aa8d8>방어도</color>가 존재할 시 2배의 데미지를 가합니다.";
    }
}
