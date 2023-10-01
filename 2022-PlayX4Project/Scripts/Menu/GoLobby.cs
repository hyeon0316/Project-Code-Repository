using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GoLobby : MonoBehaviour
{
   private void OnEnable()
   {
      FindObjectOfType<SoundManager>().Clear();
      SceneManager.LoadScene("Menu");
   }
}
