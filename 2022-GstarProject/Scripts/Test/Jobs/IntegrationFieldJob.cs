using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// 목표 셀에서 퍼져나가며 누적 거리 계산 (직선 10, 대각선 14)
/// </summary>
[BurstCompile]
public struct IntegrationFieldJob : IJob
{
    [ReadOnly] public NativeArray<byte> Cost;
    public NativeArray<int> Integration;
    public int2 Size;
    public int Offset;
    public int TargetIndex;
    public NativeArray<int> ExpandCounts;
    public int FieldIndex;

    public void Execute()
    {
        int cellCount = Size.x * Size.y;
        for (int i = 0; i < cellCount; i++)
        {
            Integration[Offset + i] = FlowField.Unreachable;
        }

        var open = new NativeQueue<int>(Allocator.Temp);
        Integration[Offset + TargetIndex] = 0;
        open.Enqueue(TargetIndex);

        int expandCount = 0;
        while (open.TryDequeue(out int index))
        {
            expandCount++;
            int2 cell = new int2(index % Size.x, index / Size.x);
            int curCost = Integration[Offset + index];

            for (int n = 0; n < 8; n++)
            {
                int2 dir = FlowField.Neighbour(n);
                if (!FlowField.CanStep(Cost, Size, cell, dir))
                    continue;

                int nextIndex = (cell.y + dir.y) * Size.x + cell.x + dir.x;
                int nextCost = curCost + (n < 4 ? 10 : 14); //0~3 직선
                if (nextCost < Integration[Offset + nextIndex])
                {
                    Integration[Offset + nextIndex] = nextCost;
                    open.Enqueue(nextIndex);
                }
            }
        }
        open.Dispose();
        ExpandCounts[FieldIndex] = expandCount;
    }
}
