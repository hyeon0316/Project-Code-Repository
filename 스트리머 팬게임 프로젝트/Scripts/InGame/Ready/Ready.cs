using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class Ready : MonoBehaviour
{
    /// <summary>
    /// 타이머
    /// </summary>
    private float _readyTimer;

    [SerializeField] private TextMeshProUGUI _readyTimerText;
    
    /// <summary>
    /// 준비 시간
    /// </summary>
    private float _readyTime;
    public float ReadyTimer => _readyTimer;

    public ReCombination ReCombination;
    public Drop Drop;

    private void OnEnable()
    {
        StartTimer();
        PlayerManager.Instance.Players[0].ResetRoulettePanel();
        WindowManager.Instance.GetInGame().PlayersMap.SetView(PlayerManager.Instance.Players[0]);
    }

    public void SetTimer(float time)
    {
        _readyTime = time;
    }

    private void StartTimer()
    {
        if (!InGame.IsEndGame)
        {
            StartCoroutine(SetReadyTimerCo());
        }
    }
    
    private IEnumerator SetReadyTimerCo()
    {
        _readyTimer = _readyTime;
        while (true)
        {
            if (_readyTimer <= 0)
            {
                _readyTimer = 0;
                _readyTimerText.text = $"{_readyTimer}";
                InGame.CurGameType = EGameType.Battle;
                break;
            }
            _readyTimerText.text = $"{Mathf.Ceil(_readyTimer)}";
            _readyTimer -= Time.deltaTime;
            yield return null;
        }
        ReCombination.ClearSlot();
    }

}
