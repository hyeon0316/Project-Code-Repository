using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// SkinnedMesh 애니메이션을 프레임별 정점 위치/노멀 텍스처(VAT)로 굽는 도구
/// </summary>
public class VatBaker : EditorWindow
{
    private const int MaxTexWidth = 4096;

    [SerializeField] private GameObject _prefab;
    [SerializeField] private Avatar _avatar; //Humanoid 클립이면 필요
    [SerializeField] private AnimationClip _idle;
    [SerializeField] private AnimationClip _walk;
    [SerializeField] private AnimationClip _attack;
    [SerializeField] private int _fps = 30;
    [SerializeField] private string _outputFolder = "Assets/Scripts/Test/VatBaked";

    [MenuItem("Tools/VAT Baker")]
    private static void Open()
    {
        GetWindow<VatBaker>("VAT Baker");
    }

    private void OnGUI()
    {
        _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);
        _avatar = (Avatar)EditorGUILayout.ObjectField("Avatar (선택)", _avatar, typeof(Avatar), false);
        _idle = (AnimationClip)EditorGUILayout.ObjectField("Idle", _idle, typeof(AnimationClip), false);
        _walk = (AnimationClip)EditorGUILayout.ObjectField("Walk", _walk, typeof(AnimationClip), false);
        _attack = (AnimationClip)EditorGUILayout.ObjectField("Attack", _attack, typeof(AnimationClip), false);
        _fps = EditorGUILayout.IntSlider("Sample FPS", _fps, 5, 60);
        _outputFolder = EditorGUILayout.TextField("Output Folder", _outputFolder);

        GUI.enabled = _prefab != null && _idle != null && _walk != null && _attack != null;
        if (GUILayout.Button("Bake"))
        {
            Bake();
        }
        GUI.enabled = true;
    }

    private void Bake()
    {
        AnimationClip[] clips = { _idle, _walk, _attack }; //VatAnimData.IdleClip/WalkClip/AttackClip 순서

        GameObject instance = Instantiate(_prefab);
        try
        {
            SkinnedMeshRenderer skinned = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinned == null)
            {
                Debug.LogError("[VatBaker] 프리팹에 SkinnedMeshRenderer가 없습니다.");
                return;
            }

            Animator animator = instance.GetComponent<Animator>();
            if (animator == null)
            {
                animator = instance.AddComponent<Animator>();
            }
            if (_avatar != null)
            {
                animator.avatar = _avatar;
            }
            animator.applyRootMotion = false; //제자리 애니메이션으로 굽기 (이동은 Job이 담당)

            int vertexCount = skinned.sharedMesh.vertexCount;
            int texWidth = Mathf.Min(vertexCount, MaxTexWidth);
            int rowsPerFrame = Mathf.CeilToInt(vertexCount / (float)texWidth);

            var clipInfos = new VatAnimData.Clip[clips.Length];
            int totalFrames = 0;
            for (int c = 0; c < clips.Length; c++)
            {
                int frames = Mathf.Max(1, Mathf.CeilToInt(clips[c].length * _fps));
                clipInfos[c] = new VatAnimData.Clip { StartFrame = totalFrames, FrameCount = frames, Fps = _fps, Loop = true };
                totalFrames += frames;
            }

            int texHeight = totalFrames * rowsPerFrame;
            var positions = new Color[texWidth * texHeight];
            var normals = new Color[texWidth * texHeight];

            PlayableGraph graph = PlayableGraph.Create("VatBake");
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Output", animator);
            var baked = new Mesh();

            for (int c = 0; c < clips.Length; c++)
            {
                AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clips[c]);
                output.SetSourcePlayable(playable);

                for (int f = 0; f < clipInfos[c].FrameCount; f++)
                {
                    playable.SetTime(f / (float)_fps);
                    graph.Evaluate();
                    skinned.BakeMesh(baked);

                    Vector3[] verts = baked.vertices;
                    Vector3[] norms = baked.normals;
                    int frameRow = (clipInfos[c].StartFrame + f) * rowsPerFrame;
                    for (int v = 0; v < vertexCount; v++)
                    {
                        int pixel = (frameRow + v / texWidth) * texWidth + v % texWidth;
                        positions[pixel] = new Color(verts[v].x, verts[v].y, verts[v].z, 1f);
                        normals[pixel] = new Color(norms[v].x, norms[v].y, norms[v].z, 0f);
                    }
                }
                playable.Destroy();
            }

            graph.Destroy();
            DestroyImmediate(baked);

            SaveAssets(clipInfos, positions, normals, vertexCount, texWidth, texHeight, rowsPerFrame);
        }
        finally
        {
            DestroyImmediate(instance);
        }
    }

    private void SaveAssets(VatAnimData.Clip[] clips, Color[] positions, Color[] normals,
        int vertexCount, int texWidth, int texHeight, int rowsPerFrame)
    {
        Directory.CreateDirectory(_outputFolder);
        string baseName = _prefab.name;

        Texture2D positionTex = CreateTexture(positions, texWidth, texHeight, $"{_outputFolder}/{baseName}_VatPosition.asset");
        Texture2D normalTex = CreateTexture(normals, texWidth, texHeight, $"{_outputFolder}/{baseName}_VatNormal.asset");

        var data = CreateInstance<VatAnimData>();
        data.PositionTex = positionTex;
        data.NormalTex = normalTex;
        data.VertexCount = vertexCount;
        data.TexWidth = texWidth;
        data.RowsPerFrame = rowsPerFrame;
        data.Clips = clips;
        AssetDatabase.CreateAsset(data, $"{_outputFolder}/{baseName}_VatData.asset");
        AssetDatabase.SaveAssets();

        long bytes = (long)texWidth * texHeight * 8 * 2;
        Debug.Log($"[VatBaker] 완료: 정점 {vertexCount}, 텍스처 {texWidth}x{texHeight} x2, 메모리 {bytes / 1024f / 1024f:F2} MB");
        Selection.activeObject = data;
    }

    private static Texture2D CreateTexture(Color[] pixels, int width, int height, string path)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBAHalf, false, true)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        tex.SetPixels(pixels);
        tex.Apply(false, false);
        AssetDatabase.CreateAsset(tex, path);
        return tex;
    }
}
