using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundPage : MonoBehaviour
{
    [SerializeField] private GameObject _checkObject;
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private TextMeshProUGUI[] _volumePercentText;

    [SerializeField] private Slider _masterVolumeSlider;
    private void Start()
    {
        SetMasterVolume(0.5f);
        _masterVolumeSlider.value = 0.5f;
    }

    public void SetMasterVolume(float value)
    {
        _volumePercentText[0].text = $"{Math.Truncate(value * 100)}%";
        _mixer.SetFloat("Master", Mathf.Log10(value)*20); //최소값: -80 을 맞추기 위해서
    }
    
    public void SetBGMVolume(float value)
    {
        _volumePercentText[1].text = $"{Math.Truncate(value * 100)}%";
        _mixer.SetFloat("BGM", Mathf.Log10(value)*20);
    }
    
    public void SetSFXVolume(float value)
    {
        _volumePercentText[2].text = $"{Math.Truncate(value * 100)}%";
        _mixer.SetFloat("SFX", Mathf.Log10(value)*20);
    }


    /// <summary>
    /// 백그라운드 음향재생 체크박스
    /// </summary>
    public void SetActiveBGM()
    {
        SoundManager.Instance.PlayEffect(EffectType.CheckBox);
        SoundManager.Instance.SetActiveBGM();
        _checkObject.SetActive(!_checkObject.activeSelf);
    }
    
}
