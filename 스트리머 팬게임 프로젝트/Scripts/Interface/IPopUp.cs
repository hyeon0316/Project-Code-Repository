using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public interface IPopUp
{
    public bool IsActive { get; set; }
    
    void ActivePopUp();

    void ClosePopUp();
}
