using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyUI : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI MaxCount;
    public TextMeshProUGUI Price;
    public TextMeshProUGUI BuyCountText;
    public ItemData Item;
    public int Gold;
    public int BuyCount=1;
    private int canPortion=999;
    private Inventory inventory;

    public void SetBuyUI(StoreSlotUI _slot)
    {
        Gold = _slot.Gold;
        Item = _slot.Item;
        if (Item is PortionItemData uItem)
        {
            canPortion = DataManager.Instance.Player.Stat.MaxPostion - _slot.HavePortion;
            if (canPortion <= 0)
                canPortion = 0;
        }
        else
        {
            canPortion = 1999;
        }
        MaxCount.text = canPortion.ToString();
        Icon.sprite = _slot.Icon.sprite;
        Name.text = _slot.Name.text;
        BuyCountText.text = BuyCount.ToString();
        int resultGold = BuyCount * Gold;
        Price.text = resultGold.ToString();
    }
    public void Button50()
    {
        if (BuyCount + 50 < canPortion)
            BuyCount += 50;
        else
            BuyCount = canPortion;
        BuyCountText.text = BuyCount.ToString();
        int resultGold = BuyCount * Gold;
        Price.text = resultGold.ToString();
    }
    public void Button100()
    {
        if (BuyCount + 100 < canPortion)
            BuyCount += 100;
        else
            BuyCount = canPortion;
        BuyCountText.text = BuyCount.ToString();
        int resultGold = BuyCount * Gold;
        Price.text = resultGold.ToString();
    }
    public void ButtonMAX()
    {
        BuyCount = canPortion;
        BuyCountText.text = BuyCount.ToString();
        int resultGold = BuyCount * Gold;
        Price.text = resultGold.ToString();
    }
    public void ButtonP()
    {
        if (BuyCount + 1 < canPortion)
            BuyCount++;
        else
            BuyCount = canPortion;
        BuyCountText.text = BuyCount.ToString();
        int resultGold = BuyCount * Gold;
        Price.text = resultGold.ToString();
    }
    public void ButtonM()
    {
        BuyCount--;
        BuyCountText.text = BuyCount.ToString();
        int resultGold = BuyCount * Gold;
        Price.text = resultGold.ToString();
    }
    public void ButtonCancel()
    {
        BuyCount = 0;
        canPortion = 999;
        Price.text = 0.ToString();
        gameObject.SetActive(false);
    }
}
