using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillToolTip : MonoBehaviour
{
    [SerializeField]
    private GameObject go_Base;
    [SerializeField]
    private Text txt_SkillName;
    [SerializeField]
    private Text txt_ManaConsum;
    [SerializeField]
    private Image skillImage;
    [SerializeField]
    private Text txt_SkillDesc;
    [SerializeField]
    private Text txt_SkillCool;

    public void SkillShowToolTip(Skill _skill,Vector3 _pos)
    {
        go_Base.SetActive(true);
        _pos += new Vector3(go_Base.GetComponent<RectTransform>().rect.width * 0.5f,
                            +go_Base.GetComponent<RectTransform>().rect.height * 0.8f,
                            0);
        go_Base.transform.position = _pos;

        txt_SkillName.text = _skill.skillName;
        txt_ManaConsum.text = _skill.manaCon;
        skillImage.sprite = _skill.skillImage;
        txt_SkillDesc.text = _skill.skillDesc;
        txt_SkillCool.text = _skill.skillCool;    
    }

    public void SkillHideToolTip()
    {
        go_Base.SetActive(false);
    }
}
