using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientItem : CountableItem
{
 
    public IngredientItem(IngredientItemData data, int amount = 1) : base(data, amount) { }

    
    protected override CountableItem Clone(int amount)
    {
        return new IngredientItem(CountableData as IngredientItemData, amount);
    }
    
}
