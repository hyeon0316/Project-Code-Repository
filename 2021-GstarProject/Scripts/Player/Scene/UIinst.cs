using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIinst : MonoBehaviour
{
    public static UIinst inst = null;
    // Start is called before the first frame update
    void Awake()
    {
        if (inst == null)
        {
            inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
