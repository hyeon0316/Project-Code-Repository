using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 잼민이의 돈까스
/// </summary>
public class PorkCutlet : Item
{
    private int _healValue;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.HealAvatar();
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _healValue = 3;
                break;
            case 1:
                _healValue = 5;
                break;
            case 2:
                _healValue = 7;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_healValue}</color> 만큼 체력을 회복합니다.";
    }
}
