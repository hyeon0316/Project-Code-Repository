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
    private List<List<NodeView>> m_NodeViews = new();

    public DungeonMapView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/DungeonMapEditor.uss");
        styleSheets.Add(styleSheet);
        //focusable = true;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        if (evt.target is NodeView nodeView)
        {
            var types = TypeCache.GetTypesDerivedFrom<StageNode>();
            foreach (var type in types)
            {
                if (nodeView.Node.GetType() == type)
                    continue;
                evt.menu.AppendAction($"Convert to {type.Name}", a => ConvertNode(nodeView, type));
            }
        }
        else
        {
            evt.menu.ClearItems();
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

        if (m_NodeViews.Count == 0)
        {
            for (int i = 0; i < m_Tree.RowCount; i++)
            {
                var row = new List<NodeView>();
                for (int j = 0; j < m_Tree.ColCount; j++)
                {
                    row.Add(null);
                }
                m_NodeViews.Add(row);
            }
        }

        for (int i = 0; i < m_Tree.Nodes.Count; i++)
        {
            var node = m_Tree.Nodes[i];
            var nodeView = CreateNodeView(node, new Vector2(node.ColIndex * GlobalVariable.DUNGEON_MAP_NODE_SPACING,
                        node.RowIndex * GlobalVariable.DUNGEON_MAP_NODE_SPACING));

            m_NodeViews[node.RowIndex][node.ColIndex] = nodeView;
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

    private void ConvertNode(NodeView oldNodeView, System.Type newNodeType)
    {
        var node = m_Tree.CreateNode(newNodeType, oldNodeView.Node.RowIndex, oldNodeView.Node.ColIndex);
        CreateNodeView(node, oldNodeView.Node.Position);

        m_Tree.DeleteNode(oldNodeView.Node);
        RemoveEdges(oldNodeView);
        RemoveElement(oldNodeView);
    }

    public void InitNode(System.Type type, int rowCount, int colCount)
    {
        m_NodeViews.Clear();

        m_Tree.RowCount = rowCount;
        m_Tree.ColCount = colCount;

        for (int i = 0; i < rowCount; i++)
        {
            var nodeViewRow = new List<NodeView>();
            for (int j = 0; j < colCount; j++)
            {
                var node = m_Tree.CreateNode(type, i, j);
                var nodeView = CreateNodeView(node, 
                    new Vector2(j * GlobalVariable.DUNGEON_MAP_NODE_SPACING, i * GlobalVariable.DUNGEON_MAP_NODE_SPACING));
                nodeViewRow.Add(nodeView);
            }
            m_NodeViews.Add(nodeViewRow);
        }
    }

    private NodeView CreateNodeView(HNode.Node node, Vector2 pos = default)
    {
        var nodeView = new NodeView(node, pos);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);

        return nodeView;
    }

    public void AddRow()
    {
        var newNodeViewRow = new List<NodeView>();

        int rowCount = m_Tree.RowCount;
        int colCount = m_Tree.ColCount;
        for (int i = 0; i < colCount; i++)
        {
            var node = m_Tree.CreateNode(typeof(PassageNode), rowCount, i);
            var nodeView = CreateNodeView(node,
                new Vector2((i * GlobalVariable.DUNGEON_MAP_NODE_SPACING), rowCount * GlobalVariable.DUNGEON_MAP_NODE_SPACING));

            newNodeViewRow.Add(nodeView);
        }

        m_NodeViews.Add(newNodeViewRow);
        m_Tree.RowCount++;
    }

    public void RemoveRow()
    {
        int rowCount = m_Tree.RowCount;
        int colCount = m_Tree.ColCount;

        if (rowCount <= 1) return;

        var lastRow = m_NodeViews[rowCount - 1];

        for (int i = 0; i < lastRow.Count; i++)
        {
            RemoveEdges(lastRow[i]);
            RemoveElement(lastRow[i]);
            m_Tree.DeleteNode(lastRow[i].Node);
        }

        m_NodeViews.RemoveAt(rowCount - 1);
        m_Tree.RowCount--;
    }

    public void AddColumn()
    {
        int rowCount = m_Tree.RowCount;
        int colCount = m_Tree.ColCount;

        for (int i = 0; i < rowCount; i++)
        {
            var node = m_Tree.CreateNode(typeof(PassageNode), i , colCount);
            var nodeView = CreateNodeView(node,
                new Vector2(colCount * GlobalVariable.DUNGEON_MAP_NODE_SPACING, i * GlobalVariable.DUNGEON_MAP_NODE_SPACING));
            m_NodeViews[i].Add(nodeView);
        }

        m_Tree.ColCount++;
    }

    public void RemoveColumn()
    {
        int rowCount = m_Tree.RowCount;
        int colCount = m_Tree.ColCount;

        if (colCount <= 1) return;

        for (int i = 0; i < rowCount; i++)
        {
            var nodeView = m_NodeViews[i][colCount - 1];
            m_Tree.DeleteNode(nodeView.Node);
            RemoveEdges(nodeView);
            RemoveElement(nodeView);
            m_NodeViews[i].RemoveAt(colCount - 1);
        }
        m_Tree.ColCount--;
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
