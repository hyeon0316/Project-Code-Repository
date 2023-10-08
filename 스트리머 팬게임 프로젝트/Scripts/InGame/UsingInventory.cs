using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsingInventory : Inventory
{

    /// <summary>
    /// 아이템이 중복으로 셋팅되어 있는지 체크
    /// </summary>
    public bool CheckDuplicationItem(EItemCode code)
    {
        foreach (var slot in ItemSlots)
        {
            if (slot.transform.childCount >= 1)
            {
                if (code == slot.OtherInSlotItem().Code)
                    return true;
            }
        }
        return false;
    }
    
    public bool CheckDuplicationItem(byte slotIndex)
    {
        for (int i = 0; i < ItemSlots.Length; i++)
        {
            if (i == slotIndex)
            {
                if (ItemSlots[i].IsExistItem())
                {
                    return true;
                }
            }
        }
        return false;
    }
}
