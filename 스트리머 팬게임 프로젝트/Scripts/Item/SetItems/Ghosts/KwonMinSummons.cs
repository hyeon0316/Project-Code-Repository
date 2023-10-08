using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KwonMinSummons : Item
{
    private int _damage = 1;
    
    
    public override void SetEquipEffectText()
    {
        _effect = $"{_damage} 데미지를 가합니다.\n" +
                  $"해당 아이템 사용시 다음 데미지는 2배가 됩니다.\n" +
                  $"사이클이 지나면 데미지는 초기화 됩니다.";
    }

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.AttackEnemy();
    }
}
