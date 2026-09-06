using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PopupFactory 
{
    private Queue<BasePopupParam> m_PararmQueue = new();
    private Stack<PopupBehaviour> m_ActivePopups = new();

    private Transform m_PopupParent;

    public void SetParent(Transform parent)
    {
        m_PopupParent = parent;
    }

    public async UniTask OnUpdate()
    {
        if (m_PararmQueue.Count == 0)
        {
            return;
        }

        await UniTask.Yield();

        BasePopupParam pararm = m_PararmQueue.Dequeue();
        PopupBehaviour popup = AddressableBundleManager.Instance.AssetInstantiate<PopupBehaviour>(pararm.GetAssetID(), m_PopupParent);
        if (popup != null)
        {
            popup.SetParam(pararm);
            popup.Show();
            m_ActivePopups.Push(popup);
        }
    }

    public ParamType PushMessage<ParamType>(AssetID assetID, string message) where ParamType : BasePopupParam , new()
    {
        ParamType paramType = new();
        paramType.Set(assetID, message);

        m_PararmQueue.Enqueue(paramType);
        return paramType;
    }

    public void PopMessage()
    {
        m_ActivePopups.Pop();
    }

    public void Clear()
    {
        foreach (var popup in m_ActivePopups)
        {
            GameObject.DestroyImmediate(popup.gameObject);
        }
        m_ActivePopups.Clear();
    }
}
