using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 부정적인 드롭스
/// </summary>
public class Drops : Item
{
    private int _value;

    private int _fixedValue = 2;
    
    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _value = 0;
                break;
            case 1:
                _value = 1;
                break;
            case 2:
                _value = 2;
                break;
        }

    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_value}</color> + {_fixedValue} 만큼 상대를 <color=red>약화</color>하고, <color=yellow>{_value}</color> 만큼 자신을 <color=red>약화</color>합니다.";
    }
}
