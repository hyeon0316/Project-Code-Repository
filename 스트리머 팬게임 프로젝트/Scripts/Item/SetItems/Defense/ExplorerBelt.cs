using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 탐험가의 벨트
/// </summary>
public class ExplorerBelt : Item
{
    private int _fixedDefense = 4;
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
                _plusDefense = 7;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"발동 순서에 따라서 <color=#4aa8d8>방어도</color>를 얻습니다.\n" +
                  $"1, 2, 3번째 <color=yellow>{_fixedDefense}</color> / 4, 5, 6번째 <color=yellow>{_fixedDefense + _plusDefense}</color>";
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
