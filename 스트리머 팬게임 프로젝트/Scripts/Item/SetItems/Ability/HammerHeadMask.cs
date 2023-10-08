using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 귀상어두 가면
/// </summary>
public class HammerHeadMask : Item 
{
    private int _value;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        opponent.Weaken();
    }


    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _value = 1;
                break;
            case 1:
                _value = 2;
                break;
            case 2:
                _value = 3;
                break;
        }

    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_value}</color> 만큼 상대를 <color=red>약화</color> 합니다.";
    }
}
