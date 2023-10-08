using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraduateAnger : Item
{
    private int _damage = 1;
    public override void SetEquipEffectText()
    {
        _effect = $"{_damage} 데미지를 가합니다.";
    }

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.AttackEnemy();
    }
}
