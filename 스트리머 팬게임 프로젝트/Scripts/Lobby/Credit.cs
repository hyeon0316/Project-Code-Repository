using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credit : MonoBehaviour, IPopUp
{
    [SerializeField] private Animator _animator;
    
    public bool IsActive { get; set; }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsActive)
            {
                ClosePopUp();
            }
        }
    }
    
    public void ActivePopUp()
    {
        IsActive = true;
        SoundManager.Instance.PlayEffect(EffectType.Page);
        _animator.SetBool(Global.OpenBool, true);
    }

    public void ClosePopUp()
    {
        IsActive = false;
        _animator.SetBool(Global.OpenBool, false);
    }
}
