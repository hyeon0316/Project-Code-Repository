using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 메이드복
/// </summary>
public class MaidCostume : Item
{
    private int _fixedDefense = 4;

    private int _counterAttack;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
       player.AddDefense();
    }


    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _counterAttack = 5;
                break;
            case 1:
                _counterAttack = 7;
                break;
            case 2:
                _counterAttack = 10;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"{_fixedDefense} <color=#4aa8d8>방어도</color>를 얻습니다.\n상대방이 [공격] 아이템 발동 시, " +
                  $"<color=yellow>{_counterAttack}</color> 만큼 데미지를 주는 버프를 얻습니다.";
    }
}
