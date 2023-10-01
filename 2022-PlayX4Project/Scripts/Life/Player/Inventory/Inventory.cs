using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    private UsedSlot _usedSlot;
    private MaterialSlot _materialSlot;

    public MaterialSlot MaterialSlot
    {
        get
        {
            return _materialSlot;
        }
        set
        {
            _materialSlot = value;
        }
    }
    
    private void Awake()
    {
        _usedSlot = FindObjectOfType<UsedSlot>();
        _materialSlot = FindObjectOfType<MaterialSlot>();
    }

    /// <summary>
    /// 소모 아이템을 슬롯에 추가 해 주는 기능(이미 같은 아이템을 추가 할 경우 개수만 증가)
    /// </summary>
    /// <param name="itemName">추가 할 아이템 이름</param>
    /// <param name="itemCount">추가 할 개수</param>
    public void AddUsed(Item item, int itemCount)
    {
        if (_usedSlot.Item != null && _usedSlot.Item == item)
        {
            _usedSlot.ItemCount += itemCount;
        }
        else
        {
            _usedSlot.Item = item;
            _usedSlot.SlotImage.sprite = item.ItemImage;
            _usedSlot.ItemCount = itemCount;
            _usedSlot.SlotImage.gameObject.SetActive(true);
        }
    }

    public void ClearUsed()
    {
        _usedSlot.ItemCount = 0;
        _usedSlot.Item = null;
        _usedSlot.SlotImage.gameObject.SetActive(false);
    }
    
    public void AddMaterial(string itemName)
    {
        _materialSlot.Item = Resources.Load<Item>($"Items/{itemName}");
        _materialSlot.SlotImage.sprite = _materialSlot.Item.ItemImage;
        _materialSlot.SlotImage.gameObject.SetActive(true); 
    }

    public void DeleteMaterial()
    {
        _materialSlot.SlotImage.gameObject.SetActive(false); 
    }


}
