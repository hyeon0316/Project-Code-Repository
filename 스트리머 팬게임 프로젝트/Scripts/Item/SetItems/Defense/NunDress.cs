using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 수녀복
/// </summary>
public class NunDress : Item
{
    private int _healValue;

    private int _fixedDefenseValue = 4;
    

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.AddDefense();
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _healValue = 6;
                break;
            case 1:
                _healValue = 8;
                break;
            case 2:
                _healValue = 11;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"{_fixedDefenseValue} <color=#4aa8d8>방어도</color>를 얻습니다.\n상대방이 [공격] 아이템 발동 시, <color=yellow>{_healValue}</color> 만큼 자신의 체력을 회복하는 버프를 얻습니다.";
    }
}
