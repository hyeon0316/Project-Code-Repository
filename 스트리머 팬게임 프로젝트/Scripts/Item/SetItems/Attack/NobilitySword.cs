using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 귀족의 상현딸
/// </summary>
public class NobilitySword : Item
{
    private int _damage;
    private int _fixedDamage = 2;
    

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
                _damage = 6;
                break;
            case 2:
                _damage = 11;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_damage}</color> 데미지를 가합니다.\n{_fixedDamage}만큼 상대를 <color=red>출혈</color> 시킵니다.";
    }
}
