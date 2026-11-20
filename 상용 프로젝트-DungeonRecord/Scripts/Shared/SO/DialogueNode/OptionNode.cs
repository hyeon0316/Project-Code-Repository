using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[System.Serializable]
public class DialogueNodeOption
{
    public string Sentence;
    public string Option;
    public DialogueNode ConnectingNode;
    public Action Callback;

    public DialogueNodeOption(string sentence, string option)
    {
        Sentence = sentence;
        Option = option;
    }

    public DialogueNodeOption(string sentence, Action callback)
    {
        Sentence = sentence;
        Callback = callback;
    }
}


[NodeTint("#009000")]
public class OptionNode : DialogueNode
{
    [Input] public int Entry;
    public List<DialogueNodeOption> DialogueOptions = new();

    public override void OnCreateConnection(NodePort from, NodePort to)
    {
        base.OnCreateConnection(from, to);

        foreach (var option in DialogueOptions)
        {
            if (from.fieldName == option.Option)
            {
                option.ConnectingNode = from.Connection.node as DialogueNode;
                break;
            }
        }
    }

    public override void OnRemoveConnection(NodePort port)
    {
        base.OnRemoveConnection(port);
        foreach (var option in DialogueOptions)
        {
            if (port.fieldName == option.Option)
            {
                option.ConnectingNode = null;
                break;
            }
        }
    }
}
