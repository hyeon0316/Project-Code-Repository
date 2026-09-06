using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonMapEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

    private DungeonMapView m_MapView;
    private InspectorView m_InspectorView;
    private IntegerField m_RowsField;
    private IntegerField m_ColumnsField;
    private TextField m_AssetNameField;
    private DungeonMapTree m_SelectedTree;
    private VisualElement m_EditButtonParent;
    private Label m_NodeViewLabel;

    [MenuItem("Tools/DungeonMapEditor")]
    public static void OpenWindow()
    {
        DungeonMapEditor wnd = GetWindow<DungeonMapEditor>();
        wnd.titleContent = new GUIContent("DungeonMapEditor");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        m_VisualTreeAsset.CloneTree(root);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/DungeonMapEditor.uss");
        root.styleSheets.Add(styleSheet);

        m_MapView = root.Q<DungeonMapView>();
        m_MapView.OnNodeSelected = OnNodeSelectionChanged;
        m_InspectorView = root.Q<InspectorView>();
        m_RowsField = root.Q<IntegerField>("RowsField");
        m_ColumnsField = root.Q<IntegerField>("ColumnsField");
        m_AssetNameField = root.Q<TextField>("AssetNameField");
        m_EditButtonParent = root.Q<VisualElement>("EditButtons");
        m_EditButtonParent.style.display = DisplayStyle.None;
        m_NodeViewLabel = root.Q<Label>("NodeViewLabel");
        m_NodeViewLabel.text = "Node View";

        var toolbarMenu = root.Q<ToolbarMenu>("ToolbarMenu");
        PopulateToolbarMenu(toolbarMenu);

        root.Q<Button>("GenerateButton").clicked += () =>
        {
            //TODO: 여기에 다른 타입의 Tree추가 고려
            var tree = ScriptableObject.CreateInstance<DungeonMapTree>();
            m_MapView.PopulateView(tree);
            AssetDatabase.CreateAsset(tree, $"Assets/SO/Dungeon/Map/{m_AssetNameField.value}.asset");
            AssetDatabase.SaveAssets();

            m_MapView.InitNode(typeof(PassageNode), m_RowsField.value, m_ColumnsField.value);
            PopulateToolbarMenu(toolbarMenu);
            m_EditButtonParent.style.display = DisplayStyle.Flex;
        };

        root.Q<Button>("AddRow").clicked += () => { m_MapView.AddRow(); };
        root.Q<Button>("RemoveRow").clicked += () => { m_MapView.RemoveRow(); };
        root.Q<Button>("AddColumn").clicked += () => { m_MapView.AddColumn(); };
        root.Q<Button>("RemoveColumn").clicked += () => { m_MapView.RemoveColumn(); };
        OnSelectionChange();
    }

    private void PopulateToolbarMenu(ToolbarMenu menu)
    {
        menu.menu.ClearItems();

        string[] guids = AssetDatabase.FindAssets("t:DungeonMapTree", new string[] { "Assets/SO/Dungeon/Tree" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<DungeonMapTree>(path);
            if (so != null)
            {
                menu.menu.AppendAction(so.name, a => OnToolbarMenuSelected(so));
            }
        }
    }

    private void OnToolbarMenuSelected(DungeonMapTree so)
    {
        m_SelectedTree = so;
        SetMapView(so);
    }

    private void OnSelectionChange()
    {
        var tree = m_SelectedTree != null ? m_SelectedTree : Selection.activeObject as DungeonMapTree;
        if (tree != null)
        {
            SetMapView(tree);
        }
    }

    private void SetMapView(DungeonMapTree tree)
    {
        m_MapView.PopulateView(tree);
        m_EditButtonParent.style.display = DisplayStyle.Flex;
        m_NodeViewLabel.text = $"Node View - {tree.name}";
    }

    private void OnNodeSelectionChanged(NodeView nodeView)
    {
        m_InspectorView.UpdateSelection(nodeView.Node);
    }
}
