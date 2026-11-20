using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownCamera : MonoBehaviour, GlobalEvent<TownContentsPararm>.IEventHandler
{
    [SerializeField] private CinemachineVirtualCamera m_VirtualCamera;
    [SerializeField] private Transform m_CharacterTr;
    [SerializeField] private Transform m_DialogueFocusAnchor;

    private void Awake()
    {
        ContentsManager.Instance.Get<TownContents>().RegisterHandler(this);
    }

    private void OnDestroy()
    {
        ContentsManager.Instance.Get<TownContents>().UnRegisterHandler(this);
    }

    public void OnEvent(TownContentsPararm parameter)
    {
        switch (parameter.Type)
        {
            case TownContentsPararm.EType.StartDialogue:
                if (parameter.FocusTarget != null)
                {
                    Vector3 actorPos = parameter.FocusTarget.position;
                    Vector3 charPos = m_CharacterTr.position;
                    m_DialogueFocusAnchor.position = new Vector3(
                        (actorPos.x + charPos.x) * 0.5f,
                        charPos.y,
                        (actorPos.z + charPos.z) * 0.5f);
                    m_VirtualCamera.Follow = m_DialogueFocusAnchor;
                }
                break;
            case TownContentsPararm.EType.FinishDialogue:
                m_VirtualCamera.Follow = m_CharacterTr;
                break;
        }
    }
}
