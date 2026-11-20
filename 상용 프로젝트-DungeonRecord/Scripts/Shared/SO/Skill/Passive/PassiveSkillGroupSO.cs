using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Character/Skill/PassiveGroup")]
public class PassiveSkillGroupSO : ScriptableObject
{
    public string ID;
    public string Name;
    public Sprite Icon;
    public PassiveSkillSO[] Skills;

    private void OnValidate()
    {
        ID = this.name;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

}
