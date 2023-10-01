using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreUI : MonoBehaviour
{
    public Inventory inventory;
    public BuyUI BuyUI;
    [SerializeField]
    private StoreSlotUI[] _slots;
    // Start is called before the first frame update
    void Start()
    {
        
        Init();
    }
    private void OnEnable()
    {
        Init();
    }
    public void Init()
    {
        BuyUI.gameObject.SetActive(false);
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].SetCount(inventory.HavePostion());
        }
    }

    public void ButtonBuy()
    {
        int result = BuyUI.BuyCount * BuyUI.Gold;
        if (DataManager.Instance.Gold > result)
        {
            DataManager.Instance.SetGold(-result);
            inventory.Add(BuyUI.Item, BuyUI.BuyCount);
            SoundManager.Instance.EffectPlay(EffectSoundType.money);
            BuyUI.ButtonCancel();
            Init();

        }
    }
    public void ButtonExit()
    {
        gameObject.SetActive(false);
    }
}
