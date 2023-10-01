using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Skip : MonoBehaviour
{
   

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("sdfsdf");
            LoadingSceneManager.LoadScene("Town");
        }
    }
}
