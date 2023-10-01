using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleBtn : MonoBehaviour
{
    [SerializeField]
    private Image startFade;

    public void GameStart()
    {
        StartCoroutine(StartFade());
        Invoke("StartSceneLoad", 3f);
    }
   
    void StartSceneLoad()
    {
        SceneManager.LoadScene("Intro");
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    IEnumerator StartFade()
    {
        startFade.gameObject.SetActive(true);

        Color color = startFade.color;

        for(int i=0; i<100; i++)
        {
            color.a = color.a + 0.01f;
            startFade.color = color;
            yield return new WaitForSecondsRealtime(0.01f);
        }      
    }
}
