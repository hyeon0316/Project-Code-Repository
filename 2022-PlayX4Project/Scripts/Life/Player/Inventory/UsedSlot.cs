using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UsedSlot : MonoBehaviour
{
    public Item Item;
    public Image SlotImage;
    public Image FillImage;
    public int ItemCount;
    private Text _countText;
    private Text _coolTimeText;
    private bool _isCoolDown;
    private float _time;
    private float _maxTime;
    
    private void Awake()
    {
        _countText = SlotImage.GetComponentInChildren<Text>();
        _coolTimeText = FillImage.GetComponentInChildren<Text>();
    }
    
    private void Update()
    {
        _countText.text = $"{ItemCount}";

        if (FindObjectOfType<Player>().HP < FindObjectOfType<Player>()._Maxhp)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && ItemCount != 0 && !_isCoolDown)
            {
                FindObjectOfType<SoundManager>().Play("Player/Potion",SoundType.Effect);
                FillImage.gameObject.SetActive(true);
                _isCoolDown = true;
                _time = Item.ItemCoolDown;
                _maxTime = _time;
                --ItemCount;
                FindObjectOfType<Player>().HP += Item.HillValue;
                if (ItemCount == 0)
                {
                    FillImage.gameObject.SetActive(false);
                    FindObjectOfType<Inventory>().ClearUsed();
                }
            }
        }

        if (_isCoolDown)
        {
            _time -= Time.deltaTime;
            _coolTimeText.text =$"{_time:N1}";
            FillImage.fillAmount = _time / _maxTime;
            if (_time <= 0)
            {
                FillImage.gameObject.SetActive(false);
                _time = 0;
                _isCoolDown = false;
            }
        }
    }
}
