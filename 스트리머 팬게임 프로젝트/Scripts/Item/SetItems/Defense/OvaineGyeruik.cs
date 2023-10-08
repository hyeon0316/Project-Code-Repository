using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 오베인 게르크 티
/// </summary>
public class OvaineGyeruik : Item
{
    private int _defense;
    
    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.AddDefense();
    }


    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _defense = 3;
                break;
            case 1:
                _defense = 5;
                break;
            case 2:
                _defense = 7;
                break;
        }

    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_defense}</color> <color=#4aa8d8>방어도</color>를 얻습니다.";
    }
}
