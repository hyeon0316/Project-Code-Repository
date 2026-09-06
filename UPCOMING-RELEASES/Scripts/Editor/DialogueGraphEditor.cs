using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeGraphEditor(typeof(DialogueGraph))]
public class DialogueGraphEditor : NodeGraphEditor
{
    public override string GetNodeMenuName(Type type)
    {
        if (typeof(DialogueNode).IsAssignableFrom(type))
        {
            return base.GetNodeMenuName(type);
        }
        else return null;
    }
}
