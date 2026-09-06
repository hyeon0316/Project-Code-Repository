using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageBehaviour : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;
    private bool m_IsTransition;

    private void OnEnable()
    {
        if (m_Animator != null)
        {
            m_Animator.enabled = false;
        }
    }

    public virtual UniTask OnCreate()
    {
        return UniTask.CompletedTask;
    }

    public virtual UniTask OnLoad()
    {
        return UniTask.CompletedTask;
    }

    public virtual async UniTask OnTransitionStart()
    {
        if (m_Animator != null)
        {
            m_IsTransition = true;
            m_Animator.enabled = true;
            m_Animator.Update(0);
            await m_Animator.WaitForCurAnimationEnd();
            m_Animator.enabled = false;
            m_IsTransition = false;
        }
    }

    public virtual UniTask OnResume()
    {
        return UniTask.CompletedTask;
    }

    public virtual UniTask OnPause()
    {
        return UniTask.CompletedTask;
    }

    public virtual UniTask OnFinish()
    {
        return UniTask.CompletedTask;
    }

    public virtual void Back()
    {
        if (m_IsTransition)
            return;

        UIManager.Instance.PageSystem.Back().Forget();
    }

    protected bool IsActivePage()
    {
        return this.gameObject.activeSelf;
    }
}
