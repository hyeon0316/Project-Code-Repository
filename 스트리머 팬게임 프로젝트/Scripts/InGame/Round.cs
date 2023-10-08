using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum RoundType
{
    /// <summary>
    /// 현재 라운드
    /// </summary>
    StartRound,
    
    /// <summary>
    /// 다음 라운드
    /// </summary>
    NextRound,
    /// <summary>
    /// 이전 라운드
    /// </summary>
    PreRound, 
    
}

public enum BattleType
{
    Battle,
    Creep,
}
public class Round : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite[] _creepSprites;
    [SerializeField] private Sprite[] _battleSprites;
    [SerializeField] private Sprite _emptySprite;
    [SerializeField] private TextMeshProUGUI _curRoundText;
    
    /// <summary>
    /// 현재 라운드 수 
    /// </summary>
    private int _curRoundCount;
    
    private int _roundImageCount;

    private int _curRoundIndex;
    private int _entireRoundIndex;

    [SerializeField] private Image[] _roundImages;

    /// <summary>
    /// 첫 라운드
    /// </summary>
    [SerializeField] private List<BattleType> _firstRounds;
    
    /// <summary>
    /// 반복 라운드
    /// </summary>
    [SerializeField] private List<BattleType> _RepeatRounds;

    public void Init()
    {
        _entireRoundIndex = 0;
        _curRoundIndex = 0;
        _curRoundCount = -1;
        _curRoundCount++;
        SetRoundText(_curRoundCount);
    }

    /// <summary>
    /// 라운드 진행
    /// </summary>
    public void ProceedRound()
    {
        if (_entireRoundIndex < 3) //첫 라운드
        {
            SetRound(_firstRounds);
            _curRoundIndex++;
            if (_curRoundIndex >= 3)
            {
                _curRoundIndex = 0;
            }
        }
        else // 반복 라운드
        {
            if (_curRoundIndex == 0)
            {
                _curRoundCount++;
                SetRoundText(_curRoundCount);
            }
            
            SetRound(_RepeatRounds);
            _curRoundIndex++;
            if (_curRoundIndex >= _roundImages.Length)
            {
                _curRoundIndex = 0;
            }
        }
        _entireRoundIndex++;
    }
    

    private void SetRound(List<BattleType> roundList)
    {
        InitSprite();

        int nextRound = _curRoundIndex;
        int preRound = _curRoundIndex;
        
        _roundImages[_curRoundIndex].sprite = roundList[_curRoundIndex] == BattleType.Creep ? _creepSprites[(int) RoundType.StartRound] 
            : _battleSprites[(int) RoundType.StartRound];
        
        while (true) //현재 라운드 기준 다음라운드들 셋팅
        {
            nextRound++;
            if (nextRound > roundList.Count - 1)
                break;
            
            _roundImages[nextRound].sprite = roundList[nextRound] == BattleType.Creep ? _creepSprites[(int) RoundType.NextRound] 
                : _battleSprites[(int) RoundType.NextRound];
        }
        
        while (true) //현재 라운드 기준 이전 라운드들 셋팅
        {
            preRound--;
            if(preRound < 0)
                break;
            
            _roundImages[preRound].sprite = roundList[preRound] == BattleType.Creep ? _creepSprites[(int) RoundType.PreRound] 
                : _battleSprites[(int) RoundType.PreRound];
        }
    }

    
    private void InitSprite()
    {
        foreach (var image in _roundImages)
        {
            image.sprite = _emptySprite;
        }
    }
    

    /// <summary>
    /// 라운드 현황 설정
    /// </summary>
    private void SetRoundText(int round)
    {
        _curRoundCount = round;
        _curRoundText.text = $"{round}라운드";

        switch (round)
        {
            case 0:
                PlayerManager.Instance.PlayersUpdateAvatarHp(50);
                PlayerManager.Instance.Players[0].RouletteController.PercentNormalText.text = "100/0/0/0/0";
                break;
            case 1:
                PlayerManager.Instance.PlayersUpdateAvatarHp(60);
                PlayerManager.Instance.Players[0].RouletteController.PercentNormalText.text = "75/25/0/0/0";
                break;
            case 2:
                PlayerManager.Instance.PlayersUpdateAvatarHp(70);
                PlayerManager.Instance.Players[0].RouletteController.PercentNormalText.text = "55/30/15/0/0";
                break;
            case 3:
                PlayerManager.Instance.PlayersUpdateAvatarHp(80);
                PlayerManager.Instance.Players[0].RouletteController.PercentNormalText.text = "25/40/30/5/0";
                break;
            case 4:
                PlayerManager.Instance.PlayersUpdateAvatarHp(90);
                PlayerManager.Instance.Players[0].RouletteController.PercentNormalText.text = "16/20/35/25/4";
                break;
            case 5:
                PlayerManager.Instance.PlayersUpdateAvatarHp(100);
                PlayerManager.Instance.Players[0].RouletteController.PercentNormalText.text = "9/15/36/30/10";
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolTipManager.Instance.ShowRoundPanel();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instance.CloseRound();
    }
}
