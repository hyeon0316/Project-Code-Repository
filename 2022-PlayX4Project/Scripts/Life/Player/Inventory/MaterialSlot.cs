using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MaterialSlot : MonoBehaviour
{
    private Item _item;

    [SerializeField] private Image _slotImage;

    public void Add(Item item)
    {
       _item = item;
       _slotImage.sprite = _item.ItemImage;
       _slotImage.gameObject.SetActive(true);
    }

    public void Add(string itemName)
    {
        _item = Resources.Load<Item>($"Items/{itemName}"); ;
        _slotImage.sprite = _item.ItemImage;
        _slotImage.gameObject.SetActive(true);
    }

    public bool HasSceretKey()
    {
        if(_item.name == "SecretKey")
        {
            Clear();
            return true;
        }

        return false;
    }

    public void Clear()
    {
        _item = null;
        _slotImage.gameObject.SetActive(false);
    }
}
