using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlToolTip : MonoBehaviour
{
    [SerializeField]
    private GameObject cToolTip;

    public static bool cToolTipOn = false;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
       if(Input.GetKeyDown(KeyCode.Escape))
        {
            Invoke("CToolTip_Exit", 0.01f);
        }
    }

    public void CToolTip()
    {
        cToolTip.SetActive(true);
        cToolTipOn = true;
    }

    public void CToolTip_Exit()
    {
        cToolTip.SetActive(false);
        cToolTipOn = false;
    }
}
