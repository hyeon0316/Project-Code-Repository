using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class EnforceUI : MonoBehaviour
{
    public ItemSlotUI Slot;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI NeedText;
    public Item Item { get; set; }

    public void SetSlot(int _num , int _enforceCount)
    {
        Slot.SetItem(Item.Data.IconSprite);
        Name.text = Item.Data.Name;
        NeedText.text = _num + " / " + _enforceCount;
    }
    public void SetSlot()
    {
        Name.text = "재료부족";
        NeedText.text = "";
    }
}
