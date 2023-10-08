using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 마법사의 스태프
/// </summary>
public class MagicianStaff : Item 
{
    private int _fixedDamage = 10;
    private int _healWeakenValue;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        opponent.WeakenHeal(_healWeakenValue);
        player.AttackEnemy();
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _healWeakenValue = 2;
                break;
            case 1:
                _healWeakenValue = 4;
                break;
            case 2:
                _healWeakenValue = 6;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"{_fixedDamage} 데미지를 가합니다.\n<color=yellow>{_healWeakenValue}</color> 만큼 상대에게 <color=red>치유력 감소</color> 를 부여합니다.";
    }
}
