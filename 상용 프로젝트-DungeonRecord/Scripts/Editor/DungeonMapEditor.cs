using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonMapEditor : EditorWindow
{
    private const string SAVE_FOLDER_PREFSNAME = "DungeonMapSaveFolder";
    private const string DEFAULT_SAVE_FOLDER = "Assets/SO/Dungeon";

    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

    private DungeonMapView m_MapView;
    private InspectorView m_InspectorView;
    private TextField m_AssetNameField;
    private TextField m_SaveFolderField;
    private DungeonMapTree m_SelectedTree;
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
        m_AssetNameField = root.Q<TextField>("AssetNameField");
        m_SaveFolderField = root.Q<TextField>("SaveFolderField");
        m_SaveFolderField.isReadOnly = true;
        m_SaveFolderField.value = EditorPrefs.GetString(SAVE_FOLDER_PREFSNAME, DEFAULT_SAVE_FOLDER);
        m_NodeViewLabel = root.Q<Label>("NodeViewLabel");
        m_NodeViewLabel.text = "Node View";

        var toolbarMenu = root.Q<ToolbarMenu>("ToolbarMenu");
        PopulateToolbarMenu(toolbarMenu);

        root.Q<Button>("SelectFolderButton").clicked += SelectSaveFolder;

        root.Q<Button>("GenerateButton").clicked += () =>
        {
            string folder = m_SaveFolderField.value;
            string assetName = m_AssetNameField.value;
            if (string.IsNullOrEmpty(assetName) || !AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogError($"[DungeonMap] Invalid asset name or folder. name: {assetName}, folder: {folder}");
                return;
            }

            string path = $"{folder}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                Debug.LogError($"[DungeonMap] Asset already exists. {path}");
                return;
            }

            //TODO: 여기에 다른 타입의 Tree추가 고려
            var tree = ScriptableObject.CreateInstance<DungeonMapTree>();
            AssetDatabase.CreateAsset(tree, path);
            tree.CreateNode(typeof(StartStageNode), Vector2.zero);

            SetMapView(tree);
            PopulateToolbarMenu(toolbarMenu);
        };

        OnSelectionChange();
    }

    private void SelectSaveFolder()
    {
        string selected = EditorUtility.OpenFolderPanel("Select Folder to save DungeonMapTree", m_SaveFolderField.value, "");
        if (string.IsNullOrEmpty(selected))
            return;

        string projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
        selected = selected.Replace('\\', '/');
        if (selected != Application.dataPath && !selected.StartsWith(Application.dataPath + "/"))
        {
            Debug.LogError($"[DungeonMap] Folder must be inside Assets. {selected}");
            return;
        }

        m_SaveFolderField.value = selected.Substring(projectRoot.Length);
        EditorPrefs.SetString(SAVE_FOLDER_PREFSNAME, m_SaveFolderField.value);
    }

    private void PopulateToolbarMenu(ToolbarMenu menu)
    {
        menu.menu.ClearItems();

        string[] guids = AssetDatabase.FindAssets("t:DungeonMapTree", new string[] { "Assets/SO/Dungeon" });
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
        m_NodeViewLabel.text = $"Node View - {tree.name}";
    }

    private void OnNodeSelectionChanged(NodeView nodeView)
    {
        m_InspectorView.UpdateSelection(nodeView.Node);
    }
}
