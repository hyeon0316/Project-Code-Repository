using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 좋은거
/// </summary>
public class GoodThings : Item
{
    private int _value;

    /// <summary>
    /// 곱해질 배수
    /// </summary>
    private int _times = 3;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.TakeAvatarDamage();
        player.AddDefense();
    }


    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _value = 3;
                break;
            case 1:
                _value = 4;
                break;
            case 2:
                _value = 5;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_value}</color>만큼 <color=red>방어도 관통</color> 데미지를 받습니다." +
                  $"\n{_times} X <color=yellow>{_value}</color> 만큼 방어도를 얻습니다.";
    }
}
