using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Battle : MonoBehaviour
{
    
    public BattlePlayer BattlePlayer;
    public BattleEnemy BattleEnemy;
    
    /// <summary>
    /// 필드의 전투가 끝났을때 끝났다는 표시 추가
    /// </summary>
    [SerializeField] private GameObject _finishImage;

    /// <summary>
    /// 전투가 끝났는지
    /// </summary>
    public bool IsFinishBattle { get; set; }
    
    
    /// <summary>
    /// 배틀종료시 기능 초기화
    /// </summary>
    public void InitField()
    {
        foreach (var itemSlot in BattlePlayer.ItemSlots)
        {
            if (itemSlot.transform.childCount == 1)
                itemSlot.DeleteItem();
            
            itemSlot.SetAlphaSlot(1);
        }

        foreach (var itemSlot in BattleEnemy.ItemSlots)
        {
            if (itemSlot.transform.childCount == 1)
                itemSlot.DeleteItem();
            
            itemSlot.SetAlphaSlot(1);
        }

        _finishImage.SetActive(false);
        
        BattlePlayer.DestroyAvatar();
        BattleEnemy.DestroyAvatar();
        IsFinishBattle = false;
    }

    public void SetView(bool isView)
    {
        BattlePlayer.SetView(isView);
        BattleEnemy.SetView(isView);
    }
    

    /// <summary>
    /// 한쪽 배틀캐릭터가 죽었을때
    /// </summary>
    public void FinishBattle()
    {
        //todo: 전체적으로 현황판 체력 갱신
        _finishImage.SetActive(true);
        IsFinishBattle = true;
    }
    
    


    /// <summary>
    ///  다음 아이템 순회 발동에 대한 준비
    /// </summary>
    public void SetNextRound()
    {
        foreach (var itemSlot in BattlePlayer.ItemSlots)
        {
            itemSlot.SetAlphaSlot(1);
            if (itemSlot.transform.childCount == 1)
            {
                itemSlot.SetAlpha(1);
            }
        }

        foreach (var itemSlot in BattleEnemy.ItemSlots)
        {
            itemSlot.SetAlphaSlot(1);
            if (itemSlot.transform.childCount == 1)
            {
                itemSlot.SetAlpha(1);
            }
        }
        
        BattlePlayer.InitCycle();
        BattleEnemy.InitCycle();
    }

    /// <summary>
    /// 왼쪽 플레이어 셋팅
    /// </summary>
    /// <param name="playerID">플레이어의 ID</param>
    public void SetPlayer(int playerID)
    {
        bool isGhost = false; //왼쪽 플레이어도 유령으로 셋팅 되어있는지 확인하여 유령이면 실제 데미지 계산에 제외
        
        if (playerID < 0)
        {
            playerID = ~playerID;
            isGhost = true;
        }

        BattlePlayer.IsGhost(isGhost);
        BattlePlayer.SetBattleCharacter(playerID);
    }

    /// <summary>
    /// 오른쪽 유령 플레이어 셋팅
    /// </summary>
    /// <param name="enemyID">플레이어의 ID</param>
    public void SetEnemy(int enemyID)
    {
        bool isGhost = false;
        if (enemyID < 0) // 유령일때
        {
            enemyID = ~enemyID;
            isGhost = true;
        }

        BattleEnemy.IsGhost(isGhost);
        BattleEnemy.SetBattleCharacter(enemyID);
        BattleEnemy.ReverseImage();
    }

    /// <summary>
    /// 오른쪽 크립 몬스터 셋팅
    /// </summary>
    public void SetCreepEnemy(ECreepType type, int opponentID)
    {
        BattleEnemy.SetCreepEnemy(type, opponentID);
    }
    
}
