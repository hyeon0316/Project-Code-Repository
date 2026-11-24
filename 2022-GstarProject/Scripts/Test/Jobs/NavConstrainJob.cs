using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

/// <summary>
/// NavMesh 표면을 따라 이동시켜 벽 통과 방지 + 높이 보정. NavMeshQuery는 단일 스레드 전용이라 IJob
/// </summary>
[BurstCompile]
public struct NavConstrainJob : IJob
{
    public NavMeshQuery Query;
    public NativeArray<NavMeshLocation> Locations;
    [ReadOnly] public NativeArray<Vector3> Targets;
    [ReadOnly] public NativeArray<int> AreaMasks;
    [WriteOnly] public NativeArray<float3> Positions;

    public void Execute()
    {
        Query.MoveLocations(Locations, Targets, AreaMasks);
        for (int i = 0; i < Locations.Length; i++)
        {
            Positions[i] = Locations[i].position;
        }
    }
}
