using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [Header("시작 연출")] 
    [SerializeField] private GameObject _startGuide;
    
    
    [Header("닉네임 입력")]
    [SerializeField] private GameObject _blurImage;
    [SerializeField] private GameObject _nickNameParent;
    [SerializeField] private TMP_InputField _inputNickName;

    private string _guide;

    private bool _isStart;

    private void Start()
    {
        Screen.SetResolution(1920, 1080, true); //todo: 게임 시작시 해상도 설정
        
        Invoke("SetStartGuide", 1.5f);
        _guide = _inputNickName.text;
    }

    private void Update()
    {
        if (Input.anyKeyDown && _startGuide.activeSelf)
        {
            if (!_isStart)
            {
                TryInputNickName();
                _startGuide.SetActive(false);
            }
        }
    }

    public void SetStartGuide()
    {
        _startGuide.SetActive(true);
    }

    private void TryInputNickName()
    {
        _isStart = true;
        _nickNameParent.SetActive(true);
        _blurImage.SetActive(true);
    }
    
    /// <summary>
    /// 닉네임 최종결정
    /// </summary>
    public void SetNickName()
    {
        if (_inputNickName.text.Trim((char) 8203).Length == 0 || string.IsNullOrWhiteSpace(_inputNickName.text) || _inputNickName.text.Equals(_guide))
            _inputNickName.text = "플레이어";
        
        DataManager.Instance.PlayerNickName = _inputNickName.text;
        PlayerManager.Instance.Players[0].SetName(_inputNickName.text);
        Debug.Log(PlayerManager.Instance.Players[0].NickName);
        WindowManager.Instance.SetWindow(WindowType.Lobby);
        
        SoundManager.Instance.PlayBGM(BGMType.Lobby);
        SoundManager.Instance.PlayEffect(EffectType.NameInput);
    }
}
