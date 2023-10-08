using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 바텐더의 나비넥타이
/// </summary>
public class BartenderNecktie : Item
{
    private int _plusDefense;
    
    private int _upgradeValue1 = 5;
    private int _upgradeValue2 = 10;
    private int _upgradeValue3 = 15;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.AddDefense();
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _plusDefense = 3;
                break;
            case 1:
                _plusDefense = 5;
                break;
            case 2:
                _plusDefense = 10;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_plusDefense}</color> <color=#4aa8d8>방어도</color>를 얻습니다.\n" +
                  $"현재 방어도는 2배가 됩니다.";
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
