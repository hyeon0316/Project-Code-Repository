using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

[BurstCompile]
public struct ApplyTransformJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<quaternion> Rotations;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position = Positions[index];
        transform.rotation = Rotations[index];
    }
}
