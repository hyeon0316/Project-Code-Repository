using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Yes : Item
{
    private int _damage = 10;
    public override void SetEquipEffectText()
    {
        _effect = $"{_damage} 데미지를 가합니다.";
    }

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.PlayAnim(AnimType.UseYes);
        StartCoroutine(AttackCo(opponent));
    }

    private IEnumerator AttackCo(BattleCharacter opponent)
    {
        yield return new WaitForSeconds(0.5f);
        opponent.TakeAvatarDamage();
    }
}
