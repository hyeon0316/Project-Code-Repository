using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Lobby : MonoBehaviour
{
    public Queue<string> Sentences = new Queue<string>();

    public GameObject BackButton;
    public GameObject ManualWindow;
    public GameObject ManualPages;

    private void Start()
    {
        JsonToDataManager.Instance.Save().Forget();
        SoundManager.Instance.Play("LobbyBGM",SoundType.Bgm);
    }

    public void StartGame()
    {
        SoundManager.Instance.Play("Object/Button",SoundType.Effect);
        SoundManager.Instance.Play("TutorialBGM",SoundType.Bgm);
        JsonToDataManager.Instance.Load().ContinueWith(() => SceneManager.LoadScene("Tutorial")).Forget(); 
    }
    
    public void ShowManual()
    {
        SoundManager.Instance.Play("Object/Button",SoundType.Effect);
        ManualWindow.SetActive(true);
    }

    public void ExitGame()
    {
        SoundManager.Instance.Play("Object/Button",SoundType.Effect);
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void NextPageBtn()
    {
        SoundManager.Instance.Play("Object/Button",SoundType.Effect);
        if (ManualPages.transform.GetChild(0).gameObject.activeSelf)
        {
            BackButton.SetActive(true);
            ManualPages.transform.GetChild(0).gameObject.SetActive(false);
            ManualPages.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            BackButton.SetActive(false);
            ManualPages.transform.GetChild(0).gameObject.SetActive(true);
            ManualPages.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
    public void BackBtn()
    {
        SoundManager.Instance.Play("Object/Button",SoundType.Effect);
        ManualWindow.SetActive(false);
    }
}
