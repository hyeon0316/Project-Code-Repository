using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ItemType
{
    Attack,
    Defense,
    Heal,
    Ability
}

public abstract class Item : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler, ISetAlpha
{
    private Vector3 _originPos;
    private Transform _originParent;
    private int _slotIndex1;
    private int _slotIndex2;

    [SerializeField] private string _name;

    /// <summary>
    /// 아이템 티어
    /// </summary>
    [SerializeField] private byte _tier;


    /// <summary>
    /// 아이템 타입
    /// </summary>
    [SerializeField] protected ItemType _curItemType;


    /// <summary>
    /// 아이템 설명
    /// </summary>
    [SerializeField][TextArea] private string _description;

    /// <summary>
    /// 효과 설명
    /// </summary>
    protected string _effect;

    /// <summary>
    /// 0, 1성
    /// 1, 2성
    /// 2, 3성 (최대)
    /// </summary>
    public byte Upgrade { get; set; }

    /// <summary>
    /// 특정 아이템에 사용될 bool값
    /// </summary>
    public bool IsFlag { get; set; }

    /// <summary>
    /// 드래그 중일때
    /// </summary>
    private bool _isDrag;

    public EItemCode Code { get; set; }

    [SerializeField] protected Image _image;

    [SerializeField] private GameObject[] _starObjects;

    /// <summary>
    /// 해당 아이템에 마우스가 오버되었는지
    /// </summary>
    private bool _isMouseOver;

    private void Update()
    {
        CancelDrag();
    }

    public void SetActiveStarObject()
    {
        int count = 0;

        if (_starObjects.Length == 0)
            return;

        if (Upgrade == 0)
            return;

        foreach (var star in _starObjects)
        {
            if (count <= Upgrade)
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

    /// <summary>
    /// 장착효과가 있는 아이템을 장착했을때 스탯 적용
    /// </summary>
    public virtual void SetEquipEffect(int playerID) { }

    /// <summary>
    /// 업그레이드 별 아이템 스탯 적용
    /// </summary>
    public virtual void ApplyUpgrade() { }

    public abstract void SetEquipEffectText();

    protected virtual void ShowEquipEffectPanel() { }

    /// <summary>
    /// 아이템 효과 사용
    /// </summary>
    public abstract void Active(BattleCharacter player, BattleCharacter opponent);


    /// <summary>
    /// 전투단계로 전환될때 드래그 중이었던 아이템을 다시 원래 자리로 옮김
    /// </summary>
    private void CancelDrag()
    {
        if (_isDrag)
        {
            if (WindowManager.Instance.GetInGame().ReadyWindow.ReadyTimer == 0
                || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
            {
                RePosItem(_originParent, _originParent.position);

                _isDrag = false;
                CursorManager.Instance.IsDrag = _isDrag;
                _image.raycastTarget = true;
            }
        }
    }


    public void OnPointerClick(PointerEventData eventData) //필드에 있는 아이템 설명 표시
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ray2D ray = new Ray2D(pos, Vector2.zero);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("FieldSlot"));

            if (hit.collider == null)
            {
                ToolTipManager.Instance.CloseToolTip();
                ToolTipManager.Instance.ShowToolTip(_image.sprite, _tier, Upgrade, _name, _curItemType, _description, _effect);
                ShowEquipEffectPanel();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsAlphaZero())
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ray2D ray = new Ray2D(pos, Vector2.zero);
            RaycastHit2D hit =
                Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("FieldSlot"));

            if (hit.collider != null)
            {
                _isMouseOver = true;
                ToolTipManager.Instance.ShowToolTip(_image.sprite, _tier, Upgrade, _name, _curItemType, _description, _effect);
                ShowEquipEffectPanel();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isMouseOver)
        {
            _isMouseOver = false;
            ToolTipManager.Instance.CloseToolTip();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ray2D ray = new Ray2D(pos, Vector2.zero);
            RaycastHit2D hit =
                Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("ItemSlot"));

            if (InGame.CurGameType == EGameType.Ready)
            {
                if (hit.collider != null)
                {
                    SoundManager.Instance.PlayEffect(EffectType.ItemDrag);
                    ToolTipManager.Instance.CloseToolTip();

                    _originPos = this.transform.position;
                    _originParent = this.transform.GetComponentInParent<ItemSlot>().transform;

                    _slotIndex1 = transform.GetComponentInParent<ItemSlot>().GetSlotIndex();

                    transform.position = eventData.position;
                    transform.SetParent(GameObject.Find("DragItem").transform);

                    this.GetComponent<Image>().raycastTarget = false; //드래그 해서 놓았을때 해당 슬롯의 hit정보를 얻기 위함
                    _isDrag = true;
                    CursorManager.Instance.IsDrag = _isDrag;
                }
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (InGame.CurGameType == EGameType.Ready)
        {
            if (_isDrag)
            {
                Vector3 pos = Input.mousePosition;
                pos = Camera.main.ScreenToWorldPoint(pos); //마우스 좌표를 월드 좌표(카메라 안)로 변환
                transform.position = new Vector3(pos.x, pos.y, 0);


            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (InGame.CurGameType == EGameType.Ready)
        {
            if (_isDrag)
            {
                EndDragEvent();
                _isDrag = false;
                CursorManager.Instance.IsDrag = _isDrag;
            }
        }
    }

    /// <summary>
    /// 드래그를 끝냈을때 레이어에 따른 이벤트
    /// </summary>
    private void EndDragEvent()
    {
        if (InGame.CurGameType != EGameType.Ready)
        {
            return;
        }

        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(pos, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity,
            LayerMask.GetMask("ItemSlot", "Drop", "CombinationSlot"));

        if (hit.collider != null)
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("ItemSlot")) //슬롯칸에 드래그 했을 때
            {
                DragItemSlot(hit);
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Drop")) //아이템을 버릴때
            {
                DropItem();
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("CombinationSlot")) //재조합 슬롯에 드래그 했을 때
            {
                RePosItem(_originParent, _originPos);
                hit.transform.GetComponent<CombinationSlot>().SetSlotItem((Byte)_slotIndex1, _image.sprite);
            }
        }
        else //아무것도 없는 공간에 드래그 했을 때
        {
            SoundManager.Instance.PlayEffect(EffectType.ItemDragEnd);
            RePosItem(_originParent, _originPos);
        }

        _image.raycastTarget = true;
    }

    private void DragItemSlot(RaycastHit2D hit)
    {
        ItemSlot slot = hit.collider.GetComponent<ItemSlot>();
        _slotIndex2 = slot.GetSlotIndex();

        Player player = PlayerManager.Instance.Players[0];

        if (_slotIndex2 < 6) //Using슬롯
        {
            if (slot.IsExistItem()) //아이템이 존재하는 슬롯에 놓을때
            {
                if (player.UsingInventory.CheckDuplicationItem(Code)) //Using인벤에 놓았을때 중복 아이템이면 
                {
                    RePosItem(_originParent, _originPos);
                    if (CheckUpgradeCondition(slot.GetItem().Code, slot.GetItem().Upgrade)) //업그레이드
                    {
                        if (_slotIndex1 == _slotIndex2) //자기 자신은 업그레이드 불가
                        {
                            _image.raycastTarget = true;
                            return;
                        }

                        SoundManager.Instance.PlayEffect(Upgrade == 0 ? EffectType.Upgrade : EffectType.Upgrade2);
                        SetAlpha(0);
                        NetworkManager.Instance.SendUpgradeItemPacket(player.ID, (Byte)_slotIndex1, (Byte)_slotIndex2);
                        WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckDuplicationIndex((Byte)_slotIndex1);
                        WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckDuplicationIndex((Byte)_slotIndex2);
                    }
                    else //업그레이드 불가
                    {
                        if (CheckEqualsItem(slot.GetItem().Code)) // 같은 아이템이지만 업그레이드 단계가 다를 경우
                        {
                            SoundManager.Instance.PlayEffect(EffectType.ItemDragEnd);
                            SetAlpha(0);
                            NetworkManager.Instance.SendChangeItemSlotPacket(player.ID, (Byte)_slotIndex1, (Byte)_slotIndex2);
                            WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckDuplicationIndex((Byte)_slotIndex1);
                            WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckDuplicationIndex((Byte)_slotIndex2);

                            PlayerManager.Instance.GetPlayer(player.ID).SwapItemNetwork((Byte)_slotIndex1, (Byte)_slotIndex2);
                        }
                        else //취소
                        {
                            SoundManager.Instance.PlayEffect(EffectType.ItemDragEnd);
                        }
                    }
                }
                else // 자리 변경
                {
                    RePosItem(_originParent, _originPos);
                    SoundManager.Instance.PlayEffect(EffectType.ItemDragEnd);
                    SetAlpha(0);
                    NetworkManager.Instance.SendChangeItemSlotPacket(player.ID, (Byte)_slotIndex1, (Byte)_slotIndex2);
                    WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckDuplicationIndex((Byte)_slotIndex1);
                    WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckDuplicationIndex((Byte)_slotIndex2);
                    PlayerManager.Instance.GetPlayer(player.ID).SwapItemNetwork((Byte)_slotIndex1, (Byte)_slotIndex2);
                }
            }
            else //아이템이 없는 슬롯에 놓을때
            {
                if (player.UsingInventory.CheckDuplicationItem(Code)) //Using인벤에 놓았을때 중복 아이템이면 
                {
                    RePosItem(_originParent, _originPos);
                    _image.raycastTarget = true;
                    return;
                }

                SoundManager.Instance.PlayEffect(EffectType.ItemDragEnd);
                RePosItem(_originParent, _originPos);
                SetAlpha(0);
                NetworkManager.Instance.SendChangeItemSlotPacket(player.ID, (Byte)_slotIndex1, (Byte)_slotIndex2);
                WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckDuplicationIndex((Byte)_slotIndex1);
                WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckDuplicationIndex((Byte)_slotIndex2);
                PlayerManager.Instance.GetPlayer(player.ID).SwapItemNetwork((Byte)_slotIndex1, (Byte)_slotIndex2);
            }
        }
        else //UnUsing 슬롯
        {
            SetAlpha(0);
            RePosItem(_originParent, _originPos);
            WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckDuplicationIndex((Byte)_slotIndex1);
            WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckDuplicationIndex((Byte)_slotIndex2);
            if (slot.IsExistItem()) //아이템이 존재할때
            {
                if (CheckUpgradeCondition(slot.GetItem().Code, slot.GetItem().Upgrade)) //업그레이드 할때
                {
                    if (_slotIndex1 == _slotIndex2) //자기 자신은 업그레이드 불가
                    {
                        SetAlpha(1);
                        _image.raycastTarget = true;
                        return;
                    }

                    SoundManager.Instance.PlayEffect(Upgrade == 0 ? EffectType.Upgrade : EffectType.Upgrade2);
                    NetworkManager.Instance.SendUpgradeItemPacket(player.ID, (Byte)_slotIndex1, (Byte)_slotIndex2);
                    PlayerManager.Instance.GetPlayer(player.ID).SwapItemNetwork((Byte)_slotIndex1, (Byte)_slotIndex2);
                }
                else // 자리 변경
                {
                    SoundManager.Instance.PlayEffect(EffectType.ItemDragEnd);
                    NetworkManager.Instance.SendChangeItemSlotPacket(player.ID, (Byte)_slotIndex1, (Byte)_slotIndex2);
                    PlayerManager.Instance.GetPlayer(player.ID).SwapItemNetwork((Byte)_slotIndex1, (Byte)_slotIndex2);
                }
            }
            else // 아이템이 없는 슬롯에 놓을때
            {
                SoundManager.Instance.PlayEffect(EffectType.ItemDragEnd);
                NetworkManager.Instance.SendChangeItemSlotPacket(player.ID, (Byte)_slotIndex1, (Byte)_slotIndex2);
                PlayerManager.Instance.GetPlayer(player.ID).SwapItemNetwork((Byte)_slotIndex1, (Byte)_slotIndex2);
            }
        }
    }

    private void DropItem()
    {
        SoundManager.Instance.PlayEffect(EffectType.ItemDrop);
        SetAlpha(0);
        RePosItem(_originParent, _originPos);
        NetworkManager.Instance.SendDropItemPacket(PlayerManager.Instance.Players[0].ID, (Byte)_slotIndex1);
        WindowManager.Instance.GetInGame().ReadyWindow.Drop.SetCloseSprite();
        Destroy(this.gameObject);
    }

    private void EndDrag_Combination()
    {

    }


    /// <summary>
    /// 업그레이드 가능한지(아이템코드와, 업그레이드 성이 같은지) 확인, 3성이 아닌지 확인
    /// </summary>
    private bool CheckUpgradeCondition(EItemCode code, int upgrade)
    {
        return Code == code && Upgrade == upgrade && Upgrade != 2;
    }

    /// <summary>
    /// 아이템 종류가 같은지
    /// </summary>
    private bool CheckEqualsItem(EItemCode code)
    {
        return Code == code;
    }

    /// <summary>
    /// 아이템의 슬롯 위치를 변경 
    /// </summary>
    public void RePosItem(Transform parent, Vector3 pos)
    {
        transform.SetParent(parent);
        transform.position = pos;
    }

    /// <summary>
    /// 아이템 슬롯의 알파값이 제로인지 확인
    /// </summary>
    /// <returns></returns>
    public bool IsAlphaZero()
    {
        Color color = _image.color;
        color.a = 0;

        return _image.color == color;
    }


    public void SetAlpha(float value)
    {
        Color color = _image.color;
        color.a = value;
        _image.color = color;

        if (value == 0)
        {
            if (_starObjects.Length != 0)
            {
                foreach (var star in _starObjects)
                {
                    star.SetActive(false);
                }
            }
        }
        else
        {
            SetActiveStarObject();
        }
    }
}
