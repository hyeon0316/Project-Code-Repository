using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class EquipSlotUI : MonoBehaviour
{
    [Tooltip("������ ������ �̹���")]
    [SerializeField] private Image _iconImage;

    [Tooltip("������ ��Ŀ���� �� ��Ÿ���� �ؽ�Ʈ")]
    [SerializeField] private GameObject _highlightText;

    

    /// <summary> ������ �ε��� </summary>
    public int Index { get; private set; }

    /// <summary> ������ �������� �����ϰ� �ִ��� ���� </summary>
    public bool HasItem => _iconImage.sprite != null;

    /// <summary> ���� ������ �������� ���� </summary>
    public bool IsAccessible => _isAccessibleSlot && _isAccessibleItem;

    public RectTransform SlotRect => _slotRect;
    public RectTransform IconRect => _iconRect;
    

    // private InventoryUI _inventoryUI;

    private RectTransform _slotRect;
    private RectTransform _iconRect;

    private GameObject _iconGo;

    private Image _slotImage;
    private InventoryUI _inventoryUI;
    // ���� ���̶���Ʈ ���İ�
    private float _currentHLAlpha = 0f;

    private bool _isAccessibleSlot = true; // ���� ���ٰ��� ����
    private bool _isAccessibleItem = true; // ������ ���ٰ��� ����
    private bool _isActive = false; // Ȱ��ȭ �Ǿ��ִ��� Ȯ��

    private void ShowIcon() => _iconGo.SetActive(true);
    private void HideIcon() => _iconGo.SetActive(false);

    public void ShowHigh() => _highlightText.SetActive(true);
    public void HideHigh() => _highlightText.SetActive(false);

    public void SetSlotIndex(int index) => Index = index;

    /// <summary> ���Կ� ������ ��� </summary>
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
    /// <summary> ���Կ��� ������ ���� </summary>
    public void RemoveItem()
    {
        _iconImage.sprite = null;
        HideIcon();
    }




}
