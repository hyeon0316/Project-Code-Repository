using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(OptionNode))]
public class DialogueOptionNodeDrawer : NodeEditor
{
    private OptionNode m_OptionNode;

    private string m_NewSentenceOutput;
    private string m_NewOptionOutput;
    private int m_DeleteOptionIndex;

    public override void OnBodyGUI()
    {
        if (m_OptionNode == null)
        {
            m_OptionNode = target as OptionNode;
        }

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Entry"));

        EditorGUILayout.PrefixLabel("Sentence");
        m_NewSentenceOutput = EditorGUILayout.TextField(m_NewSentenceOutput);
        EditorGUILayout.PrefixLabel("Option");
        m_NewOptionOutput = EditorGUILayout.TextField(m_NewOptionOutput);

        if (GUILayout.Button("Create New Option"))
        {
            bool isSentenceNull = m_NewSentenceOutput == null || m_NewSentenceOutput.Length == 0;
            bool isOptionNull = m_NewOptionOutput == null || m_NewOptionOutput.Length == 0;

            if (isSentenceNull || isOptionNull)
            {
                EditorUtility.DisplayDialog("Error create port", "Value is Empty", "OK");
                return;
            }

            bool isMatchExistingOption = false;
            foreach (NodePort port in m_OptionNode.DynamicOutputs)
            {
                if (port.fieldName == m_NewOptionOutput)
                {
                    isMatchExistingOption = true;
                    break;
                }
            }

            if (isMatchExistingOption)
            {
                EditorUtility.DisplayDialog("Error create port", "The option is already in use ", "OK");
                return;
            }
            m_OptionNode.AddDynamicOutput(typeof(int), Node.ConnectionType.Multiple, Node.TypeConstraint.None, m_NewOptionOutput);
            m_OptionNode.DialogueOptions.Add(new DialogueNodeOption(m_NewSentenceOutput, m_NewOptionOutput));
        }

        if (m_OptionNode.DynamicOutputs.Count() != 0)
        {
            EditorGUILayout.PrefixLabel("Option List");
            foreach (var option in m_OptionNode.DialogueOptions)
            {
                option.Sentence = EditorGUILayout.TextField(option.Sentence);
            }

            EditorGUILayout.PrefixLabel("Delete Option");
            List<string> options = new();
            foreach (NodePort port in m_OptionNode.DynamicOutputs)
            {
                options.Add(port.fieldName);
            }
            m_DeleteOptionIndex = EditorGUILayout.Popup(m_DeleteOptionIndex, options.ToArray());


            if (GUILayout.Button("Delete"))
            {
                foreach (var option in m_OptionNode.DialogueOptions)
                {
                    if (option.Option == m_OptionNode.DynamicOutputs.ElementAt(m_DeleteOptionIndex).fieldName)
                    {
                        m_OptionNode.DialogueOptions.Remove(option);
                        break;
                    }
                }
                m_OptionNode.RemoveDynamicPort(m_OptionNode.DynamicOutputs.ElementAt(m_DeleteOptionIndex));
            }
        }

        foreach (NodePort port in m_OptionNode.DynamicOutputs)
        {
            NodeEditorGUILayout.PortField(port);
        }
    }
}
