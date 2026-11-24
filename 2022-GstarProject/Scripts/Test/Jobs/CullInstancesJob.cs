using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// 카메라 절두체 안에 있는 인스턴스만 Visible에 압축 (순서는 보장하지 않음)
/// </summary>
[BurstCompile]
public struct CullInstancesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<EnemyInstanceData> Instances;
    [ReadOnly] public NativeArray<float4> FrustumPlanes; //xyz: 안쪽을 향한 법선, w: 거리
    public NativeList<EnemyInstanceData>.ParallelWriter Visible;
    public float3 SphereCenter; //메시 공간 경계구 중심
    public float SphereRadius; //월드 기준 반지름 (여유 포함)

    public void Execute(int index)
    {
        EnemyInstanceData instance = Instances[index];
        float3 center = math.transform(instance.Matrix, SphereCenter);
        for (int p = 0; p < 6; p++)
        {
            float4 plane = FrustumPlanes[p];
            if (math.dot(plane.xyz, center) + plane.w < -SphereRadius)
                return;
        }
        Visible.AddNoResize(instance);
    }
}
