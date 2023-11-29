using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private UsedSlot _usedSlot;
    [SerializeField] private MaterialSlot _materialSlot;

    public void AddItem(Item item, int itemCount)
    {
        if(item.Type == ItemType.Used)
        {
            AddUsed(item, itemCount);
        }
        else
        {
            AddMaterial(item);
        }
    }

    public void AddUsed(Item item, int itemCount)
    {
        _usedSlot.Add(item, itemCount);
    }

    public void ClearUsed()
    {
        _usedSlot.Clear();
    }
    
    public void AddMaterial(string itemName)
    {
        _materialSlot.Add(itemName);
    }

    public void AddMaterial(Item item)
    {
        _materialSlot.Add(item);
    }

    public bool CheckSecretKey()
    {
        return _materialSlot.HasSceretKey();
    }

    public void ClearMaterial()
    {
        _materialSlot.Clear();
    }
}
