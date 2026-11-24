using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct BuildSpatialHashJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> Positions;
    public NativeMultiHashMap<int, int>.ParallelWriter Hash;
    public float CellSize;

    public void Execute(int index)
    {
        Hash.Add(SpatialHash.Key(Positions[index], CellSize), index);
    }
}
