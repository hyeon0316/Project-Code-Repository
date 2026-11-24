using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// NavMesh를 쓰지 않을 때 DesiredPositions를 그대로 반영
/// </summary>
[BurstCompile]
public struct CopyPositionsJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector3> Source;
    [WriteOnly] public NativeArray<float3> Positions;

    public void Execute(int index)
    {
        Positions[index] = Source[index];
    }
}
