using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// 셀마다 누적 거리가 가장 작은 이웃 방향을 기록
/// </summary>
[BurstCompile]
public struct DirectionFieldJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<byte> Cost;
    [ReadOnly] public NativeArray<int> Integration;

    /// <summary>
    /// 한 배열에 여러 필드를 관리하므로 병렬 쓰기 제한 해제
    /// </summary>
    [NativeDisableParallelForRestriction] public NativeArray<float2> Directions;
    public int2 GridSize;
    public int Offset;

    public void Execute(int index)
    {
        int2 cell = new int2(index % GridSize.x, index / GridSize.x);
        int best = Integration[Offset + index];
        float2 dir = float2.zero;

        for (int n = 0; n < 8; n++)
        {
            int2 d = FlowField.Neighbour(n);
            if (!FlowField.CanStep(Cost, GridSize, cell, d))
                continue;

            int value = Integration[Offset + (cell.y + d.y) * GridSize.x + cell.x + d.x];
            if (value < best)
            {
                best = value;
                dir = math.normalize((float2)d);
            }
        }
        Directions[Offset + index] = dir;
    }
}
