using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;


[BurstCompile]
public struct BuildCostFieldJob : IJob
{
    public NavMeshQuery Query;
    public FlowGridInfo Grid;
    public float HeightExtent;
    public int AgentTypeId;
    [WriteOnly] public NativeArray<byte> Cost;

    public void Execute()
    {
        Vector3 extents = new Vector3(Grid.CellSize * 0.5f, HeightExtent, Grid.CellSize * 0.5f);
        for (int y = 0; y < Grid.Size.y; y++)
        {
            for (int x = 0; x < Grid.Size.x; x++)
            {
                int2 cell = new int2(x, y);
                NavMeshLocation location = Query.MapLocation(Grid.CellCenter(cell), extents, AgentTypeId);
                Cost[Grid.Index(cell)] = Query.IsValid(location) ? FlowField.Walkable : FlowField.Blocked;
            }
        }
    }
}
