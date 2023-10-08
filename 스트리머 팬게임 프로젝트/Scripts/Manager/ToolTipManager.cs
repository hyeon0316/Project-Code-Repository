using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipManager : Singleton<ToolTipManager>
{
    [SerializeField] private GameObject _toolTipPanel;

    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _tier;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemType;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private TextMeshProUGUI _effect;

    [SerializeField] private GameObject[] _starObjects;

    [SerializeField] private GameObject _equipEffectPanel;
    [SerializeField] private TextMeshProUGUI _equipEffectText;

    [SerializeField] private GameObject _buffDebuffPanel;
    [SerializeField] private TextMeshProUGUI _buffDebuffText;

    [SerializeField] private GameObject _combinationPanel;
    [SerializeField] private GameObject _roundPanel;
    [SerializeField] private GameObject _defensePanel;
    [SerializeField] private GameObject _firstAttackPanel;

    private void Start()
    {
        _toolTipPanel.SetActive(false);
    }

    public void ShowDefensePanel()
    {
        _defensePanel.SetActive(true);
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _defensePanel.transform.position = pos;
    }
    public void ShowFirstAttackPanel()
    {
        _firstAttackPanel.SetActive(true);
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _firstAttackPanel.transform.position = pos;
    }

    public void ShowRoundPanel()
    {
        _roundPanel.SetActive(true);
    }

    public void ShowCombinationToolTip()
    {
        if (!CursorManager.Instance.IsDrag)
        {
            _combinationPanel.SetActive(true);
        }
 
    }

    /// <summary>
    /// 버프 or 디버프 툴팁
    /// </summary>
    /// <param name="content"></param>
    public void ShowBuffDeBuff(string content)
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _buffDebuffPanel.transform.position = pos;
        _buffDebuffPanel.SetActive(true);
        _buffDebuffText.text = content;
    }

    /// <summary>
    /// 장착 효과 툴팁
    /// </summary>
    public void ShowEquipEffect(string content)
    {
        _equipEffectPanel.SetActive(true);
        _equipEffectText.text = content;
    }
    
    /// <summary>
    /// 기본적인 아이템 설명 툴팁
    /// </summary>
    public void ShowToolTip(Sprite itemSprite, int tier, int upgrade, string itemName, ItemType itemType, string description, string effect)
    {
        _toolTipPanel.SetActive(true);
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _toolTipPanel.transform.position = pos;

        _itemImage.sprite = itemSprite;
        _tier.text = $"{tier} 티어"; //todo: 티어를 색깔별로 지정
        _itemName.text = itemName;
        _description.text = description;
        _effect.text = effect;

        switch (itemType)
        {
            case ItemType.Attack:
                _itemType.text = "[ 공격 ]";
                break;
            case ItemType.Defense:
                _itemType.text = "[ 방어 ]";
                break;
            case ItemType.Heal:
                _itemType.text = "[ 회복 ]";
                break;
            case ItemType.Ability:
                _itemType.text = "[ 능력 ]";
                break;
        }
        switch (tier)
        {
            case 1:
                _tier.color = Color.yellow;
                break;
            case 2:
                _tier.color = new Color(128, 0, 128); //보라색
                break;
            case 3:
                _tier.color = Color.blue;
                break;
            case 4:
                _tier.color = Color.green;
                break;
            case 5:
                _tier.color = Color.white;
                break;
        }

        switch (upgrade)
        {
            case 0:
                SetActiveStarObject(1);
                break;
            case 1:
                SetActiveStarObject(2);
                break;
            case 2:
                SetActiveStarObject(3);
                break;
        }
    }

    /// <summary>
    /// 해당 개수만큼 별을 활성화
    /// </summary>
    private void SetActiveStarObject(int activeCount)
    {
        int count = 0;
        foreach (var star in _starObjects)
        {
            if (count < activeCount)
            {
                star.SetActive(true);
                count++;
            }
            else
            {
                star.SetActive(false);
            }
        }
    }

    public void CloseToolTip()
    {
        if (_toolTipPanel.activeSelf)
        {
            _toolTipPanel.SetActive(false);
            _equipEffectPanel.SetActive(false);
        }
        _combinationPanel.SetActive(false);
        _roundPanel.SetActive(false);
        _defensePanel.SetActive(false);
        _firstAttackPanel.SetActive(false);
    }

    public void CloseRound()
    {
        _roundPanel.SetActive(false);
    }

    public void CloseBuffDeBuff()
    {
        _buffDebuffPanel.SetActive(false);
    }

    public void CloseCombination()
    {
        _combinationPanel.SetActive(false);
    }

    public void CloseDefense()
    {
        _defensePanel.SetActive(false);
    }

    public void CloseFirstAttack()
    {
        _firstAttackPanel.SetActive(false);
    }
}
