using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Skill", menuName = "New Skill/skill")]
public class Skill : ScriptableObject
{

    public string skillName;
    public string manaCon;
    public Sprite skillImage;
    [TextArea]
    public string skillDesc;
    public string skillCool;
    
}
