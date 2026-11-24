using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 개체 GameObject 없이 DrawMeshInstancedProcedural 한 번으로 전체를 그림
/// </summary>
public class InstancedEnemyRenderer : IDisposable
{
    private static readonly int InstancesId = Shader.PropertyToID("_Instances");
    private static readonly int VatPositionTexId = Shader.PropertyToID("_VatPositionTex");
    private static readonly int VatNormalTexId = Shader.PropertyToID("_VatNormalTex");
    private static readonly int VatInfoId = Shader.PropertyToID("_VatInfo");

    public float4x4 MeshOffset { get; }
    public float3 CullSphereCenter { get; } //메시 공간 기본 포즈 경계구 중심
    public float CullSphereRadius { get; } //MeshOffset 스케일을 반영한 반지름 (여유 미포함)

    private readonly Mesh _mesh;
    private readonly Material _material;
    private readonly ComputeBuffer _buffer;
    private readonly Bounds _bounds;

    /// <param name="vatData">null이면 정적 메시(기본 포즈)로 그림</param>
    public InstancedEnemyRenderer(GameObject prefab, Material material, int count, Bounds bounds, VatAnimData vatData)
    {
        _material = material;
        _bounds = bounds;

        //프리팹을 임시로 생성해 기본 포즈를 정적 메시로 굽고, 루트 기준 메시 위치/회전을 저장
        GameObject temp = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        Transform meshTransform;
        SkinnedMeshRenderer skinned = temp.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinned != null)
        {
            _mesh = new Mesh();
            skinned.BakeMesh(_mesh);
            meshTransform = skinned.transform;
        }
        else
        {
            MeshFilter filter = temp.GetComponentInChildren<MeshFilter>();
            _mesh = Object.Instantiate(filter.sharedMesh);
            meshTransform = filter.transform;
        }
        MeshOffset = temp.transform.worldToLocalMatrix * meshTransform.localToWorldMatrix;
        Object.Destroy(temp);

        Bounds meshBounds = _mesh.bounds;
        float3 scale = new float3(math.length(MeshOffset.c0.xyz), math.length(MeshOffset.c1.xyz), math.length(MeshOffset.c2.xyz));
        CullSphereCenter = meshBounds.center;
        CullSphereRadius = meshBounds.extents.magnitude * math.cmax(scale);

        if (!_material.enableInstancing)
        {
            Debug.LogWarning("[InstancedEnemyRenderer] 머티리얼의 Enable GPU Instancing을 켜주세요.");
        }

        _buffer = new ComputeBuffer(count, EnemyInstanceData.Stride);
        _material.SetBuffer(InstancesId, _buffer);

        if (vatData != null)
        {
            if (vatData.VertexCount != _mesh.vertexCount)
            {
                Debug.LogWarning($"[InstancedEnemyRenderer] VAT 정점 수({vatData.VertexCount})와 메시 정점 수({_mesh.vertexCount})가 다릅니다. 같은 프리팹으로 다시 구워주세요.");
            }
            _material.SetTexture(VatPositionTexId, vatData.PositionTex);
            _material.SetTexture(VatNormalTexId, vatData.NormalTex);
            _material.SetVector(VatInfoId, new Vector4(vatData.TexWidth, vatData.RowsPerFrame,
                1f / vatData.PositionTex.width, 1f / vatData.PositionTex.height));
        }
    }

    /// <param name="count">앞에서부터 그릴 개수 (컬링 후 압축된 개수)</param>
    public void Draw(NativeArray<EnemyInstanceData> instances, int count)
    {
        if (count <= 0)
            return;
        _buffer.SetData(instances, 0, 0, count);
        Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, _bounds, count);
    }

    public void Dispose()
    {
        _buffer.Release();
        Object.Destroy(_mesh);
    }
}
