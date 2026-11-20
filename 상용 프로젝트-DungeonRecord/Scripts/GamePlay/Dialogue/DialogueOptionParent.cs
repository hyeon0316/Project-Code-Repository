using System.Collections.Generic;
using UnityEngine;

public class DialogueOptionParent : MonoBehaviour
{
    [SerializeField] private Transform m_ItemContainer;

    private readonly UIItemPool<DialogueOptionUI> m_Pool = new();

    private void Awake()
    {
        m_Pool.Init(m_ItemContainer, DialogueOptionUI.AssetID);
    }

    public void Set(List<DialogueNodeOption> dialogueOptionData)
    {
        m_Pool.Set(dialogueOptionData.Count, (item, i) =>
        {
            item.Set(dialogueOptionData[i].Callback, dialogueOptionData[i].Sentence);
        });
    }
}
