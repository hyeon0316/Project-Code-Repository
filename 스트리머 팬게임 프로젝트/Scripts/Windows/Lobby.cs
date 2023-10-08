using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    [SerializeField] private Match _match;
    [SerializeField] private Credit _credit;
    [SerializeField] private GameObject _bag;
    [SerializeField] private GameObject _uiBag;

    [SerializeField] private GameObject _settingPanel;
    
    private void Start()
    {
        _bag.SetActive(true);
        _uiBag.SetActive(false);
    }

    public void ReTryMatch()
    {
        _match.ReTryMatch();
    }

    public void SetActiveSettingPanel(bool isAcitve)
    {
        _settingPanel.SetActive(isAcitve);
    }

    public bool CheckActiveMatch()
    {
        if (_match.IsMatching())
            return true;

        return false;
    }
    
    public bool CheckActiveCredit()
    {
        if (_credit.IsActive)
            return true;

        return false;
    }

    public void InitMatchPanel()
    {
        _match.InitMatchPanel();
    }

    public void ActiveBag()
    {
        SoundManager.Instance.PlayEffect(EffectType.Bag);
        _bag.SetActive(false);
        _uiBag.SetActive(true);
    }
    
    
   public void QuitGame()
   {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif
   }
   
}
