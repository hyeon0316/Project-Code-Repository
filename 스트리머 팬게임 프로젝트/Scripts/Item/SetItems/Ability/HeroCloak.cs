using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 히어로의 망토
/// </summary>
public class HeroCloak : Item
{
    private int _upgradeValue1 = 3;
    private int _upgradeValue2 = 6;
    private int _upgradeValue3 = 9;
    
    
    public override void SetEquipEffectText()
    {
        _effect = "<color=red>디버프</color>를 1회 무시합니다.\n" +
                  "<color=#4aa8d8>사이클</color>이 지나면 해당 효과는 사라집니다.";
    }

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
    }

    public override void SetEquipEffect(int playerID)
    {
        switch (Upgrade)
        {
            case 0:
                PlayerManager.Instance.GetPlayer(playerID).UpdateDefense(_upgradeValue1);
                break;
            case 1:
                PlayerManager.Instance.GetPlayer(playerID).UpdateDefense(_upgradeValue2);
                break;
            case 2:
                PlayerManager.Instance.GetPlayer(playerID).UpdateDefense(_upgradeValue3);
                break;
        }
    }

    protected override void ShowEquipEffectPanel()
    {
        switch (Upgrade)
        {
            case 0:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 방어도 + <color=yellow>{_upgradeValue1}</color>");
                break;
            case 1:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 방어도 + <color=yellow>{_upgradeValue2}</color>");
                break;
            case 2:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 방어도 + <color=yellow>{_upgradeValue3}</color>");
                break;
        }
    }
}
