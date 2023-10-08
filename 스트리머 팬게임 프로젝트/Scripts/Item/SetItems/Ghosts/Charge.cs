using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charge : Item
{
    private int _damage = 2;
    private int _heal = 3;
    public override void SetEquipEffectText()
    {
        _effect = $"{_damage} 데미지를 가하고, {_heal} 체력을 회복합니다.";
    }

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.AttackEnemy();
        player.HealAvatar();
    }
}
