using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 여고생의 헤어롤
/// </summary>
public class HairRoll : Item
{
    private float _damage;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.AttackEnemy();
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _damage = 0.5f;
                break;
            case 1:
                _damage = 1;
                break;
            case 2:
                _damage = 2;
                break;
        }

    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"뽑기권의 개수 X <color=yellow>{_damage}</color> 데미지를 가합니다.";
    }
}
