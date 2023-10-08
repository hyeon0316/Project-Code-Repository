using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 비밀스런 마법봉
/// </summary>
public class SecretWand : Item
{
    private int _damage;
    private int _heal;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
       if (IsFlag) //공격
       {
           player.AttackEnemy();
       }
       else //힐
       {
           player.HealAvatar();
       }
    }
    
    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _damage = 6;
                _heal = 7;
                break;
            case 1:
                _damage = 9;
                _heal = 10;
                break;
            case 2:
                _damage = 13;
                _heal = 14;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"각각 50% 확률로 <color=yellow>{_damage}</color> 데미지를 가하거나 <color=yellow>{_heal}</color> 만큼 회복합니다.";
    }
}
