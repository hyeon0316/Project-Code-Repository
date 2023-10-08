using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Inventory : MonoBehaviour
{
    public ItemSlot[] ItemSlots;
    
    public void InitItem()
    {
        foreach (var item in ItemSlots)
        {
            item.DeleteItem();
        }
    }
    
    /// <summary>
    /// 아이템을 슬롯 순서대로 추가
    /// </summary>
    public void AddItem(EItemCode itemCode) 
    {
        foreach (var slot in ItemSlots)
        {
            if (slot.transform.childCount == 1)
                continue;

            slot.AddNewItem(itemCode);
            break;
        }
    }

    public int GetEmptySlot()
    {
        int index = 255;
        foreach (var slot in ItemSlots)
        {
            if (!slot.IsExistItem())
            {
                index = slot.GetSlotIndex();
                break;
            }
        }

        return index;
    }

}
