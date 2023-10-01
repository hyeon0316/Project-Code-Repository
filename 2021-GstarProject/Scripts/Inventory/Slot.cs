using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class Slot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Item item; //획득한 아이템
    public int itemCount; //획득한 아이템의 개수
    public Image itemImage; //아이템의 이미지

    //더블클릭 관련 변수들
    private float doubleClickTime = 0.25f; 
    private bool isOneClick = false;
    private bool isDoubleClick = false;
    private double c_Timer = 0;

    //필요 컴포넌트
    [SerializeField]
    private Text text_Count;//아이템 갯수 표시
    [SerializeField]
    private GameObject go_CountImage; //빈슬롯일땐 카운트배경이미지를 띄우지 않기 위함
    [SerializeField]
    private Information information;

    [SerializeField]
    private ToolTipDataBase theToolTipDatabase;

    private Rect invenBaseRect; //Inventory_Base 이미지의 Rect 정보 

    private InputNumber theInputNumber;
    private E_Slot e_Slot;

    public static bool pMemory = false;//장착된 장비와 착용할 장비가 서로 교체 될때 사용되는 변수
    public static int pNumber;//배열로 선언된 슬롯들중 해당 슬롯의 인덱스를 기억하기 위한 변수

    public GameObject inforPage;
    private void Start()
    {
        e_Slot = FindObjectOfType<E_Slot>();
        theInputNumber = FindObjectOfType<InputNumber>();
        invenBaseRect = transform.parent.parent.GetComponent<RectTransform>().rect;
    }

    private void Update()
    {
        SetCountImage(); 
    }

    private void SetCountImage()//아이템 갯수당 이미지 표시
    {
        RectTransform rect = (RectTransform)go_CountImage.transform;
        if (itemCount >= 1 && itemCount < 9)
            rect.sizeDelta = new Vector2(13, 13);
        else if (itemCount >= 10 && itemCount < 99)
            rect.sizeDelta = new Vector2(17, 13);
        else if (itemCount >= 100 && itemCount < 999)
            rect.sizeDelta = new Vector2(21, 13);
    }

    private void SetColor(float _alpha) //이미지 투명도 조절
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    public void AddItem(Item _item, int _count = 1)//아이템 획득
    {
        item = _item;
        itemCount = _count;
        itemImage.sprite = item.itemImage;


        if (item.itemType != Item.ItemType.Equipment)
        {
            go_CountImage.SetActive(true);
            text_Count.text = itemCount.ToString();
        }
        else    //장비의 경우 개수표현X(단일 아이템)
        {
            text_Count.text = "0";
            go_CountImage.SetActive(false);
        }
        SetColor(1);
    }

    public void SetSlotCount(int _conut) //아이템 개수 조정
    {
        itemCount += _conut;
        text_Count.text = itemCount.ToString();

        if (itemCount <= 0)
            ClearSlot();
    }

    public void ClearSlot()//슬롯 초기화
    {
        item = null;
        itemCount = 0;
        itemImage.sprite = null;
        SetColor(0);

        text_Count.text = "0";
        go_CountImage.SetActive(false);
    }

    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) //우클릭하여 아이템 사용
        {
            UseItem();
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
                UseItem();
                isDoubleClick = false;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            SoundManager.inst.SFXPlay("Drag", SoundManager.inst.uiList[1]);
            DragSlot.instance.dragSlot = this;
            DragSlot.instance.DragSetImage(itemImage);
            DragSlot.instance.transform.position = eventData.position; //마우스 포지션
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            DragSlot.instance.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData) //드래그가 끝날때 호출
    {
        //정보창 Off
        if (!inforPage.activeSelf)
        {
            if (!((DragSlot.instance.transform.localPosition.x > invenBaseRect.xMin
                && DragSlot.instance.transform.localPosition.x < invenBaseRect.xMax
                && DragSlot.instance.transform.localPosition.y > invenBaseRect.yMin
                && DragSlot.instance.transform.localPosition.y < invenBaseRect.yMax) ||
                     (DragSlot.instance.transform.position.y > 31 //아이템 사용창 범위
                     && DragSlot.instance.transform.position.y < 138
                     && DragSlot.instance.transform.position.x > 1390
                     && DragSlot.instance.transform.position.x < 1830)))
            {
                Debug.Log(DragSlot.instance.transform.position);
                if (DragSlot.instance.dragSlot != null)
                    theInputNumber.Call();
            }
            else if ((DragSlot.instance.transform.localPosition.x > invenBaseRect.xMin
               && DragSlot.instance.transform.localPosition.x < invenBaseRect.xMax
               && DragSlot.instance.transform.localPosition.y > invenBaseRect.yMin
               && DragSlot.instance.transform.localPosition.y < invenBaseRect.yMax) ||
                    (DragSlot.instance.transform.position.y > 31
                    && DragSlot.instance.transform.position.y < 138
                    && DragSlot.instance.transform.position.x > 1390
                    && DragSlot.instance.transform.position.x < 1830))
            {
                DragSlot.instance.SetColor(0);
                DragSlot.instance.dragSlot = null;
            }
        }

        else if (inforPage.activeSelf)
        {
            if (!((DragSlot.instance.transform.localPosition.x > invenBaseRect.xMin //정보창 or 인벤창 밖에서 드롭 시
               && DragSlot.instance.transform.localPosition.x < invenBaseRect.xMax
               && DragSlot.instance.transform.localPosition.y > invenBaseRect.yMin
               && DragSlot.instance.transform.localPosition.y < invenBaseRect.yMax) ||
               (DragSlot.instance.transform.position.y > 241
                && DragSlot.instance.transform.position.y < 864
                && DragSlot.instance.transform.position.x > 254
                && DragSlot.instance.transform.position.x < 700) ||
                  (DragSlot.instance.transform.position.y > 31
                    && DragSlot.instance.transform.position.y < 138
                    && DragSlot.instance.transform.position.x > 1390
                    && DragSlot.instance.transform.position.x < 1830)))
            {
                if (DragSlot.instance.dragSlot != null)
                    theInputNumber.Call();
            }

            else if ((DragSlot.instance.transform.localPosition.x > invenBaseRect.xMin//정보창 or 인벤창 안에서 드롭 시
                && DragSlot.instance.transform.localPosition.x < invenBaseRect.xMax
                && DragSlot.instance.transform.localPosition.y > invenBaseRect.yMin
                && DragSlot.instance.transform.localPosition.y < invenBaseRect.yMax) ||
                (DragSlot.instance.transform.position.y > 241
                 && DragSlot.instance.transform.position.y < 864
                 && DragSlot.instance.transform.position.x > 254
                 && DragSlot.instance.transform.position.x < 700) ||
                  (DragSlot.instance.transform.position.y > 31
                    && DragSlot.instance.transform.position.y < 138
                    && DragSlot.instance.transform.position.x > 1390
                    && DragSlot.instance.transform.position.x < 1830))
            {
                DragSlot.instance.SetColor(0);
                DragSlot.instance.dragSlot = null;
            }

        }
    }

    public void OnDrop(PointerEventData eventData) //다른 슬롯 위에서 드래그가 끝날때 호출
    {
        if (DragSlot.instance.dragSlot != null)//빈슬롯을 드래그해서 Null 참조를 발생하는 것을 방지    
            ChangeSlot();


        if (DragSlot_Equip.instance.dragSlot_Equip != null)
        {
            Inter_ChangeSlot();
        }

        if (DragSlot_Used.instance.dragSlot_Used != null)
        {
            Inter_Change_uSlot();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null)
            theToolTipDatabase.ShowToolTip(item, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        theToolTipDatabase.HideToolTip();
    }

    private void UseItem()//장비 or 소모품 사용
    {
        if (item != null)
        {
            if (item.itemType == Item.ItemType.Equipment)
            {
                pMemory = true;
                for (int i = 0; i < transform.parent.parent.parent.GetComponent<Inventory>().slots.Length; i++) //슬롯 자리 검사
                {
                    if (transform.parent.parent.parent.GetComponent<Inventory>().slots[i].item != null)
                    {
                        if (transform.parent.parent.parent.GetComponent<Inventory>().slots[i].item.Equals(item))
                        {
                            pNumber = i;
                            break;
                        }
                    }
                }
                information.EquipItem(item);//장착
                if (Information.slotClear)//장비를 장착할때 해당 장비창이 비어있을 때(교체할 필요가 없을 때)
                {
                    ClearSlot();
                    Information.slotClear = false;
                }
            }
            else
            {
                if (item.itemType == Item.ItemType.Used)
                {
                    if (item.itemName == "체력포션(대)" || item.itemName == "체력포션(중)" || item.itemName == "체력포션(소)"
                        || item.itemName == "파워엘릭서" || item.itemName == "엘릭서")
                    {
                        Player.inst.HealHp(item);
                    }
                    else if (item.itemName == "마나포션(대)" || item.itemName == "마나포션(중)" || item.itemName == "마나포션(소)")
                    {
                        Player.inst.HealMp(item);
                    }

                    if (Player.slotCountClear)
                    {
                        SetSlotCount(-1);
                        Player.slotCountClear = false;
                    }
                }
            }
        }
    }

    private void ChangeSlot()//a와 b의 자리를 바꿀 때,
    {
        Item _tempItem = item; //드래그가 끝날때 복사될 b(드래그가 끝나는 시점에서 해당 슬롯에 있었던 아이템정보를 복사함)
        int _tempItemCount = itemCount;

        if (_tempItem == null)
            AddItem(DragSlot.instance.dragSlot.item, DragSlot.instance.dragSlot.itemCount);//b 자리에 a가 들감
        else if (DragSlot.instance.dragSlot.item.itemName == item.itemName)//인벤창에서 같은 아이템을 드래그로 합칠 때
        {
            if (DragSlot.instance.dragSlot.item.itemType == Item.ItemType.Used)
                SetSlotCount(DragSlot.instance.dragSlot.itemCount);
        }

        if (_tempItem != null && DragSlot.instance.dragSlot.item.itemName != item.itemName)//a자리에 b가 들어갈 때   
        {
            AddItem(DragSlot.instance.dragSlot.item, DragSlot.instance.dragSlot.itemCount);
            DragSlot.instance.dragSlot.AddItem(_tempItem, _tempItemCount);//a와 b가 교환할 때 
        }
        else
            DragSlot.instance.dragSlot.ClearSlot();//빈자리로 이동할 때

    }

    private void Inter_ChangeSlot() //정보창에서 인벤창으로 드래그
    {
        Item _tempItem = item;

        AddItem(DragSlot_Equip.instance.dragSlot_Equip.e_item);

        if (_tempItem != null && item.EquipType == DragSlot_Equip.instance.dragSlot_Equip.e_item.EquipType)
        {
            Player.inst.TakeOffEffect(DragSlot_Equip.instance.dragSlot_Equip.e_item);
            DragSlot_Equip.instance.dragSlot_Equip.AddEquipItem(_tempItem);
        }
        else
            DragSlot_Equip.instance.dragSlot_Equip.ClearSlot();
    }

    private void Inter_Change_uSlot()//아이템 사용창에서 인벤창으로 드래그(아이템 사용창에는 이미 소모품 상태임)
    {
        if (item ==null || item.itemType == Item.ItemType.Used)//item 이 null인지 먼저 검사해야함
        {
            Item _tempItem = item;
            int _tempItemCount = itemCount;

            AddItem(DragSlot_Used.instance.dragSlot_Used.item, DragSlot_Used.instance.dragSlot_Used.itemCount);

            if (_tempItem != null && DragSlot_Used.instance.dragSlot_Used.item.itemName == _tempItem.itemName)
            {
                if (DragSlot_Used.instance.dragSlot_Used.item.itemType == Item.ItemType.Used)
                {
                    SetSlotCount(DragSlot_Used.instance.dragSlot_Used.itemCount);
                    DragSlot_Used.instance.dragSlot_Used.ClearSlot();
                }
            }

            else if (_tempItem != null)//아이템이 있는 슬롯에 드래그 했을때(a,b 교환)
            {
                DragSlot_Used.instance.dragSlot_Used.AddItem(_tempItem, _tempItemCount);
            }
            else if (_tempItem == null)//빈슬롯에 드래그 했을때
            {
                DragSlot_Used.instance.dragSlot_Used.ClearSlot();
            }
        }
    }
}

   

