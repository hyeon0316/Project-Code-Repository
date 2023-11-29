using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UsedSlot : MonoBehaviour
{

    [SerializeField] private Image _slotImage;
    [SerializeField] private Text _countText;
    [SerializeField] private Text _coolTimeText;
    [SerializeField] private Image _fillImage;

    private Item _item;
    private int _itemCount;
    private bool _isCoolDown;
    private float _time;
    private float _maxTime;

    private void Update()
    {
        _countText.text = $"{_itemCount}";

        if (PlayerManager.Instance.Player.Hp <= PlayerManager.Instance.Player.MaxHp)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && _itemCount != 0 && !_isCoolDown)
            {
                SoundManager.Instance.Play("Player/Potion", SoundType.Effect);
                _fillImage.gameObject.SetActive(true);
                _isCoolDown = true;
                _time = _item.ItemCoolDown;
                _maxTime = _time;
                --_itemCount;
                PlayerManager.Instance.Player.Heal(_item.HillValue);
                if (_itemCount == 0)
                {
                    _fillImage.gameObject.SetActive(false);
                    Clear();
                }
            }
        }

        if (_isCoolDown)
        {
            _time -= Time.deltaTime;
            _coolTimeText.text = $"{_time:N1}";
            _fillImage.fillAmount = _time / _maxTime;
            if (_time <= 0)
            {
                _fillImage.gameObject.SetActive(false);
                _time = 0;
                _isCoolDown = false;
            }
        }
    }

    public void Add(Item item, int count)
    {
        if (_item != null && _item.Id == item.Id)
        {
            _itemCount += count;
        }
        else
        {
            _item = item;
            _slotImage.sprite = item.ItemImage;
            _itemCount = count;
            _slotImage.gameObject.SetActive(true);
        }
    }

    public void Clear()
    {
        _itemCount = 0;
        _item = null;
        _slotImage.gameObject.SetActive(false);
    }
}
