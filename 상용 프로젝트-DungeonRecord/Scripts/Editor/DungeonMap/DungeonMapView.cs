using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.Linq;
using System;
using UnityEngine;
public class DungeonMapView : GraphView
{
    public new class UxmlFactory : UxmlFactory<DungeonMapView, GraphView.UxmlTraits> { }

    public Action<NodeView> OnNodeSelected;
    private DungeonMapTree m_Tree;

    public DungeonMapView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/DungeonMapEditor.uss");
        styleSheets.Add(styleSheet);
        //focusable = true;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.ClearItems();
        if (m_Tree == null)
            return;

        var types = TypeCache.GetTypesDerivedFrom<StageNode>();
        if (evt.target is NodeView nodeView)
        {
            foreach (var type in types)
            {
                if (nodeView.Node.GetType() == type || !CanUseType(type, nodeView.Node))
                    continue;
                evt.menu.AppendAction($"Convert to {type.Name}", a => ConvertNode(nodeView, type));
            }
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Delete", a => DeleteNodeView(nodeView));
            return;
        }

        Vector2 pos = SnapPosition(contentViewContainer.WorldToLocal(evt.mousePosition), null);
        foreach (var type in types)
        {
            if (!CanUseType(type, null))
                continue;
            evt.menu.AppendAction($"Create/{type.Name}", a => CreateNode(type, pos));
        }
    }

    public void PopulateView(DungeonMapTree tree)
    {
        if (m_Tree == tree)
            return;

        m_Tree = tree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        foreach (var node in m_Tree.Nodes)
        {
            CreateNodeView(node);
        }

        foreach (var n in m_Tree.Nodes)
        {
            n.Children.ForEach(c =>
            {
                var parentNodeView = FindNodeView(n);
                var childNodeView = FindNodeView(c);

                var edge = parentNodeView.Output.ConnectTo(childNodeView.Input);
                AddElement(edge);
            });
        }
    }

    private NodeView FindNodeView(HNode.Node node)
    {
        return GetNodeByGuid(node.GUID) as NodeView;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if(graphViewChange.elementsToRemove != null)
        {
            graphViewChange.elementsToRemove.ForEach(elem =>
            {
                var edge = elem as Edge;
                if(edge != null)
                {
                    var parentNodeView = edge.output.node as NodeView;
                    var childNodeView = edge.input.node as NodeView;
                    m_Tree.RemoveChild(parentNodeView.Node, childNodeView.Node);
                }
            });

            graphViewChange.elementsToRemove.ForEach(elem =>
            {
                if (elem is NodeView nodeView)
                    m_Tree.DeleteNode(nodeView.Node);
            });
        }

        if(graphViewChange.edgesToCreate != null)
        {
            graphViewChange.edgesToCreate.ForEach(edge =>
            {
                var parentNodeView = edge.output.node as NodeView;
                var childNodeView = edge.input.node as NodeView;
                m_Tree.AddChild(parentNodeView.Node, childNodeView.Node);
            });
        }

        return graphViewChange;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort =>
        endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
    }

    private bool CanUseType(Type type, HNode.Node replacing)
    {
        if (type != typeof(StartStageNode))
            return true;

        return m_Tree.StartNode == null || m_Tree.StartNode == replacing;
    }

    private void CreateNode(Type type, Vector2 pos)
    {
        if (IsOccupied(pos, null))
        {
            Debug.LogWarning($"[DungeonMap] 이미 노드가 있는 위치입니다. {pos}");
            return;
        }

        var node = m_Tree.CreateNode(type, pos);
        CreateNodeView(node);
    }

    private void ConvertNode(NodeView oldNodeView, System.Type newNodeType)
    {
        var node = m_Tree.CreateNode(newNodeType, oldNodeView.Node.Position);
        CreateNodeView(node);

        m_Tree.DeleteNode(oldNodeView.Node);
        RemoveEdges(oldNodeView);
        RemoveElement(oldNodeView);
    }

    private void DeleteNodeView(NodeView nodeView)
    {
        var elements = new List<GraphElement>();
        if (nodeView.Input != null)
            elements.AddRange(nodeView.Input.connections);
        if (nodeView.Output != null)
            elements.AddRange(nodeView.Output.connections);
        elements.Add(nodeView);
        DeleteElements(elements);
    }

    private Vector2 SnapPosition(Vector2 pos, HNode.Node moving)
    {
        float spacing = GlobalVariable.DUNGEON_MAP_NODE_SPACING;
        var snapped = new Vector2(Mathf.Floor(pos.x / spacing) * spacing, Mathf.Floor(pos.y / spacing) * spacing);

        var start = m_Tree.StartNode;
        float minX = start != null && start != moving ? start.Position.x : 0f;
        snapped.x = Mathf.Max(snapped.x, minX);

        if (moving != null && IsOccupied(snapped, moving))
            return moving.Position;

        return snapped;
    }

    private bool IsOccupied(Vector2 pos, HNode.Node except)
    {
        return m_Tree.Nodes.Any(n => n != except && n.Position == pos);
    }

    private NodeView CreateNodeView(HNode.Node node)
    {
        var nodeView = new NodeView(node);
        nodeView.OnNodeSelected = OnNodeSelected;
        nodeView.SnapPosition = pos => SnapPosition(pos, node);
        AddElement(nodeView);

        return nodeView;
    }

    private void RemoveEdges(NodeView nodeView)
    {
        var oldEdges = Enumerable.Empty<Edge>();
        if (nodeView.Input != null)
            oldEdges = oldEdges.Concat(nodeView.Input.connections);
        if (nodeView.Output != null)
            oldEdges = oldEdges.Concat(nodeView.Output.connections);
        DeleteElements(oldEdges);
    }
}
