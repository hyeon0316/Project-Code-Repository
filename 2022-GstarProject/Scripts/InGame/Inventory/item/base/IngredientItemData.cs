using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary> 재료 - 재료 아이템 </summary>
[CreateAssetMenu(fileName = "Item_Ingredient_", menuName = "Inventory System/Item Data/Ingredient", order = 4)]
public class IngredientItemData : CountableItemData
{
    public override Item CreateItem()
    {
        return new IngredientItem(this);
    }
}
