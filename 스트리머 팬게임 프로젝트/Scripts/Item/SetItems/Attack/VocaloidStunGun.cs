using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보컬로이드의 전기충격기
/// </summary>
public class VocaloidStunGun : Item
{
    private int _damage;
    private int _fixedWeaken = 1;

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
        _effect = $"<color=yellow>{_damage}</color> 데미지를 가합니다.\n{_fixedWeaken} 만큼 상대를 <color=red>약화</color>합니다.";
    }
}
