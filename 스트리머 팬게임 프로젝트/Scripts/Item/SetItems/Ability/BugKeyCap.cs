using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 벌레의 키캡
/// </summary>
public class BugKeyCap : Item
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
                _damage = 3;
                break;
            case 1:
                _damage = 4;
                break;
            case 2:
                _damage = 6;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"이 아이템을 사용한 횟수 X <color=yellow>{_damage}</color>데미지를 가합니다.";
    }
    
}
