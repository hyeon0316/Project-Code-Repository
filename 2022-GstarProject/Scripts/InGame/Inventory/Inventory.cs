using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    [SerializeField]
    private InventoryUI _inventoryUI;
    [SerializeField]
    private Item[] _items;

    [SerializeField]
    private EquipUI _equipmentUI;
    [SerializeField]
    private Item[] _equ;

    [SerializeField]
    private ActiveSlotUI _activeESlot;

    [SerializeField]
    private ActiveSlotUI _activeISlot;

    public Stat InvenStat;

    [SerializeField]
    private Enforce _enforce;
    private int _maxCapacity = 80;
    private int _equCount = 6;

    public int USESTARTINDEX = 90;
    public int _useSlotindex;
    [SerializeField] private UsePortion usePortion;
    private int _activeSlotNum;
    public GameObject InvenUI;
    public GameObject StatUI;
    public EnforceUI enforceUI;
    public GameObject Inven;
    private int portionIndex;
    [SerializeField]
    private GameObject per;


    private void Start()
    {

        DataManager.Instance.Gold = 10000;
        DataManager.Instance.SetGold(0);
    }
    public void BtnPerExit()
    {
        per.SetActive(false);
    }
    public void BtnPerOpen()
    {
        per.SetActive(true);
    }
    public void ButtonEnforce()
    {
        if (enforceUI.gameObject.activeSelf)
        {

            if (_activeSlotNum < 1)
            {
                if (SetEnforceUI(102))
                {
                    for (int i = 0; i < _items.Length; i++)
                    {
                        if (_items[i] != null)
                        {
                            if (_items[i].Data.ID == 102)
                            {
                                CountableItem asCount = _items[i] as CountableItem;
                                asCount.SetAmount(asCount.Amount - _enforce.EnforceNum[_activeSlotNum] * 2);
                                if (asCount.IsEmpty)
                                    Remove(i);
                                UpdateSlot(i);
                            }
                        }
                    }
                    ItemStat _data;
                    _data = _enforce.GetStat(_activeSlotNum);
                    UnEquPlayerStat(_data);
                    _enforce.OnButton(_activeSlotNum);
                    _data = _enforce.GetStat(_activeSlotNum);
                    EquPlayerStat(_data);
                    _activeESlot.UpdateEnforceStat(_data, _enforce.EnforceNum[_activeSlotNum]);
                    enforceUI.gameObject.SetActive(false);
                    SoundManager.Instance.EffectPlay(EffectSoundType.enforce);
                    _activeESlot.FadeInOut();
                }

            }
            else
            {
                if (SetEnforceUI(101))
                {
                    for (int i = 0; i < _items.Length; i++)
                    {
                        if (_items[i] != null)
                        {
                            if (_items[i].Data.ID == 101)
                            {
                                CountableItem asCount = _items[i] as CountableItem;
                                asCount.SetAmount(asCount.Amount - _enforce.EnforceNum[_activeSlotNum] * 2);
                                if (asCount.IsEmpty)
                                    Remove(i);

                                UpdateSlot(i);
                            }
                        }
                    }
                    ItemStat _data;
                    _data = _enforce.GetStat(_activeSlotNum);
                    UnEquPlayerStat(_data);
                    _enforce.OnButton(_activeSlotNum);
                    _data = _enforce.GetStat(_activeSlotNum);
                    EquPlayerStat(_data);
                    _activeESlot.UpdateEnforceStat(_data, _enforce.EnforceNum[_activeSlotNum]);
                    enforceUI.gameObject.SetActive(false);
                    SoundManager.Instance.EffectPlay(EffectSoundType.enforce);
                    _activeESlot.FadeInOut();
                }
            }
        }
        else
        {
            if (_activeSlotNum < 1)
            {
                SetEnforceUI(102);
            }
            else
            {
                SetEnforceUI(101);
            }
        }
    }
    public int HavePostion()
    {
        int HavePostion = 0;

        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] != null)
            {
                if (_items[i] is IUsableItem _use)
                {
                    CountableItem asCout = _items[i] as CountableItem;
                    HavePostion += asCout.Amount;
                }
            }
        }

        return HavePostion;
    }
    private bool SetEnforceUI(int _id)
    {
        int NeedObj = _enforce.EnforceNum[_activeSlotNum] * 2;
        if (GetItemSlot(_id) != null)
        {
            enforceUI.gameObject.SetActive(true);
            enforceUI.Item = GetItemSlot(_id);
            enforceUI.SetSlot(GetItemAmout(_id), NeedObj);
        }
        else
        {
            enforceUI.gameObject.SetActive(true);
            enforceUI.SetSlot();
        }
        return GetItemAmout(_id) >= NeedObj;
    }
    public Item GetItemSlot(int _id)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] != null)
            {
                if (_items[i].Data.ID == _id)
                {
                    return _items[i];
                }
            }
        }
        return null;
    }
    public int GetItemAmout(int _id)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] != null)
            {
                if (_items[i].Data.ID == _id)
                {
                    CountableItem asCout = _items[i] as CountableItem;
                    return asCout.Amount;
                }
            }
        }
        return 0;
    }

    private void Awake()
    {

        _items = new Item[_maxCapacity];
        _equ = new Item[_equCount];
        Capacity = _maxCapacity;
        ECapacity = _equCount;
        _equipmentUI.SetInventoryReference(this);
        _inventoryUI.SetInventoryReference(this);
        QuestManager.Instance.SetInventoryReference(this);
        _useSlotindex = USESTARTINDEX;
        Inven.SetActive(true);
        Inven.SetActive(false);


    }

    public int Capacity { get; private set; }
    public int ECapacity { get; private set; }

    public int Add(ItemData itemData, int amount = 1)
    {
        int index;
        UIManager.Instance.SetInvenRedDot(true);
        // 1. 수량이 있는 아이템
        if (itemData is CountableItemData ciData)
        {
            bool findNextCountable = true;
            index = -1;
            while (amount > 0)
            {
                // 1-1. 이미 해당 아이템이 인벤토리 내에 존재하고, 개수 여유 있는지 검사
                if (findNextCountable)
                {
                    index = FindCountableItemSlotIndex(ciData, index + 1);

                    // 개수 여유있는 기존재 슬롯이 더이상 없다고 판단될 경우, 빈 슬롯부터 탐색 시작
                    if (index == -1)
                    {
                        findNextCountable = false;
                    }
                    // 기존재 슬롯을 찾은 경우, 양 증가시키고 초과량 존재 시 amount에 초기화
                    else
                    {
                        CountableItem ci = _items[index] as CountableItem;
                        amount = ci.AddAmountAndGetExcess(amount);

                        if (_items[index] is PortionItem _portion)
                        {
                            portionIndex = index;
                            usePortion.SetPortion(_portion.Amount);
                        }
                        UpdateSlot(index);
                    }
                }
                // 1-2. 빈 슬롯 탐색
                else
                {
                    index = FindEmptySlotIndex(index + 1);

                    // 빈 슬롯조차 없는 경우 종료
                    if (index == -1)
                    {
                        break;
                    }
                    // 빈 슬롯 발견 시, 슬롯에 아이템 추가 및 잉여량 계산
                    else
                    {
                        // 새로운 아이템 생성
                        CountableItem ci = ciData.CreateItem() as CountableItem;
                        ci.SetAmount(amount);

                        // 슬롯에 추가
                        _items[index] = ci;

                        // 남은 개수 계산
                        amount = (amount > ciData.MaxAmount) ? (amount - ciData.MaxAmount) : 0;
                        if (_items[index] is PortionItem _portion)
                        {
                            portionIndex = index;
                            usePortion.SetPortion(_portion.Amount);
                        }
                        UpdateSlot(index);
                    }
                }
            }
        }
        // 2. 수량이 없는 아이템
        else
        {
            // 2-1. 1개만 넣는 경우, 간단히 수행
            if (amount == 1)
            {
                index = FindEmptySlotIndex();
                if (index != -1)
                {
                    // 아이템을 생성하여 슬롯에 추가
                    _items[index] = itemData.CreateItem();
                    amount = 0;

                    UpdateSlot(index);
                }
                else
                {
                    Debug.Log("err");
                }
            }

            // 2-2. 2개 이상의 수량 없는 아이템을 동시에 추가하는 경우
            index = -1;
            for (; amount > 0; amount--)
            {
                // 아이템 넣은 인덱스의 다음 인덱스부터 슬롯 탐색
                index = FindEmptySlotIndex(index + 1);

                // 다 넣지 못한 경우 루프 종료
                if (index == -1)
                {
                    break;
                }

                // 아이템을 생성하여 슬롯에 추가
                _items[index] = itemData.CreateItem();

                UpdateSlot(index);
            }
        }

        return amount;
    }
    public void ShowActiveESlot(int index)
    {
        _activeSlotNum = index;
        _activeESlot.UpdateUI(_equ[index]);
        _activeESlot.UpdateEnforceStat(_enforce.GetStat(index), _enforce.EnforceNum[index]);
        enforceUI.gameObject.SetActive(false);
    }
    public void ShowActiveISlot(int index)
    {
        _activeISlot.UpdateUI(_items[index]);
    }
    public void HideActiveESlot()
    {
        _activeESlot.ResetUI();
        enforceUI.gameObject.SetActive(false);
    }
    public void HideActiveISlot()
    {
        _activeISlot.ResetUI();
    }
    public void Use(int index)
    {
        if (!IsValidIndex(index)) return;
        if (_items[index] == null) return;

        // 사용 가능한 아이템인 경우
        if (_items[index] is IUsableItem uItem)
        {
            // 아이템 사용
            bool succeeded = uItem.Use();
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Drink);
            if (succeeded)
            {
                if (_items[index] is CountableItem ciData)
                {
                    usePortion.SetPortion(ciData.Amount);
                }
                UpdateSlot(index);
            }
        }
        else if (_items[index] is EquipmentItem _uItem)
        {

            Equip(_items[index]);
            Remove(index);
        }
    }

    public void UnEquip(int index,bool f = true)
    {
        if (!IsValidIndex(index)) return;
        if (_equ[index] == null) return;

        if (_equ[index] is EquipmentItem _uItem)
        {
            if(f)
                SoundManager.Instance.EffectPlay(EffectSoundType.equip);
            Add(_equ[index].Data);
            UnEquPlayerStat(_uItem.EquipmentData.Stat);
            EquRemove(index);
        }

    }
    public void EquPlayerStat(ItemStat _stat)
    {

        DataManager.Instance.Player.Stat.Attack += _stat.Attack;
        DataManager.Instance.Player.Stat.HitPercent += _stat.HitPercent;
        DataManager.Instance.Player.Stat.SkillDamage += _stat.SkillDamage;
        DataManager.Instance.Player.Stat.AllDamge += _stat.AllDamge;

        DataManager.Instance.Player.Stat.Defense += _stat.Defense;
        DataManager.Instance.Player.Stat.Dodge += _stat.Dodge;
        DataManager.Instance.Player.Stat.ReduceDamage += _stat.ReduceDamage;

        DataManager.Instance.Player.Stat.MaxHp += _stat.MaxHp;
        DataManager.Instance.Player.Stat.MaxMp += _stat.MaxMp;
        DataManager.Instance.Player.Stat.MaxPostion += _stat.MaxPostion;
        DataManager.Instance.Player.Stat.RecoveryHp += _stat.RecoveryHp;
        DataManager.Instance.Player.Stat.RecoveryMp += _stat.RecoveryMp;
        DataManager.Instance.Player.UpdateHpBar();

    }
    public void UnEquPlayerStat(ItemStat _stat)
    {

        DataManager.Instance.Player.Stat.Attack -= _stat.Attack;
        DataManager.Instance.Player.Stat.HitPercent -= _stat.HitPercent;
        DataManager.Instance.Player.Stat.SkillDamage -= _stat.SkillDamage;
        DataManager.Instance.Player.Stat.AllDamge -= _stat.AllDamge;

        DataManager.Instance.Player.Stat.Defense -= _stat.Defense;
        DataManager.Instance.Player.Stat.Dodge -= _stat.Dodge;
        DataManager.Instance.Player.Stat.ReduceDamage -= _stat.ReduceDamage;

        DataManager.Instance.Player.Stat.MaxHp -= _stat.MaxHp;
        DataManager.Instance.Player.Stat.MaxMp -= _stat.MaxMp;
        DataManager.Instance.Player.Stat.MaxPostion += _stat.MaxPostion;
        DataManager.Instance.Player.Stat.RecoveryHp += _stat.RecoveryHp;
        DataManager.Instance.Player.Stat.RecoveryMp += _stat.RecoveryMp;
        DataManager.Instance.Player.UpdateHpBar();

    }
    public void Equip(Item _item)
    {
        int _slotNum;
        if (_item is EquipmentItem _uItem)
        {
            SoundManager.Instance.EffectPlay(EffectSoundType.equip);
            _slotNum = _uItem.EquipmentData.EquType;
            if (!CheckEquSlot(_slotNum))
            {
                UnEquip(_slotNum,false);
            }
            _equ[_slotNum] = _item;
            EquPlayerStat(_uItem.EquipmentData.Stat);
            UpdateEqu(_slotNum);

        }

    }
    public bool CheckEquSlot(int _slotNum)
    {
        Item item = _equ[_slotNum];

        return item == null;
    }
    public void UpdateEqu(int index)
    {
        if (!IsValidIndex(index)) return;

        Item item = _equ[index];

        // 1. 아이템이 슬롯에 존재하는 경우
        if (item != null)
        {
            // 아이콘 등록
            _equipmentUI.SetItemIcon(index, item.Data.IconSprite);
        }
        // 2. 빈 슬롯인 경우 : 아이콘 제거
        else
        {
            RemoveIcon();
        }

        // 로컬 : 아이콘 제거하기
        void RemoveIcon()
        {
            _equipmentUI.RemoveItem(index);
        }
    }
    /// <summary> 해당하는 인덱스의 슬롯 상태 및 UI 갱신 </summary>
    private void UpdateSlot(int index)
    {
        if (!IsValidIndex(index)) return;

        Item item = _items[index];

        // 1. 아이템이 슬롯에 존재하는 경우
        if (item != null)
        {
            // 아이콘 등록

            _inventoryUI.SetItemIcon(index, item.Data.IconSprite);

            // 1-1. 셀 수 있는 아이템
            if (item is CountableItem ci)
            {
                // 1-1-1. 수량이 0인 경우, 아이템 제거
                if (ci.IsEmpty)
                {
                    _items[index] = null;
                    RemoveIcon();
                    return;
                }
                // 1-1-2. 수량 텍스트 표시
                else
                {
                    _inventoryUI.SetItemAmountText(index, ci.Amount);
                }
            }
            // 1-2. 셀 수 없는 아이템인 경우 수량 텍스트 제거
            else
            {
                _inventoryUI.HideItemAmountText(index);
            }

            // 슬롯 필터 상태 업데이트
            //_inventoryUI.UpdateSlotFilterState(index, item.Data);
        }
        // 2. 빈 슬롯인 경우 : 아이콘 제거
        else
        {
            RemoveIcon();
        }

        // 로컬 : 아이콘 제거하기
        void RemoveIcon()
        {
            _inventoryUI.RemoveItem(index);
            _inventoryUI.HideItemAmountText(index); // 수량 텍스트 숨기기
        }
    }
    /// <summary> 앞에서부터 개수 여유가 있는 Countable 아이템의 슬롯 인덱스 탐색 </summary>
    private int FindCountableItemSlotIndex(CountableItemData target, int startIndex = 0)
    {
        for (int i = startIndex; i < Capacity; i++)
        {
            var current = _items[i];
            if (current == null)
                continue;

            // 아이템 종류 일치, 개수 여유 확인
            if (current.Data == target && current is CountableItem ci)
            {
                if (!ci.IsMax)
                    return i;
            }
        }

        return -1;
    }
    private int FindEmptySlotIndex(int startIndex = 0)
    {
        for (int i = startIndex; i < Capacity; i++)
            if (_items[i] == null)
                return i;
        return -1;
    }
    /// <summary> 인덱스가 수용 범위 내에 있는지 검사 </summary>
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < Capacity;
    }
    /// <summary> 해당 슬롯의 아이템 제거 </summary>
    public void Remove(int index)
    {
        if (!IsValidIndex(index)) return;

        _items[index] = null;
        _inventoryUI.RemoveItem(index);
    }
    public void EquRemove(int index)
    {
        if (!IsValidIndex(index)) return;

        _equ[index] = null;
        _equipmentUI.RemoveItem(index);
    }


    public void InvenButton()
    {
        StatUI.SetActive(false);
        InvenUI.SetActive(true);
    }
    public void StatButton()
    {
        StatUI.SetActive(true);
        InvenUI.SetActive(false);
    }
    public void ExitButton()
    {
        Inven.gameObject.SetActive(false);
    }

    public void UsePortion()
    {
        Use(portionIndex);
    }
}