using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 노인의 틀니
/// </summary>
public class Denture : Item
{
    private int _attackBuff;

    private int _fixedBuff = 3;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _attackBuff = 1;
                break;
            case 1:
                _attackBuff = 2;
                break;
            case 2:
                _attackBuff = 3;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"발동 순서에 따라서 <color=#4aa8d8>공격력을 강화</color>합니다.\n" +
                  $"1, 2, 3번째 {_fixedBuff + _attackBuff - 1} / 4, 5, 6번째 {_fixedBuff + _attackBuff + 1}";
    }
}
