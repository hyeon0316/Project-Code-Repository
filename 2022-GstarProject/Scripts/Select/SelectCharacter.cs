using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectCharacter : MonoBehaviour, IPointerDownHandler
{
    public JobType CurJobType;
    public string JobName;
    [TextArea]
    public string JobDescription;

    public SelectCamera SelectCamera;

    public Animator Animator;
    
    public bool IsClick { get; set; }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsClick)
        {
            SelectCamera.LookCharacter(this);
            Animator.SetTrigger(Global.SelectTrigger);
            IsClick = true;
        }
    }
}
