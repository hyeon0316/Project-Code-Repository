using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class SystemBase : MonoBehaviour
{
    public GameObject SystemWindow;
    private bool _isActivate;
    public Slider BgmSlider;
    public Slider EffectSlider;
    private bool _canAdjust;

    public float BgmVolume = 1;
    public float EffectVolume = 1;


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    
    private void Start()
    {
        BgmSlider.value = BgmVolume;
        EffectSlider.value = EffectVolume;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isActivate = !_isActivate;
            OpenSystem(_isActivate);
            BgmSlider.gameObject.SetActive(false);
            EffectSlider.gameObject.SetActive(false);
        }
        FreezeGame();
    }

    private void FreezeGame()
    {
        if (_isActivate)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
    
    private void OpenSystem(bool activate)
    {
        SystemWindow.SetActive(activate);
    }

    public void ResumeBtn()
    {
        SoundManager.Instance.Play("Object/Button",SoundType.Effect);
        _isActivate = false;
        OpenSystem(_isActivate);
    }

    public void LobbyBtn()
    {
        SoundManager.Instance.Play("Object/Button",SoundType.Effect);
        _isActivate = false;
        SystemWindow.SetActive(_isActivate);
        SceneManager.LoadScene("Menu");
        Destroy(this.gameObject);
    }

    public void ExitBtn()
    {
        SoundManager.Instance.Play("Object/Button", SoundType.Effect);
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }

    public void SoundBtn()
    {
        SoundManager.Instance.Play("Object/Button",SoundType.Effect);
        _canAdjust = !_canAdjust;
        BgmSlider.gameObject.SetActive(_canAdjust);
        EffectSlider.gameObject.SetActive(_canAdjust);
    }

    public void ApplyBgm(float value)
    {
        SoundManager.Instance.ApplyBgmVolume(value);
    }

    public void ApplyEffect(float value)
    {
        SoundManager.Instance.ApplyEffectVolume(value);
    }


    private void CloseSoundBtn()
    {
        _canAdjust = false;
        BgmSlider.gameObject.SetActive(_canAdjust);
        EffectSlider.gameObject.SetActive(_canAdjust);
    }
}
