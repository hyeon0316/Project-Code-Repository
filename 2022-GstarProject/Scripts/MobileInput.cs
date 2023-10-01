using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInput : MonoBehaviour
{
    private float _interval = 0.3f;
    private float _doubleClickedTime = -1.0f;

    // Update is called once per frame
    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) //뒤로가기 버튼
            {
                if ((Time.time - _doubleClickedTime) < _interval)
                {
                    _doubleClickedTime = -1;
                    Application.Quit();
                }
                else
                {
                    _doubleClickedTime = Time.time;
                }
                
            }
        }
    }
}
