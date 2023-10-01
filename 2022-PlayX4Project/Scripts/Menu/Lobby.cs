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

    private float _textDelay = 0.1f;

    private void Start()
    {
        FindObjectOfType<SoundManager>().Play("LobbyBGM",SoundType.Bgm);
        DataManager.Instance().Save();
    }

    public void StartGame()
    {
        FindObjectOfType<SoundManager>().Play("Object/Button",SoundType.Effect);
        FindObjectOfType<SoundManager>().Play("TutorialBGM",SoundType.Bgm);
        DataManager.Instance().Load();
        SceneManager.LoadScene("Tutorial");
    }
    
    public void ShowManual()
    {
        FindObjectOfType<SoundManager>().Play("Object/Button",SoundType.Effect);
        ManualWindow.SetActive(true);
    }
    public void ExitGame()
    {
        FindObjectOfType<SoundManager>().Play("Object/Button",SoundType.Effect);
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void NextPageBtn()
    {
        FindObjectOfType<SoundManager>().Play("Object/Button",SoundType.Effect);
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
        FindObjectOfType<SoundManager>().Play("Object/Button",SoundType.Effect);
        ManualWindow.SetActive(false);
    }
}
