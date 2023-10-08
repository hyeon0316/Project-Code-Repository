using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnemy : BattleCharacter
{
    /// <summary>
    /// 크립 몬스터 셋팅
    /// </summary>
    public void SetCreepEnemy(ECreepType type, int opponentID)
    {
        _isCreep = true;
        GameObject obj = Instantiate(DataManager.Instance.EnemyPrefabs[(int)type], _characterPos);
        _characterController = obj.GetComponent<CharacterController>();
        NetworkID = opponentID + Global.CreepIDNumber;

        //아이템 셋팅
        switch (type)
        {
            case ECreepType.Shrimp:
                SetCreepItem(EItemCode.GraduateAnger);
                break;
            case ECreepType.NegativeMan:
                SetCreepItem(EItemCode.ArmyAnger);
                break;
            case ECreepType.Hodd:
                SetCreepItem(EItemCode.PoorAnger);
                break;
            case ECreepType.Wakpago:
                SetCreepItem(EItemCode.Charge);
                break;
            case ECreepType.ShortAnswer:
                ItemSlots[0].AddNewItem(EItemCode.Yes);
                ItemSlots[1].AddNewItem(EItemCode.Yes);
                ItemSlots[2].AddNewItem(EItemCode.Yes);
                ItemSlots[3].AddNewItem(EItemCode.No);
                ItemSlots[4].AddNewItem(EItemCode.No);
                ItemSlots[5].AddNewItem(EItemCode.No);
                break;
            case ECreepType.ChunSik:
                SetCreepItem(EItemCode.PressMachine);
                break;
            case ECreepType.KwonMin:
                SetCreepItem(EItemCode.KwonMinSummons);
                break;
        }
    }

    private void SetCreepItem(EItemCode code)
    {
        foreach (var slot in ItemSlots)
        {
            slot.AddNewItem(code);
        }
    }

    /// <summary>
    /// 오른쪽에 배치되어 이미지가 반전되어야 할때(크립 몬스터 제외)
    /// </summary>
    public void ReverseImage()
    {
        _characterController.ReverseImage();
    }
    
}
