using System;
using System.Collections;
using System.Collections.Generic;
using Packet;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BattleController : MonoBehaviour
{
    /// <summary>
    /// 전투가 이루어지는 장소
    /// </summary>
    [SerializeField] private List<Battle> _battles = new List<Battle>();

    [SerializeField] private GameObject[] _battleObjs;

    /// <summary>
    /// 크립몬스터의 아이템 발동 인덱스 관리
    /// </summary>
    private byte _creepSlotIndex;

    /// <summary>
    /// 대결할 플레이어들 끼리 짝지어서 순서대로(선공 포함) 추가되는 리스트
    /// </summary>
    public List<Int32> BattlePlayerList = new List<Int32>();

    private void OnEnable()
    {
        Invoke("DelaySetView", 0.5f);
    }

    private void DelaySetView()
    {
        WindowManager.Instance.GetInGame().PlayersMap.SetView(PlayerManager.Instance.Players[0]);//시작 시 자신을 뷰로 설정
    }
    

    /// <summary>
    /// 플레이어들과 크립 셋팅
    /// </summary>
    public void SetCreepRound(ECreepType type)
    {
        foreach (var battle in _battleObjs)
        {
            battle.transform.localPosition = new Vector3(0, 1200, 0);
        }
        foreach (var battle in _battles)
        {
            battle.SetView(false);
        }
        
        _creepSlotIndex = 0;
        int battleIndex = 0;
        foreach (var player in PlayerManager.Instance.Players)
        {
            if (!player.IsDead)
            {
                _battles[battleIndex].SetPlayer(player.ID);
                _battles[battleIndex++].SetCreepEnemy(type, player.ID);
            }
        }
    }

    /// <summary>
    /// 플레이어들을 전부 자신 기준 왼쪽에 배치하게 끔 각각 세팅
    /// </summary>
    public void SetBattleRound()
    {
        foreach (var battle in _battleObjs)
        {
            battle.transform.localPosition = new Vector3(0, 1200, 0);
        }
        
        foreach (var battle in _battles)
        {
            battle.SetView(false);
        }
        
        //플레이어 자신과 자신의 상대 먼저 배틀 셋팅
        int myselfID = 0;
        int opponentID = 0;
        for (int i = 0; i < BattlePlayerList.Count; i++)
        {
            if (BattlePlayerList[i] == PlayerManager.Instance.Players[0].ID)
            {
                myselfID = BattlePlayerList[i];
                _battles[0].SetPlayer(myselfID);
                _battles[1].SetEnemy(myselfID);
                opponentID = (i + 1) % 2 == 0 ? BattlePlayerList[i - 1] : BattlePlayerList[i + 1]; //true 일 경우 적이 선 공격
                _battles[0].SetEnemy(opponentID);
                _battles[1].SetPlayer(opponentID);
                break;
            }
        }

        if (_battles.Count >= 3)
        {
            int battleIndex = 0; // 플레이어 순서의 인덱스
            int mapIndex = 2; //배틀 맵의 인덱스
            
            //나머지 다른 플레이어 배틀 셋팅
            while (mapIndex != _battles.Count - 1)
            {
                if (battleIndex >= BattlePlayerList.Count - 1)
                    break;
                
                if (BattlePlayerList[battleIndex] == myselfID || BattlePlayerList[battleIndex] == opponentID)
                {
                    battleIndex += 2;
                }

                if (battleIndex >= BattlePlayerList.Count - 1)
                    break;
                
                //각자 왼쪽 기준으로 셋팅
                Debug.Log(_battles.Count);
                Debug.Log(BattlePlayerList.Count);
                Debug.Log(mapIndex);
                Debug.Log(battleIndex);
                _battles[mapIndex].SetPlayer(BattlePlayerList[battleIndex]);
                _battles[mapIndex].SetEnemy(BattlePlayerList[battleIndex + 1]);
                mapIndex++;
                _battles[mapIndex].SetPlayer(BattlePlayerList[battleIndex + 1]);
                _battles[mapIndex].SetEnemy(BattlePlayerList[battleIndex]);
                mapIndex++;
                battleIndex += 2;
            }
        }
    }

    /// <summary>
    /// 주기적으로 아바타의 정보 갱신
    /// </summary>
    /// <param name="networkID"></param>
    public void UpdateAvatarInfoNetwork(sc_BattleAvatarInfoPacket avtarInfo)
    {
        foreach (var field in _battles)
        {
            if (field.BattlePlayer.NetworkID == avtarInfo.networkID)
            {
                field.BattlePlayer.UpdateAvatarInfo(avtarInfo);
            }

            if (field.BattleEnemy.NetworkID == avtarInfo.networkID)
            {
                field.BattleEnemy.UpdateAvatarInfo(avtarInfo);
            }
        }
    }

    /// <summary>
    /// ID값에 해당하는 모든 배틀 캐릭터들의 아이템을 발동시킴
    /// </summary>
    public void FindActiveItemNetwork(int networkID, byte slotIndex)
    {
        foreach (var field in _battles)
        {
            if (field.IsFinishBattle)
                continue;

            if (field.BattlePlayer.NetworkID == networkID)
            {
                field.BattlePlayer.ActiveItemNetwork(slotIndex);
            }

            if (field.BattleEnemy.NetworkID == networkID)
            {
                field.BattleEnemy.ActiveItemNetwork(slotIndex);
            }
        }
    }

    /// <summary>
    /// 크립라운드 발동
    /// </summary>
    public void FindActiveCreepItemNetwork()
    {
        foreach (var field in _battles)
        {
            if (field.IsFinishBattle)
                continue;
            
            if (_creepSlotIndex > 5)
                _creepSlotIndex = 0;
            
            field.BattleEnemy.ActiveItemNetwork(_creepSlotIndex);
        }
        _creepSlotIndex++;
    }

    public void FindUseEmotion(int networkID, EEmoticonType type)
    {
        foreach (var field in _battles)
        {
            if (field.BattlePlayer.NetworkID == networkID)
            {
                field.BattlePlayer.UseEmotion(type);
            }

            if (field.BattleEnemy.NetworkID == networkID)
            {
                field.BattleEnemy.UseEmotion(type);
            }
        }
    }

    public void FindHamburgerNetwork(int networkID,byte slotIndex, EHamburgerType type)
    {
        foreach (var field in _battles)
        {
            if (field.BattlePlayer.NetworkID == networkID)
            {
                field.BattlePlayer.FindHamburger(slotIndex, type);
            }
            
            if (field.BattleEnemy.NetworkID == networkID)
            {
                field.BattleEnemy.FindHamburger(slotIndex, type);
            }
        }
    }
    
    public void FindDoctorToolNetwork(int networkID, byte slotIndex, EItemCode type, byte upgrade)
    {
        foreach (var field in _battles)
        {
            if (field.BattlePlayer.NetworkID == networkID)
            {
                field.BattlePlayer.FindDoctorTool(slotIndex, type, upgrade);
            }
            
            if (field.BattleEnemy.NetworkID == networkID)
            {
                field.BattleEnemy.FindDoctorTool(slotIndex, type, upgrade);
            }
        }
    }
    
    /// <summary>
    /// 특정 배틀 아이템을 가진 모든 플레이어 찾아서 적용 
    /// </summary>
    public void FindBattleItem(int networkID, EItemCode code, bool isFlag)
    {
        foreach (var field in _battles)
        {
            if (field.BattlePlayer.NetworkID == networkID)
            {
                foreach (var slot in field.BattlePlayer.ItemSlots)
                {
                    if (slot.IsExistItem())
                    {
                        if (slot.GetItem().Code == code)
                        {
                            slot.GetItem().IsFlag = isFlag;
                            break;
                        }
                    }
                }
            }

            if (field.BattleEnemy.NetworkID == networkID)
            {
                foreach (var slot in field.BattleEnemy.ItemSlots)
                {
                    if (slot.IsExistItem())
                    {
                        if (slot.GetItem().Code == code)
                        {
                            slot.GetItem().IsFlag = isFlag;
                            break;
                        }
                    }
                }
            }
        }
    }
    
    

    /// <summary>
    /// 초기 배틀장소를 셋팅
    /// </summary>
    public void SetBattle()
    {
        _battles.Clear();
        for (int i = 0; i < Global.BattleMapCount; i++)
        {
            _battles.Add(_battleObjs[i].GetComponent<Battle>());
        }
    }


    /// <summary>
    /// 플레이어가 사망할때마다 조건에 만족하면 필요없는 배틀 장소를 리스트에서 지운다.
    /// </summary>
    /// <returns></returns>
    public void DeleteBattleMap()
    {
        int survivor = 0;
        foreach (var player in PlayerManager.Instance.Players)
        {
            if (!player.IsDead)
                survivor++;
        }

        int removeCount = (_battles.Count - survivor) / 2;
        removeCount *= 2; 

        for(int i = 0; i< removeCount; i++)
        {
            _battles.RemoveAt(_battles.Count - 1);
        }
    }

    /// <summary>
    /// 남은 플레이어 중 카메라에 담길 전투화면 셋팅
    /// </summary>
    public void SetBattleView(int playerID)
    {
        foreach (var battle in _battleObjs)
        {
            battle.transform.localPosition = new Vector3(0, 1200, 0);
        }

        foreach (var battle in _battles)
        {
            battle.SetView(false);
        }

        foreach (var battle in _battles)
        {
            if (battle.BattlePlayer.NetworkID == playerID)
            {
                battle.gameObject.transform.localPosition = new Vector3(-128, 0, 0);
                battle.SetView(true);
                break;
            }
        }
    }

    /// <summary>
    /// 전투가 끝났을때 모든 배틀 필드 초기화
    /// </summary>
    public void InitBattleField()
    {
        foreach (var field in _battles)
        {
            field.InitField();
        }
    }

    /// <summary>
    /// 다음 아이템 사이클 진행
    /// </summary>
    public void SetNextRound()
    {
        foreach (var field in _battles)
        {
            field.SetNextRound();
        }   
    }

}
