using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary> �Һ� ������ ���� </summary>
[CreateAssetMenu(fileName = "Item_Portion_", menuName = "Inventory System/Item Data/Portion", order = 3)]
public class PortionItemData : CountableItemData
{
    /// <summary> ȿ����(ȸ���� ��) </summary>
    public int Value => _value;
    [SerializeField] private int _value;
    public override Item CreateItem()
    {
        return new PortionItem(this);
    }
}