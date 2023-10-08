using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 관심병사의 K2
/// </summary>
public class K2 : Item
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
                _damage = 6;
                break;
            case 1:
                _damage = 9;
                break;
            case 2:
                _damage = 13;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_damage}</color> 만큼 <color=#4aa8d8>방어도 관통</color> 데미지를 가합니다.";
    }
}
