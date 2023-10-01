using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class EquipSlotUI : MonoBehaviour
{
    [Tooltip("아이템 아이콘 이미지")]
    [SerializeField] private Image _iconImage;

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

    private Image _slotImage;
    private InventoryUI _inventoryUI;
    // 현재 하이라이트 알파값
    private float _currentHLAlpha = 0f;

    private bool _isAccessibleSlot = true; // 슬롯 접근가능 여부
    private bool _isAccessibleItem = true; // 아이템 접근가능 여부
    private bool _isActive = false; // 활성화 되어있는지 확인

    private void ShowIcon() => _iconGo.SetActive(true);
    private void HideIcon() => _iconGo.SetActive(false);

    public void ShowHigh() => _highlightText.SetActive(true);
    public void HideHigh() => _highlightText.SetActive(false);

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
    }




}
