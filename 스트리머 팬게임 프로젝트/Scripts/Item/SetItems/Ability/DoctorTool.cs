using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 박사의 만능툴
/// </summary>
public class DoctorTool : Item
{
    private int _upgradeValue1 = 5;
    private int _upgradeValue2 = 10;
    private int _upgradeValue3 = 15;

    private int _fixedDamage = 5;
    
    public override void SetEquipEffectText()
    {
        _effect = $"상대의 아이템 중 하나로 <color=#4aa8d8>변화</color>합니다.\n" +
                  $"크립라운드 혹은 복사할 아이템이 없는 경우 {_fixedDamage} 만큼 데미지를 가합니다.";
    }

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.AttackEnemy(); //변신 실패 했을 경우
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
