using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가지치기용 도끼
/// </summary>
public class PruningAx : Item
{
    private int _damage;

    /// <summary>
    /// 기본 전용 데미지
    /// </summary>
    private int _fixedDamage = 5;
    

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
        _effect = $"{_fixedDamage} + 자신의 <color=#4aa8d8>방어도</color> <color=yellow>{_damage}</color>배의 데미지를 가합니다.";
    }
}
