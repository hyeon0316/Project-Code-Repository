using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 선량한 시민의 빠루
/// </summary>
public class CrowBar : Item
{
    private int _damage;
    
    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
            
        player.AttackEnemy();
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _damage = 7;
                break;
            case 1:
                _damage = 11;
                break;
            case 2:
                _damage = 15;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"상대방의 잃은 체력 20% + <color=yellow>{_damage}</color> 데미지를 가합니다.";
    }
}
