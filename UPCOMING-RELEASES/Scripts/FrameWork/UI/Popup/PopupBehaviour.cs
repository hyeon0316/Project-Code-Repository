using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PopupBehaviour : MonoBehaviour
{
    public abstract void SetParam(BasePopupParam pararm);

    public virtual void Show()
    {
    }

    public virtual void Close()
    {
        UIManager.Instance.PopupSystem.PopMessage();
        GameObject.Destroy(this.gameObject);
    }
}
