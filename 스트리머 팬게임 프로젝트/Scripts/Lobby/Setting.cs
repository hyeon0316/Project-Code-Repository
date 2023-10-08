using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Setting : MonoBehaviour, IPopUp
{
    private enum SetType
    {
        Sound,
        Graphic,
        Control
    }

    private Sequence _sequence;
    public bool IsActive { get; set; }

    [SerializeField] private Transform[] _bookMarks;
    [SerializeField] private GameObject[] _pages;
    [SerializeField] private CanvasGroup _canvasGroup;
    
    [SerializeField] private GameObject _blurEffect;

    private void Start()
    {
        SetSoundPage();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsActive)
            {
                ClosePopUp();
            }
            else
            {
                if (!WindowManager.Instance.GetLobby().CheckActiveMatch() && !WindowManager.Instance.GetLobby().CheckActiveCredit())
                {
                    Debug.Log("설정창 오픈");
                    ActivePopUp();
                }
            }
        }
    }

    /// <summary>
    /// 사운드 설정창으로 변경
    /// </summary>
    public void SetSoundPage()
    {
        int page = (int) SetType.Sound;
        SetPage(page);
    }
    
    /// <summary>
    /// 그래픽 설정창으로 변경
    /// </summary>
    public void SetGraphicPage()
    {
        int page = (int) SetType.Graphic;
        SetPage(page);
    }

    public void SetControlPage()
    {
        int page = (int) SetType.Control;
        SetPage(page);
    }

    /// <summary>
    /// 설정창 변경
    /// </summary>
    private void SetPage(int page)
    {
        for (int i = 0; i < _bookMarks.Length; i++)
        {
            if (i == page)
            {
                _bookMarks[i].SetAsLastSibling();
                _pages[i].SetActive(true);
            }
            else
            {
                _bookMarks[i].SetAsFirstSibling();
                _pages[i].SetActive(false);
            }
        }
    }

    public void ActivePopUp()
    {
        SoundManager.Instance.PlayEffect(EffectType.Page);
        IsActive = true;
        _blurEffect.SetActive(true);
        _sequence = DOTween.Sequence().SetAutoKill(false)
            .OnStart(() =>
            {
                transform.localScale = Vector3.zero;
                GetComponent<CanvasGroup>().alpha = 0;
            }).Append(transform.DOScale(0.8f, 0.2f)).SetEase(Ease.Linear)
            .Join(_canvasGroup.DOFade(1, 0.2f));
    }

    public void ClosePopUp()
    {
        IsActive = false;
        _blurEffect.SetActive(false);
        _sequence = DOTween.Sequence().SetAutoKill(false)
            .Append(transform.DOScale(0, 0.2f)).SetEase(Ease.Linear)
            .Join(_canvasGroup.DOFade(0, 0.2f));
    }
}
