using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// 목표 셀까지의 Flow Field 묶음. 필드 여러 장을 하나의 배열에 이어붙여 보관 (fieldIndex * CellCount 오프셋)
/// </summary>
public class FlowField : IDisposable
{
    public const byte Walkable = 1;
    public const byte Blocked = 255;
    public const int Unreachable = int.MaxValue;

    public NativeArray<int> Integration;
    public NativeArray<float2> Directions;
    public NativeArray<int> ExpandCounts; //필드별 마지막 계산의 셀 확장 횟수 (측정용)

    private readonly FlowGridInfo _grid;

    public FlowField(FlowGridInfo grid, int fieldCount)
    {
        _grid = grid;
        Integration = new NativeArray<int>(grid.CellCount * fieldCount, Allocator.Persistent);
        Directions = new NativeArray<float2>(grid.CellCount * fieldCount, Allocator.Persistent);
        ExpandCounts = new NativeArray<int>(fieldCount, Allocator.Persistent);
    }

    public int TotalExpandCount()
    {
        int total = 0;
        for (int i = 0; i < ExpandCounts.Length; i++)
        {
            total += ExpandCounts[i];
        }
        return total;
    }

    /// <summary>
    /// fieldIndex번 필드를 targetPos 기준으로 다시 계산
    /// </summary>
    public JobHandle Schedule(int fieldIndex, float3 targetPos,
        NativeArray<byte> cost, JobHandle dependency, bool usePriorityQueue)
    {
        int2 targetCell = math.clamp(_grid.CellOf(targetPos), 0, _grid.Size - 1);
        int offset = fieldIndex * _grid.CellCount;

        JobHandle handle;
        if (usePriorityQueue)
        {
            handle = new IntegrationFieldHeapJob
            {
                Cost = cost,
                Integration = Integration,
                Size = _grid.Size,
                Offset = offset,
                TargetIndex = _grid.Index(targetCell),
                ExpandCounts = ExpandCounts,
                FieldIndex = fieldIndex,
            }.Schedule(dependency);
        }
        else
        {
            handle = new IntegrationFieldJob
            {
                Cost = cost,
                Integration = Integration,
                Size = _grid.Size,
                Offset = offset,
                TargetIndex = _grid.Index(targetCell),
                ExpandCounts = ExpandCounts,
                FieldIndex = fieldIndex,
            }.Schedule(dependency);
        }
        var directionJob = new DirectionFieldJob
        {
            Cost = cost,
            Integration = Integration,
            Directions = Directions,
            GridSize = _grid.Size,
            Offset = offset,
        };

        return directionJob.Schedule(_grid.CellCount, 256, handle);
    }

    public void Dispose()
    {
        if (Integration.IsCreated)
            Integration.Dispose();
        if (Directions.IsCreated)
            Directions.Dispose();
        if (ExpandCounts.IsCreated)
            ExpandCounts.Dispose();
    }

    public static int2 Neighbour(int n)
    {
        switch (n)
        {
            case 0: return new int2(1, 0);
            case 1: return new int2(-1, 0);
            case 2: return new int2(0, 1);
            case 3: return new int2(0, -1);
            case 4: return new int2(1, 1);
            case 5: return new int2(1, -1);
            case 6: return new int2(-1, 1);
            default: return new int2(-1, -1);
        }
    }

    /// <summary>
    /// 대각선 이동은 인접한 두 직선 셀이 모두 걸을 수 있을 때만 허용 (벽 모서리 파고들기 방지)
    /// </summary>
    public static bool CanStep(NativeArray<byte> cost, int2 gridSize, int2 from, int2 dir)
    {
        int2 to = from + dir;
        if (to.x < 0 || to.y < 0 || to.x >= gridSize.x || to.y >= gridSize.y)
            return false;
        if (cost[to.y * gridSize.x + to.x] == Blocked)
            return false;
        if (dir.x != 0 && dir.y != 0)
        {
            if (cost[from.y * gridSize.x + from.x + dir.x] == Blocked)
                return false;
            if (cost[(from.y + dir.y) * gridSize.x + from.x] == Blocked)
                return false;
        }
        return true;
    }
}
