using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// 위치/회전 + 애니 파라미터로 인스턴싱 버퍼 원소 생성 (ApplyTransformJob 대체)
/// </summary>
[BurstCompile]
public struct BuildMatricesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<quaternion> Rotations;
    [ReadOnly] public NativeArray<float4> AnimParams;
    [WriteOnly] public NativeArray<EnemyInstanceData> Instances;
    public float4x4 MeshOffset; //프리팹 루트 기준 메시 위치/회전 보정

    public void Execute(int index)
    {
        Instances[index] = new EnemyInstanceData
        {
            Matrix = math.mul(float4x4.TRS(Positions[index], Rotations[index], new float3(1f)), MeshOffset),
            Anim = AnimParams[index],
        };
    }
}
