using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;



public class InventoryUI : MonoBehaviour
{

    private Inventory _inventory;
    [SerializeField]
    private List<ItemSlotUI> _slotUIList = new List<ItemSlotUI>();

    private ItemSlotUI _activeSlot;
   
    
    private int _useSlotindex;
    private List<RaycastResult> _rrList;
    private GraphicRaycaster _gr;
    private PointerEventData _ped;

    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        TryGetComponent(out _gr);
        if (_gr == null)
            _gr = gameObject.AddComponent<GraphicRaycaster>();

        _ped = new PointerEventData(EventSystem.current);
        _rrList = new List<RaycastResult>(10);

        for(int i =0; i<_inventory.Capacity;i++)
        {
            _slotUIList[i].SetSlotIndex(i);
        }

        _useSlotindex = _inventory.USESTARTINDEX;

    }

    private void Update()
    {
        _ped.position = Input.mousePosition;
        OnClickIn();
    
    }
    /// <summary> 마우스 클릭할경우 </summary>
    private void OnClickIn()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ItemSlotUI slot = RaycastFrist<ItemSlotUI>();
            if (slot != null && slot.HasItem && slot.IsAccessible)
            {
                TryUseItem(slot.Index);
            }
        }
    }

    private void TryUseItem(int index)
    {
        int startIndex = _inventory.USESTARTINDEX;
        if (_inventory._useSlotindex == startIndex)
        {
            _inventory._useSlotindex = index;
            _inventory.ShowActiveISlot(index);
            _slotUIList[index].ShowHigh();

        }
        else if (_inventory._useSlotindex == index)
        {

            _slotUIList[index].HideHigh();
            _inventory.HideActiveISlot();
            _inventory._useSlotindex = 90;
            _inventory.Use(index);
        }
        else
        {
            _slotUIList[_inventory._useSlotindex].HideHigh();
            _inventory.HideActiveISlot();
            _inventory.ShowActiveISlot(index);
            _slotUIList[index].ShowHigh();
            _inventory._useSlotindex = index;
        }
    }
    /// <summary> 레이케스트 첫번째 Component </summary>
    private T RaycastFrist<T>() where T:Component
    {
        _rrList.Clear();
        
        _gr.Raycast(_ped, _rrList);

        if (_rrList.Count == 0)
            return null;
        for(int i = 0; i<10;i++)
        {
            if(_rrList[i].gameObject.tag == "Slot")
            {
                return _rrList[i].gameObject.GetComponent<T>();
            }
                
        }
        return null;
    }

    public void SetInventoryReference(Inventory inventory)
    {
        _inventory = inventory;
    }

    /// <summary> 슬롯에 아이템 아이콘 등록 </summary>
    public void SetItemIcon(int index, Sprite icon)
    {
        _slotUIList[index].SetItem(icon);
    }

    /// <summary> 해당 슬롯의 아이템 개수 텍스트 지정 </summary>
    public void SetItemAmountText(int index, int amount)
    {
        _slotUIList[index].SetItemAmount(amount);
    }

    /// <summary> 해당 슬롯의 아이템 개수 텍스트 지정 </summary>
    public void HideItemAmountText(int index)
    {
        _slotUIList[index].SetItemAmount(1);
    }

    /// <summary> 슬롯에서 아이템 아이콘 제거, 개수 텍스트 숨기기 </summary>
    public void RemoveItem(int index)
    {
        _slotUIList[index].RemoveItem();
    }
}
