using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class StoreSlotUI : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI MaxCount;
    public TextMeshProUGUI Price;
    public ItemData Item;
    public BuyUI BuyUI;
    public int HavePortion;
    public int Gold;
    
    public void Start()
    {
        Icon.sprite = Item.IconSprite;
        Name.text = Item.Name;
        Price.text = Gold.ToString();
    }
    public void SetCount(int _portionCount)
    {
        HavePortion = _portionCount;
        MaxCount.text = _portionCount + "/" + DataManager.Instance.Player.Stat.MaxPostion;
    }
    public void OnClickButton()
    {
        BuyUI.gameObject.SetActive(true);
        BuyUI.SetBuyUI(this);
    }
    
}
