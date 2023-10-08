using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 다이아 검
/// </summary>
public class DiaSword : Item
{
    private int _damage;

    private int _upgradeValue1 = 2;
    private int _upgradeValue2 = 3;
    private int _upgradeValue3 = 4;

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        player.AttackEnemy();
        if (player.IsView)
        {
            SoundManager.Instance.PlayEffect(EffectType.DiaSword);
        }
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _damage = 10;
                break;
            case 1:
                _damage = 20;
                break;
            case 2:
                _damage = 30;
                break;
        }
    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"<color=yellow>{_damage}</color> 데미지를 아이템 사용 횟수 만큼 가합니다.";
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
