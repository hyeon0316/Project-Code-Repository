using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Tutorial/Parent")]
public class TutorialParentSO : ScriptableObject
{
    public List<TutorialSO> TutorialList;
}
