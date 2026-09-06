using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueOptionUI : ItemBehaviour
{
    public static AssetID AssetID = new("Prefabs/UI/DialogueOption");
    [SerializeField] private Text m_Text;

    private Action m_Callback;

    public void Set(Action callback, string sentence)
    {
        m_Callback = callback;
        m_Text.text = sentence;
    }

    public void OnClickEvent_Callback()
    {
        m_Callback?.Invoke();
    }
}
