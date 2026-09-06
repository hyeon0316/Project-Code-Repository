using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유저별 고유 컨텐츠 관리
/// </summary>
public interface IManagableContents
{
    public void Initialize();
    public void UnInitialize();
    public void OnUpdate(float deltaTime);
}

public class ContentsManager : SingletonMonoBehaviour<ContentsManager>
{
    private Dictionary<Type, IManagableContents> m_Contents = new();

    private void Update()
    {
        OnUpdate(Time.deltaTime);
    }

    public void UnInitialize()
    {
        foreach (IManagableContents contents in m_Contents.Values)
        {
            contents.UnInitialize();
        }

        m_Contents.Clear();
    }

    public void AddContents<T>() where T : IManagableContents, new()
    {
        T contents = new T();
        m_Contents.Add(typeof(T), contents);
        contents.Initialize();
    }

    public Type Get<Type>() where Type : class, IManagableContents
    {
        if (m_Contents.TryGetValue(typeof(Type), out var outContents))
        {
            return outContents as Type;
        }

        return null;
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var contents in m_Contents.Values)
        {
            contents.OnUpdate(deltaTime);
        }
    }

}
