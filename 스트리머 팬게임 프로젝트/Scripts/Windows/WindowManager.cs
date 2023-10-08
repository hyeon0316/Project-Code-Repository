using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum WindowType 
{
   Title,
   Lobby,
   Select,
   InGame
}

public class WindowManager : Singleton<WindowManager>
{
   [SerializeField] private GameObject[] _windows;

   [SerializeField] private GameObject _mainCamera;
   private void Start()
   {
      if (DataManager.Instance.IsInit)
      {
         SetWindow(WindowType.Lobby);
         PlayerManager.Instance.Players[0].SetName(DataManager.Instance.PlayerNickName);
         SoundManager.Instance.PlayBGM(BGMType.Lobby);
         DataManager.Instance.IsInit = false;
      }
      else
      {
         Screen.SetResolution(1920, 1080, true); //todo: 게임 시작시 해상도 설정
         SetWindow(WindowType.Title);
      }
   }

   public Lobby GetLobby()
   {
      return _windows[(int) WindowType.Lobby].GetComponent<Lobby>();
   }

   public Select GetSelect()
   {
      return _windows[(int) WindowType.Select].GetComponent<Select>();
   }

   public InGame GetInGame()
   {
      return _windows[(int) WindowType.InGame].GetComponent<InGame>();
   }
 

   public void SetWindow(WindowType windowType)
   {
      switch (windowType)
      {
         case WindowType.Title:
            SoundManager.Instance.PlayBGM(BGMType.Title);
            break;
         case WindowType.Select:
            SoundManager.Instance.PlayBGM(BGMType.Select);
            break;
      }
      int windowNum = (int)windowType;
      
      for (int i = 0; i < _windows.Length; i++)
      {
         _windows[i].SetActive(i == windowNum);
      }
   }

   /// <summary>
   /// 컷씬 진입 시 카메라가 2개이기 때문에 프레임이 떨어지는 것을 방지
   /// </summary>
   public void SetActiveCamera(bool isActive)
   {
      _mainCamera.SetActive(isActive);
   }

   public void GoLobby()
   {
      DataManager.Instance.IsInit = true;
      SceneManager.LoadScene("InGame",LoadSceneMode.Single);
   }



}
