using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum StatusType
{
    AttackBuff,
    PlusDefense,
    Weaken,
    Blooding,
    HealWeaken
}
public class StatusUI : MonoBehaviour
{
    [SerializeField] private BuffUI[] _buffs;

    public void SetBuffUI(StatusType type, int count)
    {
        _buffs[(int)type].SetCount(count);
    }

    public bool IsUIActive(StatusType type)
    {
        return _buffs[(int) type].gameObject.activeSelf;
    }

    public void CloseBuffUI(StatusType type)
    {
        _buffs[(int)type].CloseUI();
    }

    public void Init()
    {
        foreach (var buff in _buffs)
        {
            buff.CloseUI();
        }
    }
    
}
