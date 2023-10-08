using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 500원 짜리 고철
/// </summary>
public class MetalScrap : Item
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
                _defense = 5;
                break;
            case 1:
                _defense = 9;
                break;
            case 2:
                _defense = 13;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_defense}</color> <color=#4aa8d8>방어도</color>를 얻습니다.";
    }
}
