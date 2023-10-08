using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대학원생의 USB
/// </summary>
public class USB : Item
{
    private int _upgradeValue1 = 1;
    private int _upgradeValue2 = 2;
    private int _upgradeValue3 = 3;
    
    public override void SetEquipEffectText()
    {
        _effect = "상대방의 <color=#4aa8d8>공격력 상승</color> 효과와 자신의 <color=red>약화</color> 효과를 제거합니다.";
    }

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        //todo: 이펙트
    }

    public override void SetEquipEffect(int playerID)
    {
        switch (Upgrade)
        {
            case 0:
                PlayerManager.Instance.GetPlayer(playerID).UpdateFirstAttack(_upgradeValue1);
                break;
            case 1:
                PlayerManager.Instance.GetPlayer(playerID).UpdateFirstAttack(_upgradeValue2);
                break;
            case 2:
                PlayerManager.Instance.GetPlayer(playerID).UpdateFirstAttack(_upgradeValue3);
                break;
        }
    }
    
    protected override void ShowEquipEffectPanel()
    {
        switch (Upgrade)
        {
            case 0:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 선공 + <color=yellow>{_upgradeValue1}</color>");
                break;
            case 1:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 선공 + <color=yellow>{_upgradeValue2}</color>");
                break;
            case 2:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 선공 + <color=yellow>{_upgradeValue3}</color>");
                break;
        }
    }
}
