using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SkillToolTipDatabase : MonoBehaviour
{
    [SerializeField]
    private SkillToolTip theSkillToolTip;

    public void ShowToolTip(Skill _skill ,Vector3 _pos)
    {
        theSkillToolTip.SkillShowToolTip(_skill, _pos);
    }

    // 📜SlotToolTip 👉 📜Slot 징검다리
    public void HideToolTip()
    {
        theSkillToolTip.SkillHideToolTip();
    }
}
