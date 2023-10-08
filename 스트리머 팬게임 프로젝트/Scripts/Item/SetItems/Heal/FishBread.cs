using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 슈크림 붕어빵
/// </summary>
public class FishBread : Item
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
                _healValue = 1;
                break;
            case 1:
                _healValue = 3;
                break;
            case 2:
                _healValue = 4;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"아이템을 사용 할때 마다, <color=yellow>{_healValue}</color> 만큼 체력을 회복하는 버프를 얻습니다.";
    }
}
