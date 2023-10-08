using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 심리상담가의 자격증
/// </summary>
public class CounselLicense : Item
{
    private int _percent;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.CheckGetTicket(IsFlag);
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _percent = 30;
                break;
            case 1:
                _percent = 50;
                break;
            case 2:
                _percent = 100;
                break;
        }
    }
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_percent}</color>% 확률로 일반 뽑기권을 생성합니다.";
    }
}
