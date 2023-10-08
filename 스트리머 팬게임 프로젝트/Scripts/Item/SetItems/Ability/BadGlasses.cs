using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 악질안경
/// </summary>
public class BadGlasses : Item
{
    private int _plusDamage;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
    }


    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _plusDamage = 2;
                break;
            case 1:
                _plusDamage = 3;
                break;
            case 2:
                _plusDamage = 5;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_plusDamage}</color> 만큼 <color=#4aa8d8>공격력을 강화</color> 합니다.";
    }
}
