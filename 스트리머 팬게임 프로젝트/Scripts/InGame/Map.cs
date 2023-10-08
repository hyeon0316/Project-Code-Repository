using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// 오른쪽에 표시되는 플레이어들의 정보
/// </summary>
public class Map : MonoBehaviour
{
    /// <summary>
    /// 오른쪽에 표시되는 플레이어 정보의 수, 플레이어 사망 할때마다 감소
    /// </summary>
    private int _infoCount;
    
    [SerializeField] private GameObject _playerInfoPrefab;
    [SerializeField] private GameObject _otherInfoPrefab;

    /// <summary>
    /// id값에 따라 플레이어 정보 저장
    /// </summary>
    private Dictionary<Player, PlayerInfo> _infosDic = new Dictionary<Player, PlayerInfo>();

    /// <summary>
    /// 하이어라키 상의 8개의 플레이어 뷰 중 현재 자신의 순서 
    /// </summary>
    private int _viewOrder;

    private void Update()
    {
        InputNumberView();
        InputUseEmotion();
    }

    /// <summary>
    /// 이모티콘 사용
    /// </summary>
    private void InputUseEmotion()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            NetworkManager.Instance.SendUseEmoticonPacket(PlayerManager.Instance.Players[0].ID, EEmoticonType.Woowakgood);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            NetworkManager.Instance.SendUseEmoticonPacket(PlayerManager.Instance.Players[0].ID, EEmoticonType.Ine);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            NetworkManager.Instance.SendUseEmoticonPacket(PlayerManager.Instance.Players[0].ID, EEmoticonType.Jinburger);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            NetworkManager.Instance.SendUseEmoticonPacket(PlayerManager.Instance.Players[0].ID, EEmoticonType.Lilpa);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            NetworkManager.Instance.SendUseEmoticonPacket(PlayerManager.Instance.Players[0].ID, EEmoticonType.Jururu);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            NetworkManager.Instance.SendUseEmoticonPacket(PlayerManager.Instance.Players[0].ID, EEmoticonType.Gosegu);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            NetworkManager.Instance.SendUseEmoticonPacket(PlayerManager.Instance.Players[0].ID, EEmoticonType.Viichan);
        }
    }
    /// <summary>
    /// 커멘드에 따른 관전 뷰 셋팅
    /// </summary>
    private void InputNumberView()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetView(PlayerManager.Instance.Players[0]); //자기 자신으로 설정
        }
        else if (Input.GetKeyDown(KeyCode.Q)) // 위쪽으로 순차적으로 관전
        {
            _viewOrder--;
            if (_viewOrder < 0)
                _viewOrder = _infoCount - 1;
            
            transform.GetChild(_viewOrder).GetComponent<PlayerInfo>().SetView();
        }
        else if (Input.GetKeyDown(KeyCode.E)) // 아래쪽으로 순차적으로 관전
        {
            _viewOrder++;
            if (_viewOrder > _infoCount - 1)
                _viewOrder = 0;
            
            transform.GetChild(_viewOrder).GetComponent<PlayerInfo>().SetView();
        }
    }

    public void InitPlayersInfo()
    {
        if (transform.childCount != 0)
        {
            for (int i = 0; i < Global.MaxRoomPlayer; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }

    /// <summary>
    /// 오른쪽에 표시 될 플레이어 전원의 현황판 셋팅
    /// </summary>
    public void SetPlayersInfo()
    {
        _infosDic.Clear();
        int playerInfo = Random.Range(0, 8); //플레이어(자신) 현황판의 위치 결정
        int index = 1;
        _infoCount = 0;
        for (int i = 0; i < Global.MaxRoomPlayer; i++)
        {
            GameObject obj;
            Player player;
            PlayerInfo info;
            if (playerInfo == i) //플레이어(자신) 일때
            {
                obj = Instantiate(_playerInfoPrefab, transform);
                info = obj.GetComponent<PlayerInfo>();
                player = PlayerManager.Instance.Players[0];
                _viewOrder = obj.transform.GetSiblingIndex();
            }
            else //자신 제외 적들일때
            {
                obj = Instantiate(_otherInfoPrefab, transform);
                info = obj.GetComponent<PlayerInfo>();
                player = PlayerManager.Instance.Players[index];
                info.NameText.text = $"{player.NickName}";
                index++;
            }

            info.ID = player.ID;
            info.Hp = player.Hp;
            info.HpText.text = $"{player.Hp}";
            info.CharacterImage.sprite = player.Sprite;
            info.GrayScale.gameObject.SetActive(false);
            _infosDic.Add(player, info);
            _infoCount++;
        }
    }

    /// <summary>
    /// 해당 플레이어로 관전 설정
    /// </summary>
    public void SetView(Player player)
    {
        _infosDic[player].SetView();
    }
    
        public void SetOrderView(int view)
    {
        _viewOrder = view;
    }

    public void ShowEmotion(int networkID, EEmoticonType type)
    {
        _infosDic[PlayerManager.Instance.GetPlayer(networkID)].ShowEmotion(DataManager.Instance.EmotionSprites[(int) type]);
    }

    /// <summary>
    /// 현재 관전중인 플레이어를 가르키는 오브젝트 설정
    /// </summary>
    public void SetArrow(int viewID)
    {
        foreach (var info in _infosDic)
        {
            _infosDic[info.Key].SetActiveArrow(viewID == _infosDic[info.Key].ID);
        }
    }


    /// <summary>
    /// 해당 플레이어의 현황판 체력을 갱신
    /// </summary>
    public void UpdatePlayersHp(Player player)
    {
        _infosDic[player].Hp = player.Hp;
        int hp = _infosDic[player].Hp <= 0 ? 0 : _infosDic[player].Hp;
        
        _infosDic[player].HpText.text = $"{hp}";

        SortPlayersInfo();
    }

    /// <summary>
    /// 체력 갱신 이후 전체 정보판을 위에서 아래로 체력 순으로 내림차순 정렬
    /// </summary>
    private void SortPlayersInfo()
    {
        for (int i = 0; i < _infoCount; i++)
        {
            for (int j = i + 1; j < _infoCount; j++)
            {
                PlayerInfo playerInfo1 = transform.GetChild(i).GetComponent<PlayerInfo>();
                PlayerInfo playerInfo2 = transform.GetChild(j).GetComponent<PlayerInfo>();
                if (playerInfo1.Hp < playerInfo2.Hp)
                {
                    playerInfo2.transform.SetSiblingIndex(playerInfo1.transform.GetSiblingIndex());
                }
            }
        }
    }

    /// <summary>
    /// 사망 시 회색 표시
    /// </summary>
    public void MapDead(Player player)
    {
        if (player.IsDead)
        {
            _infosDic[player].GrayScale.gameObject.SetActive(true);
        }
    }
}
