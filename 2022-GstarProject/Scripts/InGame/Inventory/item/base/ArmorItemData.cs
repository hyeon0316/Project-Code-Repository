using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 장비 - 방어구 아이템 </summary>
    [CreateAssetMenu(fileName = "Item_Armor_", menuName = "Inventory System/Item Data/Armor", order = 2)]
    public class ArmorItemData : EquipmentItemData
    {
        public override Item CreateItem()
        {
            return new ArmorItem(this);
        }
    }