using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TownLoading2 : MonoBehaviour
{
    public string sceneName;
    private void OnEnable()
    {
        LoadingSceneManager.LoadScene(sceneName);
    }
}
