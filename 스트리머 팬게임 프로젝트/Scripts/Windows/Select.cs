using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Profiling.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;

public class Select : MonoBehaviour
{
    public ChoiceCharacter[] ChoiceCharacters;
    
    public TextMeshProUGUI TimerText;

    private float _pickTimer; 
    private float _pickTime;

    public Character[] Characters;
    
    
    [SerializeField] private Button _pickButton;
    
    
    private void OnEnable()
    {
        WindowManager.Instance.GetLobby().InitMatchPanel();
        Init();
    }

    /// <summary>
    /// 캐릭터 선택창 초기상태
    /// </summary>
    private void Init()
    {
        foreach (var player in PlayerManager.Instance.Players)
        {
            player.Type = ECharacterType.Empty;
        }

        foreach (var pickPanel in ChoiceCharacters)
        {
            pickPanel.Init();
        }
        SetCharacterButtons(true);
        _pickButton.gameObject.SetActive(true);
        _pickButton.interactable = false;
        
        StartCoroutine(SetSelectTimerCo());
    }

    /// <summary>
    /// 캐릭터 선택시간 설정
    /// </summary>
    public void SetTimer(float time)
    {
        _pickTime = time;
    }

    /// <summary>
    /// 캐릭터 선택이 취소되어 다시 매칭을 잡아야할때
    /// </summary>
    public void CancelSelect()
    {
        WindowManager.Instance.SetWindow(WindowType.Lobby);
        SoundManager.Instance.PlayBGM(BGMType.Lobby);
        WindowManager.Instance.GetLobby().ReTryMatch();
    }

   
    /// <summary>
    /// 8명의 플레이어가 캐릭터 선택을 완료했을때
    /// </summary>
    public void ReadyAllPlayer()
    {
        if(_pickTimer > Global.AllReadyTime)
            _pickTimer = Global.AllReadyTime;
    }
    
    
    /// <summary>
    /// 모든 플레이어를 픽창에 배치
    /// </summary>
    public void SetUserInfo(Packet.UserInfo[] userInfos)
    {
        int index = 1;
        ChoiceCharacters[0].NetworkID = PlayerManager.Instance.Players[0].ID;
        
        for (int i = 0; i < PlayerManager.PLAYER_COUNT; i++)
        {
            if (userInfos[i].networkID == ChoiceCharacters[0].NetworkID)
            {
                ChoiceCharacters[0].SetUserNickName(Encoding.Unicode.GetString(userInfos[i].name));
                continue;
            }

            ChoiceCharacters[index].SetUserNickName(Encoding.Unicode.GetString(userInfos[i].name));
            ChoiceCharacters[index++].NetworkID = (int)userInfos[i].networkID;
        }
    }
    
    /// <summary>
    /// 전체적인 플레이어들의 캐릭터 선택 현황 표시
    /// </summary>
    /// <param name="networkID"></param>
    /// <param name="characterType"></param>
    public void ChangeCharacterImage(int networkID, int characterType)
    {
        for (int i = 0; i < ChoiceCharacters.Length; i++)
        {
            if (networkID == ChoiceCharacters[i].NetworkID)
            {
                ChoiceCharacters[i].ChangeCharacterImage(characterType);
                PlayerManager.Instance.GetPlayer(networkID).Type = (ECharacterType)characterType;
                PlayerManager.Instance.GetPlayer(networkID).Sprite = ChoiceCharacters[i].PickImage.sprite;
                break;
            }
        }
    }


    /// <summary>
    /// 모든 캐릭터 초상화 버튼 활성화 여부
    /// </summary>
    public void SetCharacterButtons(bool isActive)
    {
        foreach (var btn in Characters)
        {
            btn.SetButtonSprite(isActive);
            btn.GetComponent<Button>().interactable = isActive;
        }
    }

    /// <summary>
    /// 선택한 캐릭터 버튼 외에 나머지 버튼을 활성화
    /// </summary>
    private void SetOnlyButton(int type)
    {
        foreach (var btn in Characters)
        {
            btn.SetButtonSprite(btn.CurCharType != (ECharacterType)type);
            btn.GetComponent<Button>().interactable = btn.CurCharType != (ECharacterType)type;
        }
    }

    /// <summary>
    /// 캐릭터를 직접 선택
    /// </summary>
    public void PickCharacter(int type)
    {
        SoundManager.Instance.PlayEffect(EffectType.CharacterClick);
        Player player = PlayerManager.Instance.Players[0];
       
        NetworkManager.Instance.SendChangeCharacterPacket(player.ID, type);
        player.Type = (ECharacterType)type;
        
        SetOnlyButton(type);
        _pickButton.interactable = true;
    }

    /// <summary>
    /// 캐릭터 선택 확정버튼을 눌렀을때 
    /// </summary>
    public void SetPickButton()
    {
        _pickButton.gameObject.SetActive(false);
        SetCharacterButtons(false);
        
        NetworkManager.Instance.SendChoiceCharacterPacket(PlayerManager.Instance.Players[0].ID);
        
        switch (PlayerManager.Instance.Players[0].Type)
        {
            case ECharacterType.Woowakgood:
                SoundManager.Instance.PlayEffect(EffectType.WakgoodSelect);
                break;
            case ECharacterType.Ine:
                SoundManager.Instance.PlayEffect(EffectType.IneSelect);
                break;
            case ECharacterType.Jingburger:
                SoundManager.Instance.PlayEffect(EffectType.JingburgerSelect);
                break;
            case ECharacterType.Lilpa:
                SoundManager.Instance.PlayEffect(EffectType.LilpaSelect);
                break;
            case ECharacterType.Jururu:
                SoundManager.Instance.PlayEffect(EffectType.JururuSelect);
                break;
            case ECharacterType.Gosegu:
                SoundManager.Instance.PlayEffect(EffectType.GoseguSelect);
                break;
            case ECharacterType.Viichan:
                SoundManager.Instance.PlayEffect(EffectType.ViichanSelect);
                break;
        }

    }

    /// <summary>
    /// 확정버튼 클릭 시 UI 변경
    /// </summary>
    public void SelectDone(int networkID)
    {
        foreach (var pick in ChoiceCharacters)
        {
            if (pick.NetworkID == networkID)
            {
                pick.IsReady = true;
                pick.SelectDone(true);
                break;
            }
        }
    }

    public float GetTime()
    {
        return _pickTimer;
    }
    private IEnumerator SetSelectTimerCo()
    {
        _pickTimer = _pickTime;
        bool is5Seconds = false;
        while (true)
        {
            if (_pickTimer <= 5 && !is5Seconds)
            {
                SoundManager.Instance.SetActiveBGM(); //잠시 꺼줌
                SoundManager.Instance.PlayEffect(EffectType.Seconds);
                SoundManager.Instance.PlayEffect(EffectType.SelectTimer);
                is5Seconds = true;
            }
            if (_pickTimer <= 0)
            {
                _pickTimer = 0;
                TimerText.text = $"{_pickTimer}";
                SoundManager.Instance.PlayBGM(BGMType.CutScene);
                break;
            }

            TimerText.text = $"{Mathf.Ceil(_pickTimer)}";
            _pickTimer -= Time.deltaTime;
            yield return null;
        }
        Debug.Log("게임 시작");
        InGame.IsEndGame = false;
    }

    public void SetPlayersDelay()
    {
        Invoke(nameof(SetPlayers), 1);
    }

    /// <summary>
    /// 제한시간이 지난 이후 모든 플레이어들의 캐릭터 셋팅
    /// </summary>
    public void SetPlayers()
    {
        for (var i = 0; i < PlayerManager.Instance.Players.Length; i++)
        {
            var player = PlayerManager.Instance.Players[i];
            
            player.Init(ChoiceCharacters[i].PickImage.sprite, 100);
        }
        WindowManager.Instance.GetInGame().PlayersMap.SetPlayersInfo();
    }
}
