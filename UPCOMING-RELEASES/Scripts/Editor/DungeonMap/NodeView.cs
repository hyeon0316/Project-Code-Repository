using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Action<NodeView> OnNodeSelected;
    public HNode.Node Node;
    public Port Input;
    public Port Output;

    public NodeView(HNode.Node node ,Vector2 pos = default)
    {
        Node = node;
        this.title = node.name;
        this.viewDataKey = node.GUID;
        SetPosition(new Rect(pos == default ? node.Position : pos, Vector2.zero));

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
        base.SetPosition(newPos);
        Node.Position.x = newPos.xMin;
        Node.Position.y = newPos.yMin;
    }
}
