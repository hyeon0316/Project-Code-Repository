using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum GameOverType
{
    Win,
    Lose
}

public class GameOver : MonoBehaviour, ISetAlpha
{
    [SerializeField] private Sprite _winSprite;
    [SerializeField] private Sprite _loseSprite;

    [SerializeField] private Image _image;
    [SerializeField] private GameObject _button;

    [SerializeField] private TextMeshProUGUI _rankText;

    public bool IsGameFinish => _button.gameObject.activeInHierarchy;

    public void Init()
    {
        SetActiveUI(false);
        gameObject.SetActive(false);
    }

    public void ShowGameOver(GameOverType type)
    {
        gameObject.SetActive(true);
        StartCoroutine(ShowGameOverCo(type));
    }
    
   
    /// <summary>
    /// 승리 혹은 패배시 보여질 UI 셋팅
    /// </summary>
    /// <param name="type"></param>
    private IEnumerator ShowGameOverCo(GameOverType type)
    {
        switch (type)
        {
            case GameOverType.Win:
                _image.sprite = _winSprite;
                _rankText.text = "1등";
                break;
            case GameOverType.Lose:
                _rankText.text = $"{Global.MaxRoomPlayer - PlayerManager.Instance.GetPlayerDeadCount()}등";
                _image.sprite = _loseSprite;
                break;
        }
        SetAlpha(0);
        _image.DOFade(1, 1);
        yield return new WaitForSeconds(1f);
        SetActiveUI(true);
    }

    /// <summary>
    /// 나가기 버튼과 등수 텍스트 활성화 여부
    /// </summary>
    /// <param name="isActive"></param>
    private void SetActiveUI(bool isActive)
    {
        _button.SetActive(isActive);
        _rankText.gameObject.SetActive(isActive);
    }

    public void SetAlpha(float value)
    {
        Color color = _image.color;
        color.a = value;
        _image.color = color;
    }
}
