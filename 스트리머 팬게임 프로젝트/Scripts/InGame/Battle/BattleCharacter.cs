using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Packet;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public abstract class BattleCharacter : MonoBehaviour
{
    [SerializeField] protected Battle _myFiled;
    
    protected bool _isGhost;

    [Header("자신의 캐릭터 위치")]
    [SerializeField] protected Transform _characterPos;

    [SerializeField] private StatusUI _statusUI;
    [SerializeField] private ItemBuffUIController _itemBuffUI;
    
    [SerializeField] protected TextMeshProUGUI _avatarHpText;
    [SerializeField] protected TextMeshProUGUI _avatarDefenseText;
    [SerializeField] protected TextMeshProUGUI _avtarFirstAttackText;
    [SerializeField] private Slider _avtarHpSlider;
    
    public ItemSlot[] ItemSlots;
    
    private int _avatarMaxHp;
    
    protected CharacterController _characterController;

    /// <summary>
    /// 플레이어의 아바타 체력
    /// </summary>
    private int _avatarHp;

    private UInt32 _avatarDefense;

    public UInt32 AvatarDefense
    {
        get { return _avatarDefense; }
        set { _avatarDefense = value; }
    }

    /// <summary>
    /// 상대에 대한 정보를 담아두는 변수
    /// </summary>
    public BattleCharacter Opponent;
    public int NetworkID { get; protected set; }

    /// <summary>
    /// 해당 캐릭터가 크립몬스터인지
    /// </summary>
    protected bool _isCreep;

    /// <summary>
    /// 현재 이 캐릭터를 보고 있는지
    /// </summary>
    public bool IsView { get; set; }

    private bool _isBombSoundOn;

    [SerializeField] private FloatingText _floatingText;

    /// <summary>
    /// 데미지를 받기 전 아바타체력
    /// </summary>
    private int _previousAvatarHp;
    
    public void SetBattleCharacter(int networkID)
    {
        _statusUI.Init();
        _itemBuffUI.Init();
        _isCreep = false;
        _previousAvatarHp = 0;
        NetworkID = networkID;
        Player player = PlayerManager.Instance.GetPlayer(networkID);
        GameObject obj = Instantiate(DataManager.Instance.CharacterPrefabs[(int) player.Type], _characterPos);
        _characterController = obj.GetComponent<CharacterController>();
        _characterController.SetNickName(player.NickName);
        player.SetBattleSlot(ItemSlots);
        
        SetGhost();
        Invoke(nameof(CheckDead), 1);
    }
    
    /// <summary>
    /// 아이템 발동
    /// </summary>
    /// <param name="slotIndex">발동할 슬롯 인덱스</param>
    public void ActiveItemNetwork(byte slotIndex)
    {
        int[] slotIndexOrder = new int[6] { 0, 3, 1, 4, 2, 5 };
        int activeIndex = slotIndexOrder[slotIndex];
        if (ItemSlots[activeIndex].IsExistItem())
        {
            ItemSlots[activeIndex].ActiveItem(this, Opponent);
            ItemSlots[activeIndex].SetAlphaSlot(0);//임시
            ItemSlots[activeIndex].SetAlpha(0); //임시
        }
        else //빈슬롯 발동일 경우
        {
            ItemSlots[activeIndex].ActiveItem(this, Opponent);
            ItemSlots[activeIndex].SetAlphaSlot(0);//임시
        }


        if (_statusUI.IsUIActive(StatusType.Blooding))
        {
            Blooding();
        }
    }

    /// <summary>
    /// 아이템 사용전 사용후 캐릭터 정보 갱신
    /// </summary>
    public void UpdateAvatarInfo(sc_BattleAvatarInfoPacket avatarInfo)
    {
        StartCoroutine(UpdateAvatarInfoCo(avatarInfo));
    }

    private IEnumerator UpdateAvatarInfoCo(sc_BattleAvatarInfoPacket avatarInfo)
    {
        yield return new WaitForSeconds(0.5f);
        _avtarFirstAttackText.text = $"{avatarInfo.firstAttackState}";
        UpdateAvatarHp(avatarInfo.maxHp, avatarInfo.hp);
        UpdateAvatarDefense(avatarInfo.defensive);
        UpdateStatusUI(avatarInfo);
        UpdateItemBuffUI(avatarInfo);
        if (_previousAvatarHp > 0)
        {
            int damage = avatarInfo.hp - _previousAvatarHp;
            if(IsView)
                _floatingText.ShowFloating(damage);
        }
        _previousAvatarHp = avatarInfo.hp;
        if (!_isCreep)
        {
            //실제 체력도 갱신
            if (PlayerManager.Instance.GetPlayer(avatarInfo.networkID) != null)
            {
                if (!PlayerManager.Instance.GetPlayer(avatarInfo.networkID).IsDisconnect)
                {
                    PlayerManager.Instance.GetPlayer(avatarInfo.networkID).Hp = avatarInfo.playerHp;
                    WindowManager.Instance.GetInGame().PlayersMap.UpdatePlayersHp(PlayerManager.Instance.GetPlayer(avatarInfo.networkID));
                }
            }
        }
    }

    private void UpdateStatusUI(sc_BattleAvatarInfoPacket avatarInfo)
    {
        _statusUI.SetBuffUI(StatusType.AttackBuff, avatarInfo.offensePower);
        _statusUI.SetBuffUI(StatusType.PlusDefense, avatarInfo.additionDefensive);
        _statusUI.SetBuffUI(StatusType.Weaken, avatarInfo.weakening);
        _statusUI.SetBuffUI(StatusType.Blooding, avatarInfo.bleeding);
        _statusUI.SetBuffUI(StatusType.HealWeaken, avatarInfo.reducedHealing);
    }

    private void UpdateItemBuffUI(sc_BattleAvatarInfoPacket avatarInfo)
    {
        _itemBuffUI.SetItemBuffUI(BuffType.FishBread, avatarInfo.isEffectHeal, avatarInfo.effectHeal);

        if (avatarInfo.isInstallBomb)
        {
            _isBombSoundOn = true;
        }

        if (_isBombSoundOn)
        {
            if (!avatarInfo.isInstallBomb)
            {
                ExplodeBomb();
                _isBombSoundOn = false;
            }
        }
        _itemBuffUI.SetItemBuffUI(BuffType.SisterHeart, avatarInfo.isInstallBomb, avatarInfo.installBombDamage);
        
        _itemBuffUI.SetItemBuffUI(BuffType.Broom, avatarInfo.isIgnoreNextDamage);
        _itemBuffUI.SetItemBuffUI(BuffType.HeroCloak, avatarInfo.canDefendNegativeEffect);
        _itemBuffUI.SetItemBuffUI(BuffType.MaidCostume, avatarInfo.isCounterAttack, avatarInfo.counterAttackDamage);
        _itemBuffUI.SetItemBuffUI(BuffType.NunDress, avatarInfo.isCounterHeal, avatarInfo.counterHeal);
    }
    
    /// <summary>
    /// 유령이면 실제 데미지 계산 제외
    /// </summary>
    public void IsGhost(bool isGhost)
    {
        _isGhost = isGhost;
    }

    /// <summary>
    /// 체력 갱신
    /// </summary>
    private void UpdateAvatarHp(int maxHp, int hp)
    {
        _avatarMaxHp = maxHp;
        _avatarHp = hp;
        _avtarHpSlider.maxValue = _avatarMaxHp;
        _avtarHpSlider.value = _avatarHp;
        _avatarHpText.text = $"{_avatarHp}/{_avatarMaxHp}";

        if (_avatarHp <= 0)
        {
            DeadAvatar();
        }
    }

    /// <summary>
    /// 방어도 갱신
    /// </summary>
    /// <param name="value"></param>
    private void UpdateAvatarDefense(UInt32 value)
    {
        _avatarDefense = value;
        _avatarDefenseText.text = $"{value}";
    }

    public void FindHamburger(byte slotIndex, EHamburgerType type)
    {
        ItemSlots[slotIndex].GetSpecificItem<Hamburger>().SelectBurger(type);
    }

    public void FindDoctorTool(byte slotIndex, EItemCode code, byte upgrade)
    {
        int[] slotIndexOrder = new int[6] { 0, 3, 1, 4, 2, 5 };
        int activeIndex = slotIndexOrder[slotIndex];

        ItemSlots[activeIndex].DeleteItem();
        ItemSlots[activeIndex].AddNewItem(code, upgrade);
    }
    
    /// <summary>
    /// 유령일 경우 다시 부호 반전
    /// </summary>
    private void SetGhost()
    {
        if (_isGhost)
            NetworkID = ~NetworkID;
    }

    private void CheckDead()
    {
        if (PlayerManager.Instance.GetPlayer(NetworkID) != null)
        {
            if (PlayerManager.Instance.GetPlayer(NetworkID).IsDead)
            {
                UpdateAvatarHp(100,-999);
            }
        }
    }


    public void UseEmotion(EEmoticonType type)
    {
        if(_characterController != null)
            _characterController.UseEmotion(DataManager.Instance.EmotionSprites[(int) type]);
    }
    
    
    /// <summary>
    /// 한 사이클 마다 초기화
    /// </summary>
    public void InitCycle()
    {
        _itemBuffUI.Init();
        _statusUI.CloseBuffUI(StatusType.Weaken);
        _statusUI.CloseBuffUI(StatusType.AttackBuff);
        _statusUI.CloseBuffUI(StatusType.HealWeaken);
        _statusUI.CloseBuffUI(StatusType.PlusDefense);
    }


    /// <summary>
    /// 뽑기권을 얻었는지 확인 하여 이펙트 발생
    /// </summary>
    public void CheckGetTicket(bool isSuccess)
    {
        if (isSuccess)
        {
            //todo: 성공 이펙트
            if(IsView)
                SoundManager.Instance.PlayEffect(EffectType.CounselLicense);
        }
        else
        {
            //todo: 실패 이펙트
        }
    }
    

    /// <summary>
    /// 출혈
    /// </summary>
    private void Blooding()
    {
        if(IsView)
            SoundManager.Instance.PlayEffect(EffectType.Blood);
        
        StartCoroutine(BloodEffectCo());
    }

    /// <summary>
    /// 치유력 감소
    /// </summary>
    public void WeakenHeal(int value)
    {
        //todo: 치유력 감소 이펙트
        if (_itemBuffUI.IsActive(BuffType.HeroCloak))
        {
            if(IsView)
                SoundManager.Instance.PlayEffect(EffectType.Dodge);
        }
    }

    /// <summary>
    /// 추가방어력 획득
    /// </summary>
    public void GetDefensePower()
    {
        //todo: 추가 방어력 상승 효과 이펙트 연출
    }

    public void RemoveDeBuff()
    {
        //todo: 디버프 제거 이펙트
    }

    /// <summary>
    /// 자신에거 폭탄을 설치
    /// </summary>
    public void PlantBomb()
    {
        //todo: 폭탄 설치 연출
        if (!_itemBuffUI.IsActive(BuffType.HeroCloak))
        {
            if(IsView)
                SoundManager.Instance.PlayEffect(EffectType.Dodge);
        }
    }

    /// <summary>
    /// 폭탄이 폭발되어 데미지를 입음
    /// </summary>
    private void ExplodeBomb()
    {
        //todo: 폭발 연출
        if(IsView)
            SoundManager.Instance.PlayEffect(EffectType.Bomb);
    }
    
    
    
    /// <summary>
    /// 공격을 무효화 시키는 효과 셋팅
    /// </summary>
    public void SetDenyAttack()
    {
        //todo: 공격 무효화 셋팅 이펙트
    }

    public void Weaken()
    {
        if (!_itemBuffUI.IsActive(BuffType.HeroCloak))
        {
            if (IsView)
            {
                SoundManager.Instance.PlayEffect(EffectType.Weaken);
            }
        }
    }


    /// <summary>
    /// 아바타 체력 회복
    /// </summary>
    public void HealAvatar()
    {
        //todo: 회복 이펙트
        if (IsView)
            SoundManager.Instance.PlayEffect(EffectType.Heal);
    }

    /// <summary>
    /// 방어도를 증가시킬때
    /// </summary>
    public void AddDefense()
    {
        //todo: 방어도 증가 이펙트
        if(IsView)
            SoundManager.Instance.PlayEffect(EffectType.Defense);
    }

    public void AttackEnemy()
    {
        StartCoroutine(AttackEnemyCo());
    }

    private IEnumerator AttackEnemyCo()
    {
        PlayAnim(AnimType.Use);

        yield return new WaitForSeconds(0.5f);
        Opponent.TakeAvatarDamage();
    }

    
    /// <summary>
    /// 아바타가 데미지를 받을때
    /// </summary>
    public void TakeAvatarDamage()
    {
        if (_itemBuffUI.IsActive(BuffType.Broom))
        {
            //todo: 공격 무시 연출
            if(IsView)
                SoundManager.Instance.PlayEffect(EffectType.Dodge);
            
            return;
        }
        
        PlayAnim(AnimType.Hit);
    }


    public void PlayAnim(AnimType animType)
    {
        if (animType == AnimType.Victory)
        {
            if (_isCreep)
                return;
        }

        if (_characterController != null)
        {
            _characterController.PlayAnim(animType);
        }
    }

    public void SetView(bool isView)
    {
        if (_characterController != null)
        {
            _characterController.SetView(isView);
            IsView = isView;
        }
    }
    

    /// <summary>
    /// 아바타의 체력이 0이하가 되어 사망했을때
    /// </summary>
    private void DeadAvatar()
    {
        if (!_isCreep)
        {
            if(_characterController != null)
                PlayAnim(AnimType.Defeat);
        }
        else
        {
            if(_characterController != null)
                _characterController.SetAlphaTransition();
        }
        
        _myFiled.FinishBattle();
        Opponent.PlayAnim(AnimType.Victory);
    }

    /// <summary>
    /// 배틀 종료후 초기화시 아바타 제거
    /// </summary>
    public void DestroyAvatar()
    {
        if(_characterPos.childCount == 1) 
            Destroy(_characterController.gameObject);
    }

    private IEnumerator BloodEffectCo()
    {
        WaitForSeconds delay = new WaitForSeconds(0.5f);
        
        yield return delay;
        _characterController.SetImageColor(Color.red);
        yield return delay;
        _characterController.SetImageColor(Color.white);
    }
    

}
