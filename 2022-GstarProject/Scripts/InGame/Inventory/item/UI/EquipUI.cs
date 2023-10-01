using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipUI : MonoBehaviour
{
    
    private Inventory _inventory;
    [SerializeField]
    private List<EquipSlotUI> _slotUIList = new List<EquipSlotUI>();

    [SerializeField] private GameObject _EquipActiveUI;

    private List<RaycastResult> _rrList;
    private GraphicRaycaster _gr;
    private PointerEventData _ped;
    private const int SIndex = 90;
    private int _useSlotindex = SIndex;
    public Stat Stat;

    private void Start()
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

        
        for (int i = 0; i <_inventory.ECapacity; i++)
        {
            _slotUIList[i].SetSlotIndex(i);
        }
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
            EquipSlotUI slot = RaycastFrist<EquipSlotUI>();
            if (slot != null && slot.HasItem && slot.IsAccessible)
            {
                TryUseItem(slot.Index);
            }
        }
    }
    private void TryUseItem(int index)
    {
        
        if (_useSlotindex == SIndex)
        {
            _useSlotindex = index;
            _slotUIList[index].ShowHigh();
            _inventory.ShowActiveESlot(index);

        }
        else if (_useSlotindex == index)
        {

            _slotUIList[index].HideHigh();
            _inventory.HideActiveESlot();
            _useSlotindex = 90;
            _inventory.UnEquip(index);
        }
        else
        {
            _slotUIList[_useSlotindex].HideHigh();
            _inventory.HideActiveESlot();
            _slotUIList[index].ShowHigh();
            _inventory.ShowActiveESlot(index);
            _useSlotindex = index;
        }
    }
    private T RaycastFrist<T>() where T : Component
    {
        _rrList.Clear();

        _gr.Raycast(_ped, _rrList);

        if (_rrList.Count == 0)
            return null;
        for (int i = 0; i < 10; i++)
        {
            if (_rrList[i].gameObject.tag == "Slot")
            {
                return _rrList[i].gameObject.GetComponent<T>();
            }

        }
        return null;
    }
    public void RemoveItem(int index)
    {
        _slotUIList[index].RemoveItem();
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
}
