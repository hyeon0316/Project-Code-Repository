using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 왁엔터 사장 명패
/// </summary>
public class WakEnterNameplate : Item
{
    private int _damage;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.AttackEnemy();
        if (player.IsView)
        {
            SoundManager.Instance.PlayEffect(EffectType.WakEnter);
        }
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
        _effect = $"<color=yellow>{_damage}</color> 데미지를 2번 가합니다.";
    }
    
}
