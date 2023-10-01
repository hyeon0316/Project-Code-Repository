using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UsePortion : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI count;
    [SerializeField] private Inventory inventory;
    
    [Header("포션 쿨타임")] [SerializeField] protected CoolDown _potionCoolDown;

    public CoolDown PotionCoolDown => _potionCoolDown;
   

    public void SetPortion(int i)
    {
        count.text = i.ToString();
    }

    public void UsePotion()
    {
        if (!_potionCoolDown.IsCoolDown)
        {
            if (count.text != "0")
            {
                inventory.UsePortion();
                _potionCoolDown.SetCoolDown();
            }
        }
    }
}
