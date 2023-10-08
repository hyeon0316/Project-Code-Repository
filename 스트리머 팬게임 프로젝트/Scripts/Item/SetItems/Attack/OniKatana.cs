using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오니의 카타나
/// </summary>
public class OniKatana : Item
{
    private int _damage;

    private int _fixedDamage = 6;
    

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.AttackEnemy();
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _damage = 2;
                break;
            case 1:
                _damage = 3;
                break;
            case 2:
                _damage = 5;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_fixedDamage}</color> 데미지를 가합니다.\n{_damage}만큼 상대를 <color=red>출혈</color> 시킵니다.";
    }
}
