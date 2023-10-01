using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class E_Slot : MonoBehaviour, IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Item e_item; //획득한 아이템
    public Image e_itemImage; //아이템의 이미지

    private float doubleClickTime = 0.25f; //더블클릭 관련 변수들
    private bool isOneClick = false;
    private bool isDoubleClick = false;
    private double c_Timer = 0;

    [SerializeField]
    private Inventory inventory;

    [SerializeField]
    private ToolTipDataBase theToolTipDatabase;

    private void SetColor(float _alpha) //이미지 투명도 조절
    {
        Color color = e_itemImage.color;
        color.a = _alpha;
        e_itemImage.color = color;
    }

    public void AddEquipItem(Item _item) //정보창에 장비 장착
    {
        e_item = _item;
        e_itemImage.sprite = e_item.itemImage;      
        SetColor(1);
        Player.inst.EquipEffect(e_item);
    }

    public void ClearSlot()//정보창 슬롯 초기화
    {
        Player.inst.TakeOffEffect(e_item);
        e_item = null;
        e_itemImage.sprite = null;
        SetColor(0);       
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) //우클릭하여 장비해제
        {
            if (e_item != null)
            {
                inventory.AcquireItem(e_item);
                ClearSlot();
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Left) //더블클릭
        {
            if (!isOneClick)
            {
                c_Timer = Time.time;
                isOneClick = true;
            }
            else if (isOneClick && ((Time.time - c_Timer) > doubleClickTime))
            {
                isOneClick = false;
            }
            else if (isOneClick && ((Time.time - c_Timer) < doubleClickTime))
            {
                isOneClick = false;
                isDoubleClick = true;
            }

            if (isDoubleClick)
            {
                if (e_item != null)
                {
                    inventory.AcquireItem(e_item);
                    ClearSlot();
                }
                isDoubleClick = false;
            }
        }
    }

    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(e_item !=null)
        {
            DragSlot_Equip.instance.dragSlot_Equip = this;
            DragSlot_Equip.instance.DragSetImage(e_itemImage);
            DragSlot_Equip.instance.transform.position = eventData.position; //마우스 포지션
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (e_item != null)
        {
            DragSlot_Equip.instance.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {             
        DragSlot_Equip.instance.SetColor(0);           
        DragSlot_Equip.instance.dragSlot_Equip = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (DragSlot.instance.dragSlot.item.itemType == Item.ItemType.Equipment)
        {
            if (this.gameObject.tag == DragSlot.instance.dragSlot.item.EquipType)
                Inter_ChangeSlot();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (e_item != null)
            theToolTipDatabase.ShowToolTip(e_item, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        theToolTipDatabase.HideToolTip();
    }

    private void Inter_ChangeSlot() //인벤창에서 정보창으로 드래그
    {
        Item _tempItem = e_item;

        AddEquipItem(DragSlot.instance.dragSlot.item);

        if (_tempItem != null)
        {
            DragSlot.instance.dragSlot.AddItem(_tempItem);
            Player.inst.TakeOffEffect(_tempItem);
        }
        else
            DragSlot.instance.dragSlot.ClearSlot();
    }

    
}
