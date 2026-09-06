using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIMultiView))]
class UIMultiViewInspector : Editor
{
    private string m_ViewName;

    public void OnEnable()
    {
        UIMultiView multiView = target as UIMultiView;
        bool canSave = multiView.Save();
        if (canSave)
        {
            EditorUtility.SetDirty(multiView);
        }
    }

    public void OnDisable()
    {
        UIMultiView multiView = target as UIMultiView;
        bool canSave = multiView.Save(); 
        if (canSave)
        {
            EditorUtility.SetDirty(multiView);
        }
    }

    public override void OnInspectorGUI()
    {
        UIMultiView multiView = target as UIMultiView;

        multiView.RemoveDeletedChildrens();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add View"))
        {
            if (string.IsNullOrEmpty(m_ViewName))
            {
                EditorUtility.DisplayDialog("", $"Empty view name.", "Ok");
            }
            else
            {
                if (!multiView.Add(m_ViewName))
                {
                    EditorUtility.DisplayDialog("", $"Key already exists. {m_ViewName}", "Ok");
                }
            }
        }

        m_ViewName = EditorGUILayout.TextField(m_ViewName);

        EditorGUILayout.EndHorizontal();

        DrawTabList(multiView);
    }


    private void DrawTabList(UIMultiView multiView)
    {
        string[] keyNames = multiView.GetKeyArray();

        if (keyNames.Length <= 0)
        {
            EditorGUILayout.LabelField("Empty groups");
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            int prevSelectIndex = 0;
            bool isContains = false;
            string selectView = multiView.GetSelectView();
            for(int i = 0; i < keyNames.Length; i++)
            {
                if (keyNames[i] == selectView)
                {
                    prevSelectIndex = i;
                    isContains = true;
                    break;
                }
            }

            if (!isContains)
            {
                multiView.SetSelectView(keyNames[0]);
            }
            else
            {
                int newSelectIndex = EditorGUILayout.Popup(prevSelectIndex, keyNames);
                if (prevSelectIndex != newSelectIndex)
                {
                    //원래 자리 복원해주기
                    multiView.Save();

                    multiView.SetSelectView(keyNames[newSelectIndex]);
                    multiView.Save(); 

                    EditorUtility.SetDirty(multiView);
                }
            }

            if (GUILayout.Button("Save"))
            {
                multiView.Save();
                EditorUtility.SetDirty(multiView);
            }

            if (GUILayout.Button("Remove"))
            {
                multiView.RemoveCurrent();
            }

            EditorGUILayout.EndHorizontal();
        }

        var widgetList = multiView.GetWidget(multiView.GetSelectView());
        if (widgetList == null)
        {
            return;
        }

        GUIStyle guiStyle = new GUIStyle(GUI.skin.window)
        {
            alignment = TextAnchor.UpperLeft,
            fontStyle = FontStyle.Bold
        };
   
        for (int i = 0; i < widgetList.List.Count; ++i)
        {
            var widget = widgetList.List[i];
            GUILayout.BeginVertical(widget.Object.name, guiStyle);

            widget.AnchoredPosition = EditorGUILayout.Vector2Field("Position", widget.AnchoredPosition);
            widget.SizeDelta = EditorGUILayout.Vector2Field("Size", widget.SizeDelta);
            widget.Active = EditorGUILayout.Toggle("Active", widget.Active);
            widget.Apply();

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }
}
