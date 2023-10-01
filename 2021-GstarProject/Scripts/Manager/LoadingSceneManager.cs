using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;
    [SerializeField]
    private Image loadingBar;

    [SerializeField]
    private Image[] randBackGround;
    [SerializeField]
    private Text[] randToolTip;

    [SerializeField]
    private Text loadingValueText;

    private float loadingValue = 0;

    private float delay;

    private void Start()
    {
        StartCoroutine(LoadScene());

        int randBack = Random.Range(0, 3);
        int randTip = Random.Range(0, 3);
        randBackGround[randBack].gameObject.SetActive(true);
        randToolTip[randTip].gameObject.SetActive(true);
    }

    private void Update()
    {
        if (loadingValue >= 100)
            loadingValue = 100;
        loadingValueText.text = string.Format("{0:P1}", loadingValue);
    }
    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    IEnumerator LoadScene()
    {
        yield return null;
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;
        float timer = 0.0f;
        while(!op.isDone)
        {
            yield return null;
            delay = 5;
            timer += Time.deltaTime / delay;
            loadingValue = loadingBar.fillAmount;
            if (op.progress <0.9f)
            {
                loadingBar.fillAmount = Mathf.Lerp(loadingBar.fillAmount, op.progress, timer);
                if (loadingBar.fillAmount >= op.progress)
                {
                    timer = 0f;
                }
                if (GameObject.Find("PlayerM 2") != null)
                {
                    Player.inst.rigidbody.useGravity = false;
                    Player.inst.isMove = false;
                    Player.inst.animator.SetBool("isMove", false);

                }
            }
            else
            {
                delay = 1;
                loadingBar.fillAmount = Mathf.Lerp(loadingBar.fillAmount, 1f, timer);
                if (loadingBar.fillAmount == 1.0f)
                {
                    if (GameObject.Find("PlayerM 2") != null)
                    {
                        Player.inst.rigidbody.useGravity = true;
                        Player.inst.isMove = false;
                        Player.inst.animator.SetBool("isMove", false);
                        Player.inst.isSkill = false;
                        op.allowSceneActivation = true;
                        yield break;
                    }
                    else if (GameObject.Find("PlayerM 2") == null)
                    {
                        op.allowSceneActivation = true;
                        yield break;
                    }

                }
            }
        }
    }
}
