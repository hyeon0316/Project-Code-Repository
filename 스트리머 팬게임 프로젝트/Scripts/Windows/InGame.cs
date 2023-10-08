using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 아이템 드래그 관련 이벤트를 Ready창과 Battle창에 따라 나누기 위함
/// </summary>
public enum EGameType
{
    Ready,
    Battle,
}

public class InGame : MonoBehaviour
{
    public static bool IsEndGame;
    public Ready ReadyWindow;
    public BattleController BattleWindow;

    public static EGameType CurGameType = EGameType.Ready;

    public Map PlayersMap;

    public Round Round;

    /// <summary>
    /// 승리or패배시 보여질 UI 
    /// </summary>
    public GameOver GameOverController;

    private void OnEnable()
    {
        GameOverController.Init();
    }

    /// <summary>
    /// 준비단계에서 전투단계로 이동
    /// </summary>
    public void OpenBattle()
    {
        SoundManager.Instance.PlayBGM(BGMType.Battle);
        ToolTipManager.Instance.CloseToolTip();
        ToolTipManager.Instance.CloseBuffDeBuff();
        BattleWindow.gameObject.SetActive(true);
        ReadyWindow.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 전투 종료 후 다시 준비단계로 이동
    /// </summary>
    public void OpenReady()
    {
        SoundManager.Instance.PlayBGM(BGMType.Ready);
        ToolTipManager.Instance.CloseToolTip();
        ToolTipManager.Instance.CloseBuffDeBuff();
        CurGameType = EGameType.Ready;
        SelectWinner();
        BattleWindow.gameObject.SetActive(false);
        ReadyWindow.gameObject.SetActive(true);
        CheckDeadPlayer();
    }

    /// <summary>
    /// 죽은 플레이어는 데드 애니메이션 실행
    /// </summary>
    private void CheckDeadPlayer()
    {
        foreach (var player in PlayerManager.Instance.Players)
        {
            if(player.IsDead)
                player.PlayAnim(AnimType.Defeat);
        }
    }

    /// <summary>
    /// 게임의 최종 승자를 결정
    /// </summary>
    public void SelectWinner()
    {
        int deadCount = 0;
        foreach (var player in PlayerManager.Instance.Players)
        {
            if (player.IsDead)
            {
                deadCount += 1;
            }
        }
        
        Debug.Log($"죽은사람{deadCount}");
        if (deadCount == 7)
        {
            Debug.Log("게임 승리");
            GameOverController.ShowGameOver(GameOverType.Win);
            IsEndGame = true;
            Debug.Log($"게임 끝난냐?{IsEndGame}");
        }
    }
}
