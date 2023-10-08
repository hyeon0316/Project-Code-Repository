using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class No : Item
{
    private int _defense = 8;
    public override void SetEquipEffectText()
    {
        _effect = $"{_defense} 방어도를 획득합니다.";
    }

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.PlayAnim(AnimType.UseNo);
        player.AddDefense();
    }
}
