using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RouletteType
{
    Normal,
    Advanced,
    Top,
    Supreme
}

public class RouletteController : MonoBehaviour
{
    /// <summary>
    /// 룰렛 버튼을 눌렀을 때 활성화 될 종류별 룰렛 모음 패널
    /// </summary>
    [SerializeField] private GameObject _roulettesPanel;

    [SerializeField] private Roulette[] _roulettes;

    public TextMeshProUGUI PercentNormalText;
    
    /// <summary>
    /// 종류 별 뽑기권의 수 전체 합
    /// </summary>
    private int _rouletteCountTotal;

    public int RouletteCountTotal => _rouletteCountTotal;
    
    [SerializeField] private TextMeshProUGUI _rouletteCountTotalText;

    [SerializeField] private Image _buttonImage;
    [SerializeField] private Sprite _closeSprite;
    [SerializeField] private Sprite _openSprite;
    
    private void Start()
    {
        CheckUseRoulette();
    }
    
    /// <summary>
    /// 뽑기권 모음 패널 활성화
    /// </summary>
    public void SetActivePanel()
    {
        _roulettesPanel.SetActive(!_roulettesPanel.activeSelf);
        _buttonImage.sprite = _roulettesPanel.activeSelf ? _openSprite : _closeSprite;
    }

    public void ResetPanel()
    {
        if (_roulettesPanel.activeSelf)
            SetActivePanel();
    }

    /// <summary>
    /// UnUsing과 Using 슬롯에 모두 아이템이 존재 하는지 체크하여 뽑기 버튼 활성화 여부 판단
    /// </summary>
    public void CheckUseRoulette()
    {
        int fullCheck = 0;
        foreach (var slot in PlayerManager.Instance.Players[0].UnUsingInventory.ItemSlots)
        {
            if (slot.IsExistItem())
                ++fullCheck;
        }

        foreach (var button in _roulettes)
        {
            button.InteractableButton(fullCheck != Global.MaxUnUsingItemCount);
        }

    }
    
    /// <summary>
    /// 뽑기권 수를 지정 받음
    /// </summary>
    public void SetRouletteCount(RouletteType type, int count)
    {
        _roulettes[(int)type].SetRouletteCount(count);
        UpdateTotalCount();
    }

    /// <summary>
    /// 뽑기권 총합 개수 갱신
    /// </summary>
    public void UpdateTotalCount()
    {
        _rouletteCountTotal = 0;
        foreach (var roulette in _roulettes)
        {
            _rouletteCountTotal += roulette.RouletteCount;
        }
        _rouletteCountTotalText.text = $"{_rouletteCountTotal}";
    }

    
}
