using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 철학도의 칫솔
/// </summary>
public class ToothBrush : Item
{
    private int _healPercent;
    
    private int _upgradeValue1 = 7;
    private int _upgradeValue2 = 12;
    private int _upgradeValue3 = 20;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.HealAvatar();
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _healPercent = 25;
                break;
            case 1:
                _healPercent = 40;
                break;
            case 2:
                _healPercent = 50;
                break;
        }
    }

    public override void SetEquipEffectText()
    {
        _effect = $"잃은 체력의<color=yellow>{_healPercent}</color>% 만큼 체력을 회복합니다.";
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
