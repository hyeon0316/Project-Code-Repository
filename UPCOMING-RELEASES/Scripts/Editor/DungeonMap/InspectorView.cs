using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

    private Editor m_Editor;

    public void UpdateSelection(UnityEngine.Object target)
    {
        Clear();
        UnityEngine.Object.DestroyImmediate(m_Editor);

        m_Editor = Editor.CreateEditor(target);
        var container = new IMGUIContainer(() => { m_Editor.OnInspectorGUI(); });
        Add(container);
    }
}
