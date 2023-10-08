using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Match : MonoBehaviour, IPopUp
{
   public bool IsActive { get; set; }
   [SerializeField] private Button _startButton;

   [SerializeField] private Animator _panelAnim;

   /// <summary>
   /// 매칭이 시작
   /// </summary>
   private bool _isMatching;

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Escape))
      {
         if (IsActive)
         {
            ClosePopUp();
         }
      }
   }

   public void ActivePopUp()
   {
      _isMatching = true;
      _panelAnim.gameObject.SetActive(true);
      SoundManager.Instance.PlayBGM(BGMType.Loading);
      SoundManager.Instance.PlayEffect(EffectType.MatchStart);
      _startButton.interactable = false;
      
      _panelAnim.SetBool(Global.OpenBool, true);
      
      Invoke("DelaySetBool",1f);
   }

   private void DelaySetBool()
   {
      IsActive = true;
   }

   public void InitMatchPanel()
   {
      _panelAnim.gameObject.SetActive(false);
      _startButton.interactable = true;
      IsActive = false;
      _isMatching = false;
   }
   

   public void ClosePopUp()
   {
      SoundManager.Instance.PlayBGM(BGMType.Lobby);
      IsActive = false;
      _startButton.interactable = true;
      NetworkManager.Instance.DisconnectServer();
      _panelAnim.SetBool(Global.OpenBool, false);
      Invoke("DelaySetMatchBool",1);
   }

   public bool IsMatching()
   {
      return _isMatching;
   }
   private void DelaySetMatchBool()
   {
      _isMatching = false;
   }
   
   public void StartMatch()
   {
      NetworkManager.Instance.StartConnectServer();
   }

   
   /// <summary>
   /// 닷지되어 다시 매칭을 잡을때
   /// </summary>
   public void ReTryMatch()
   {
      _panelAnim.gameObject.SetActive(true);
      StartCoroutine(StartDelayMatchCo());
   }
   
   private IEnumerator StartDelayMatchCo()
   {
      ActivePopUp();
      yield return new WaitForSeconds(1f);
      StartMatch();
      yield return new WaitForSeconds(1f);
      IsActive = true;
   }
   
}
