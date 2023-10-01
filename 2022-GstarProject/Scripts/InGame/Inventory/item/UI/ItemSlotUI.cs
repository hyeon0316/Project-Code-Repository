using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [Tooltip("아이템 아이콘 이미지")]
    [SerializeField] private Image _iconImage;

    [Tooltip("아이템 개수 텍스트")]
    [SerializeField] private Text _amountText;

    [Tooltip("슬롯이 포커스될 때 나타나는 텍스트")]
    [SerializeField] private GameObject _highlightText;

    /// <summary> 슬롯의 인덱스 </summary>
    public int Index { get; private set; }

    /// <summary> 슬롯이 아이템을 보유하고 있는지 여부 </summary>
    public bool HasItem => _iconImage.sprite != null;

    /// <summary> 접근 가능한 슬롯인지 여부 </summary>
    public bool IsAccessible => _isAccessibleSlot && _isAccessibleItem;

    public RectTransform SlotRect => _slotRect;
    public RectTransform IconRect => _iconRect;


   // private InventoryUI _inventoryUI;

    private RectTransform _slotRect;
    private RectTransform _iconRect;

    private GameObject _iconGo;
    private GameObject _textGo;
    private GameObject _highlightGo;

    private Image _slotImage;
    private InventoryUI _inventoryUI;

    private bool _isAccessibleSlot = true; // 슬롯 접근가능 여부
    private bool _isAccessibleItem = true; // 아이템 접근가능 여부
    private bool _isActive = false; // 활성화 되어있는지 확인

    /// <summary> 비활성화된 슬롯의 색상 </summary>
    private static readonly Color InaccessibleSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    /// <summary> 비활성화된 아이콘 색상 </summary>
    private static readonly Color InaccessibleIconColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private void ShowIcon() => _iconGo.SetActive(true);
    private void HideIcon() => _iconGo.SetActive(false);

    public void ShowHigh() => _highlightText.SetActive(true);
    public void HideHigh() => _highlightText.SetActive(false);

    private void ShowText() => _textGo.SetActive(true);
    private void HideText() => _textGo.SetActive(false);

    public void SetSlotIndex(int index) => Index = index;

    /// <summary> 슬롯에 아이템 등록 </summary>
    public void SetItem(Sprite itemSprite)
    {
        if (itemSprite != null)
        {
            _iconImage.sprite = itemSprite;
            ShowIcon();
        }
        else
        {
            RemoveItem();
        }
    }
    private void Awake()
    {
        InitComponents();
    }
    private void InitComponents()
    {
        _inventoryUI = GetComponentInParent<InventoryUI>();

        // Rects
        _slotRect = GetComponent<RectTransform>();
        _iconRect = _iconImage.rectTransform;
        //_highlightRect = _highlightImage.rectTransform;

        // Game Objects
        _iconGo = _iconRect.gameObject;
        _textGo = _amountText.gameObject;
        //_highlightGo = _highlightImage.gameObject;

        // Images
        _slotImage = GetComponent<Image>();
        _highlightText.SetActive(_isActive);
    }
    /// <summary> 슬롯에서 아이템 제거 </summary>
    public void RemoveItem()
    {
        _iconImage.sprite = null;
        HideIcon();
        HideText();
    }


    /// <summary> 아이템 개수 텍스트 설정(amount가 1 이하일 경우 텍스트 미표시) </summary>
    public void SetItemAmount(int amount)
    {
        if (HasItem && amount > 1)
            ShowText();
        else
            HideText();

        _amountText.text = amount.ToString();
    }

}
