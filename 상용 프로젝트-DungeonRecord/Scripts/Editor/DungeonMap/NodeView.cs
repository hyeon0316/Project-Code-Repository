using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Action<NodeView> OnNodeSelected;
    public Func<Vector2, Vector2> SnapPosition;
    public HNode.Node Node;
    public Port Input;
    public Port Output;

    public NodeView(HNode.Node node)
    {
        Node = node;
        this.title = node.name;
        this.viewDataKey = node.GUID;
        SetPosition(new Rect(node.Position, Vector2.zero));

        CreateInputPorts();
        CreateOutputPorts();
    }

    public override void OnSelected()
    {
        base.OnSelected();
        OnNodeSelected?.Invoke(this);
    }

    private void CreateInputPorts()
    {
        if (Node is not StartStageNode)
        {
            Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        }

        if(Input != null)
        {
            Input.portName = "";
            inputContainer.Add(Input);
        }
    }

    private void CreateOutputPorts()
    {
        if (Node is not EndStageNode)
        {
            Output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        }

        if (Output != null)
        {
            Output.portName = "";
            outputContainer.Add(Output);
        }
    }


    public override void SetPosition(Rect newPos)
    {
        if (SnapPosition != null)
            newPos.position = SnapPosition(newPos.position);

        base.SetPosition(newPos);
        if (Node.Position == newPos.position)
            return;

        Node.Position = newPos.position;
        UnityEditor.EditorUtility.SetDirty(Node);
    }
}
