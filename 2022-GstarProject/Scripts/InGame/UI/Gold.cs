using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class Gold : MonoBehaviour
{
    public TextMeshProUGUI GoldText;

    private void Start()
    {
        DataManager.Instance.GoldObj = this;
    }
}
