using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    FishBread,
    SisterHeart,
    Broom,
    HeroCloak,
    MaidCostume,
    NunDress
}
public class ItemBuffUIController : MonoBehaviour
{
    [SerializeField] private ItemBuffUI[] _buffs;

    public void SetItemBuffUI(BuffType type, bool isActive, byte value)
    {
        _buffs[(int)type].SetUI(isActive, value);
    }
    
    public void SetItemBuffUI(BuffType type, bool isActive)
    {
        _buffs[(int)type].SetUI(isActive);
    }

    public bool IsActive(BuffType type)
    {
        return _buffs[(int) type].gameObject.activeSelf;
    }
    public void CloseItemBuffUI(BuffType type)
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
