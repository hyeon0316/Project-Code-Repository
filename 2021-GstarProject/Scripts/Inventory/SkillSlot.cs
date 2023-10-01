using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Skill skill;

    [SerializeField]
    private SkillToolTipDatabase theSkillToolTipDatabase;
    

    public void OnPointerEnter(PointerEventData eventData)
    {
        theSkillToolTipDatabase.ShowToolTip(skill, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        theSkillToolTipDatabase.HideToolTip();
    }
}
