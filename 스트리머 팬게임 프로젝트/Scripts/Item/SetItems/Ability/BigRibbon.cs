using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 매우 큰 리본
/// </summary>
public class BigRibbon : Item
{

    /// <summary>
    /// 추가 방어력, 방어도 상승 효과에 같이 적용
    /// </summary>
    private int _plusDefense;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.GetDefensePower();
    }


    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _plusDefense = 2;
                break;
            case 1:
                _plusDefense = 4;
                break;
            case 2:
                _plusDefense = 6;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_plusDefense}</color> 만큼 추가 방어력이 증가 합니다. \n" +
                  $"방어도를 획득할 시 추가 방어력 만큼 추가 방어도를 획득합니다.";
    }
}
