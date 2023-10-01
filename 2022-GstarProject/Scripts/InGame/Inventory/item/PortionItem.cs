using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> ���� ������ - ���� ������ </summary>
public class PortionItem : CountableItem, IUsableItem
{
    public PortionItemData PortionItemData;
    public PortionItem(PortionItemData data, int amount = 1) : base(data, amount) 
    {
        PortionItemData = data;
    }

    public bool Use()
    {
        DataManager.Instance.Player.Heal(PortionItemData.Value);
        Amount--;

        return true;
    }

    protected override CountableItem Clone(int amount)
    {
        return new PortionItem(CountableData as PortionItemData, amount);
    }
}