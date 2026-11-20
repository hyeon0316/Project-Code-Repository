using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 로직과 UI간 정보 전달
/// </summary>
/// <typeparam name="T">보내질 정보가 담긴 콘텐츠</typeparam>
public class GlobalEvent<T> where T : struct
{
    public interface IEventHandler
    {
        /// <summary>
        /// 정보를 받아 실행될 이벤트 
        /// </summary>
        public void OnEvent(T parameter);
    }

    /// <summary>
    /// 이벤트 관리
    /// </summary>
    public interface IManagableHandler
    {
        public void RegisterHandler(IEventHandler handler);
        public void UnRegisterHandler(IEventHandler handler);
        public void Send(T parameter);
    }

    private List<IEventHandler> m_EventHandlers = new();
    private Queue<T> m_EventQueue = new();

    public void RegisterHandler(IEventHandler targetHandler)
    {
        m_EventHandlers.Add(targetHandler);
    }

    public void UnRegisterHandler(IEventHandler targetHandler)
    {
        m_EventHandlers.Remove(targetHandler);
    }

    public void Send(T parameter)
    {
        m_EventQueue.Enqueue(parameter);
    }

    public void OnUpdate()
    {
        if (m_EventQueue.Count == 0)
            return;

        var param = m_EventQueue.Dequeue();
        for (int i = m_EventHandlers.Count - 1; i >= 0; i--)
        {
            IEventHandler handler = m_EventHandlers[i];
            if (handler == null)
            {
                Debug.LogError("Handler is null");
                m_EventHandlers.RemoveAt(i);
                continue;
            }
            if (handler is Object unityObj && unityObj == null)
            {
                Debug.LogError($"Handler is destroyed: {handler.GetType().Name}");
                m_EventHandlers.RemoveAt(i);
                continue;
            }
            handler.OnEvent(param);
        }
    }
}