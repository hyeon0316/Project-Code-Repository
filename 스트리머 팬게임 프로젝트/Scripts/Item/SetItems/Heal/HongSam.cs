using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 홍삼스틱
/// </summary>
public class HongSam : Item
{
    private int _upgradeValue1 = 5;
    private int _upgradeValue2 = 10;
    private int _upgradeValue3 = 15;
    

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.RemoveDeBuff();
    }

    public override void SetEquipEffect(int playerID)
    {
        switch (Upgrade)
        {
            case 0:
                PlayerManager.Instance.GetPlayer(playerID).UpdateAvtarHp(_upgradeValue1);
                break;
            case 1:
                PlayerManager.Instance.GetPlayer(playerID).UpdateAvtarHp(_upgradeValue2);
                break;
            case 2:
                PlayerManager.Instance.GetPlayer(playerID).UpdateAvtarHp(_upgradeValue3);
                break;
        }
    }

    public override void SetEquipEffectText()
    {
        _effect = $"<color=red>디버프</color> 효과를 제거합니다.";
    }

    protected override void ShowEquipEffectPanel()
    {
        switch (Upgrade)
        {
            case 0:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 최대체력 + <color=yellow>{_upgradeValue1}</color>");
                break;
            case 1:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 최대체력 + <color=yellow>{_upgradeValue2}</color>");
                break;
            case 2:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 최대체력 + <color=yellow>{_upgradeValue3}</color>");
                break;
        }
    }
   
}
