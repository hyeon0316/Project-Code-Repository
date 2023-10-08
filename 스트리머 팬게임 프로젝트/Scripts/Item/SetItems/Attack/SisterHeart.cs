using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 언니의 마음
/// </summary>
public class SisterHeart : Item
{
    private int _damage;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
       opponent.PlantBomb();
    }


    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _damage = 15;
                break;
            case 1:
                _damage = 20;
                break;
            case 2:
                _damage = 30;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=#4aa8d8>사이클</color>이 끝날 시, <color=yellow>{_damage}</color>데미지를 주는 디버프를 상대방에게 부여합니다.";
    }
}
