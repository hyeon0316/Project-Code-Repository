using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [Tooltip("������ ������ �̹���")]
    [SerializeField] private Image _iconImage;

    [Tooltip("������ ���� �ؽ�Ʈ")]
    [SerializeField] private Text _amountText;

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
    private GameObject _textGo;
    private GameObject _highlightGo;

    private Image _slotImage;
    private InventoryUI _inventoryUI;

    private bool _isAccessibleSlot = true; // ���� ���ٰ��� ����
    private bool _isAccessibleItem = true; // ������ ���ٰ��� ����
    private bool _isActive = false; // Ȱ��ȭ �Ǿ��ִ��� Ȯ��

    /// <summary> ��Ȱ��ȭ�� ������ ���� </summary>
    private static readonly Color InaccessibleSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    /// <summary> ��Ȱ��ȭ�� ������ ���� </summary>
    private static readonly Color InaccessibleIconColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private void ShowIcon() => _iconGo.SetActive(true);
    private void HideIcon() => _iconGo.SetActive(false);

    public void ShowHigh() => _highlightText.SetActive(true);
    public void HideHigh() => _highlightText.SetActive(false);

    private void ShowText() => _textGo.SetActive(true);
    private void HideText() => _textGo.SetActive(false);

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
        _textGo = _amountText.gameObject;
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
        HideText();
    }


    /// <summary> ������ ���� �ؽ�Ʈ ����(amount�� 1 ������ ��� �ؽ�Ʈ ��ǥ��) </summary>
    public void SetItemAmount(int amount)
    {
        if (HasItem && amount > 1)
            ShowText();
        else
            HideText();

        _amountText.text = amount.ToString();
    }

}
