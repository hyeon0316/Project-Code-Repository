using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphicPage : MonoBehaviour
{
   [SerializeField] private TextMeshProUGUI _curResolutionText;

   [SerializeField] private GameObject _resolutionList;

   [SerializeField] private GameObject _fullScreenCheckObject;
   [SerializeField] private GameObject _EmoteCheckObject;



   private void Start()
   {
      InitScreen();
   }

   private void InitScreen()
   {
      _fullScreenCheckObject.SetActive(Screen.fullScreen);
      _curResolutionText.text = $"{Screen.width}x{Screen.height}";
   }
   
   /// <summary>
   /// 해상도 선택창 열기
   /// </summary>
   public void SetActiveResolutionList()
   {
      _resolutionList.SetActive(!_resolutionList.activeSelf);
   }

   public void Set3840x2160()
   {
      SetResolution(3840,2160);
   }
   public void Set1920x1080()
   {
      SetResolution(1920, 1080);
   }

   public void Set1600x900()
   {
      SetResolution(1600,900);
   }

   public void Set1280x720()
   {
      SetResolution(1280,720);
   }

   /// <summary>
   /// 원하는 해상도를 설정
   /// </summary>
   private void SetResolution(int width, int height)
   {
      Screen.SetResolution(width, height, Screen.fullScreen);
      _curResolutionText.text = $"{width}x{height}";
      SetActiveResolutionList();
   }

   public void SetFullScreen()
   {
      SoundManager.Instance.PlayEffect(EffectType.CheckBox);
      Screen.fullScreen = !Screen.fullScreen;
      _fullScreenCheckObject.SetActive(!_fullScreenCheckObject.activeSelf);
   }
   
   public void SetEmotionBlock()
   {
      SoundManager.Instance.PlayEffect(EffectType.CheckBox);
      _EmoteCheckObject.SetActive(!_EmoteCheckObject.activeSelf);
      DataManager.Instance.IsEmotionBlock = _EmoteCheckObject.activeSelf;
   }
}
