using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnUsingInventory))] 
public class UnUsingInventoryEditor: Editor
{
    private UnUsingInventory _unUsingInventory = null;
    
    private void OnEnable()
    {
        _unUsingInventory = (UnUsingInventory) target;
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        if (GUILayout.Button("아이템 생성"))
        {
            _unUsingInventory.AddItem(_unUsingInventory.CurItemCode);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
