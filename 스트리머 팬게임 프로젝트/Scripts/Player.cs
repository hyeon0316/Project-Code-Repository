using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    /// <summary>
    /// 선택한 캐릭터 타입
    /// </summary>
    public ECharacterType Type { get; set; }

    private int _maxHp;
    private int _hp;

    public string NickName { get; private set; }

    private Sprite _sprite;

    /// <summary>
    /// 자신 캐릭터 스폰 위치
    /// </summary>
    [SerializeField] private Transform _spawnPos;
    private CharacterController _characterController;
    [SerializeField] private RouletteController _rouletteController;
    public RouletteController RouletteController => _rouletteController;

    /// <summary>
    /// 선공력
    /// </summary>
    public int FirstAttack { get; set; } 
    
    public bool IsDead { get; set; }

    public Sprite Sprite
    {
        get { return _sprite; }
        set { _sprite = value; }
    }

    [SerializeField] private int _id;

    public int ID => _id;

    public int Hp
    {
        get { return _hp; }
        set { _hp = value; }
    }

    public int Defense { get; set; }

    [SerializeField] private TextMeshProUGUI _avatarHpText;
    [SerializeField] private Slider _avtarHpSlider;
    [SerializeField] private TextMeshProUGUI _defenseText;
    [SerializeField] private TextMeshProUGUI _firstAttackText;
    
    public int MaxAvatarHp { get; set; }
    
    /// <summary>
    /// 라운드별 아바타 최대체력
    /// </summary>
    public int RoundMaxAvatarHp { get; set; }
    
    /// <summary>
    /// 전투에서 사용될 hp
    /// </summary>
    public int AvatarHp { get; set; }

    public UsingInventory UsingInventory;
    public UnUsingInventory UnUsingInventory;

    /// <summary>
    /// 플레이어가 전투 중에 끊어졌는지 확인
    /// </summary>
    private bool _isDisconnectBattle;
    
    /// <summary>
    /// 플레이어가 끊어졌는지 체크
    /// </summary>
    public bool IsDisconnect { get; set; }


    public void Init(Sprite sprite, int maxHp)
    {
        DestroyAvatar();
        GameObject obj = Instantiate(DataManager.Instance.CharacterPrefabs[(int)Type], _spawnPos);
        _characterController = obj.GetComponent<CharacterController>();
        _characterController.SetNickName(NickName);
        _sprite = sprite;
        _maxHp = maxHp;
        _hp = _maxHp;
        InitAvatarStat(50);
        IsDead = false;
        IsDisconnect = false;
    }


    /// <summary>
    /// Using 인벤에 아이템을 셋팅할떄마다 장착효과 업데이트
    /// </summary>
    public void UpdateEquipEffect()
    {
        InitAvatarStat(RoundMaxAvatarHp);
        foreach (var slot in UsingInventory.ItemSlots)
        {
            if (slot.transform.childCount == 1)
            {
                slot.GetItem().SetEquipEffect(_id);
            }
            else if (slot.transform.childCount == 2)
            {
                slot.OtherInSlotItem().SetEquipEffect(_id);
            }
        }
    }

    /// <summary>
    /// 아바타 초기화
    /// </summary>
    private void DestroyAvatar()
    {
        if(_spawnPos.childCount == 1)
            Destroy(_characterController.gameObject);
    }

    /// <summary>
    /// 아바타 스탯 초기화
    /// </summary>
    private void InitAvatarStat(int maxHp)
    {
        FirstAttack = 0;
        Defense = 0;
        MaxAvatarHp = maxHp;
        AvatarHp = MaxAvatarHp;
        _defenseText.text = $"{Defense}";
        _avatarHpText.text = $"{AvatarHp}/{MaxAvatarHp}";
        _firstAttackText.text = $"{FirstAttack}";
    }

    /// <summary>
    /// 아바타 최대 체력 갱신
    /// 50 60 70 80 90 100(라운드 별 체력 증가율)
    /// </summary>
    public void UpdateAvtarHp(int amount)
    {
        MaxAvatarHp += amount;
        _avtarHpSlider.maxValue = MaxAvatarHp;
        _avtarHpSlider.value = MaxAvatarHp;
        AvatarHp = MaxAvatarHp;
        _avatarHpText.text = $"{AvatarHp}/{MaxAvatarHp}";
    }

    /// <summary>
    /// 방어도 갱신
    /// </summary>
    /// <param name="amount"></param>
    public void UpdateDefense(int amount)
    {
        Defense += amount;
        _defenseText.text = $"{Defense}";
    }

    /// <summary>
    /// 선공력 갱신
    /// </summary>
    public void UpdateFirstAttack(int amount)
    {
        FirstAttack += amount;
        _firstAttackText.text = $"{FirstAttack}";
    }
    
    public void SetName(string name)
    {
        NickName = name;
    }

    public void SetID(int id)
    {
        _id = id;
    }

    /// <summary>
    /// 뽑기권 수 지정
    /// </summary>
    public void SetRouletteCountNetwork(RouletteType type, int count)
    {
        _rouletteController.SetRouletteCount(type, count);
    }

    public void ResetRoulettePanel()
    {
        _rouletteController.ResetPanel();
    }

    /// <summary>
    /// 데미지 또는 회복이후 체력 갱신
    /// </summary>
    public void UpdateHp(int amount)
    {
        _hp += amount;

        WindowManager.Instance.GetInGame().PlayersMap.UpdatePlayersHp(this); //플레이어 현황판 체력 갱신
        if (_hp <= 0)
        {
            Die();
        }
    }

    public void PlayAnim(AnimType type)
    {
        _characterController.PlayAnim(type);
    }
    

    private void Die()
    {
        IsDead = true;
        WindowManager.Instance.GetInGame().PlayersMap.MapDead(this);
        
        _characterController.PlayAnim(AnimType.Defeat);
        
        if (InGame.CurGameType == EGameType.Ready)
        {
            WindowManager.Instance.GetInGame().BattleWindow.DeleteBattleMap();
        }
    }
    

    public void AddNewItemNetwork(int slotIndex, EItemCode code)
    {
        if (slotIndex < 6)
        {
            UsingInventory.ItemSlots[slotIndex].AddNewItem(code);
        }
        else
        {
            UnUsingInventory.ItemSlots[slotIndex - 6].AddNewItem(code);
        }
        _rouletteController.CheckUseRoulette();
        UpdateEquipEffect();
    }
    
    /// <summary>
    /// 준비단계에서 셋팅한 아이템을 전투단계에 사용
    /// </summary>
    public void SetBattleSlot(ItemSlot[] usingSlots)
    {

        int[] slotIndexOrder = new int [6]{ 0, 2, 4, 1, 3, 5 };
        for (int i = 0; i < UsingInventory.ItemSlots.Length; i++)
        {
            if (UsingInventory.ItemSlots[slotIndexOrder[i]].transform.childCount == 1)
            {
                Item addItem = UsingInventory.ItemSlots[slotIndexOrder[i]].GetItem();
                usingSlots[i].AddNewItem(addItem.Code, addItem.Upgrade);
            }
        }
    }

    /// <summary>
    /// 이모티콘 사용
    /// </summary>
    public void UseEmotionNetwork(EEmoticonType type)
    {
        if (InGame.CurGameType == EGameType.Ready) //준비중일떄
        {
            _characterController.UseEmotion(DataManager.Instance.EmotionSprites[(int) type]);
        }
        else //전투중일때
        {
            WindowManager.Instance.GetInGame().BattleWindow.FindUseEmotion(_id, type);
        }
    }

    public void DelayCheckRoulette()
    {
        Invoke("CheckRoulette",0.1f);
    }

    private void CheckRoulette()
    {
        _rouletteController.CheckUseRoulette();
    }

    /// <summary>
    /// 인덱스에 따라 Using과 UnUsing의 슬롯 구분
    /// </summary>
    public ItemSlot GetItemSlot(byte slotIndex)
    {
        ItemSlot itemSlot = slotIndex < 6
            ? UsingInventory.ItemSlots[slotIndex]
            : UnUsingInventory.ItemSlots[slotIndex - 6];

        return itemSlot;
    }

    /// <summary>
    /// 아이템 자리 변경
    /// </summary>
    public void SwapItemNetwork(int slotIndex1, int slotIndex2)
    {
        ItemSlot itemSlot1 = slotIndex1 < 6 ? UsingInventory.ItemSlots[slotIndex1] : UnUsingInventory.ItemSlots[slotIndex1 - 6];
        ItemSlot itemSlot2 = slotIndex2 < 6 ? UsingInventory.ItemSlots[slotIndex2] : UnUsingInventory.ItemSlots[slotIndex2 - 6];

        if (itemSlot2.IsExistItem()) //옮긴 자리에 아이템이 존재할 경우
        {
            Item tempItem2 = itemSlot2.GetItem();
            tempItem2.RePosItem(itemSlot1.transform, Vector3.zero);
            tempItem2.transform.localPosition = Vector3.zero;
        }

        Item tempItem1 = itemSlot1.GetItem(); //드래그 하여 옮긴 아이템
        tempItem1.RePosItem(itemSlot2.transform, Vector3.zero);
        tempItem1.transform.localPosition = Vector3.zero;
        tempItem1.SetAlpha(1);
        
        UpdateEquipEffect();
        CheckRoulette();
    }
    
    
}