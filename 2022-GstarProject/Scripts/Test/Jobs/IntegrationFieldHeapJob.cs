using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// IntegrationFieldJob의 우선순위 큐(이진 최소 힙) 버전. 비용이 가장 작은 셀부터 확장하는 다익스트라
/// </summary>
[BurstCompile]
public struct IntegrationFieldHeapJob : IJob
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

        var heap = new NativeList<int2>(cellCount, Allocator.Temp); //x: 비용, y: 셀 인덱스
        Integration[Offset + TargetIndex] = 0;
        Push(heap, new int2(0, TargetIndex));

        int expandCount = 0;
        while (heap.Length > 0)
        {
            int2 top = Pop(heap);
            int index = top.y;
            int curCost = top.x;
            if (curCost > Integration[Offset + index]) //더 짧은 값으로 이미 갱신된 오래된 항목
                continue;

            expandCount++;
            int2 cell = new int2(index % Size.x, index / Size.x);
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
                    Push(heap, new int2(nextCost, nextIndex));
                }
            }
        }
        heap.Dispose();
        ExpandCounts[FieldIndex] = expandCount;
    }

    private static void Push(NativeList<int2> heap, int2 item)
    {
        heap.Add(item);
        int i = heap.Length - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[parent].x <= item.x)
                break;
            heap[i] = heap[parent];
            i = parent;
        }
        heap[i] = item;
    }

    private static int2 Pop(NativeList<int2> heap)
    {
        int2 top = heap[0];
        int2 last = heap[heap.Length - 1];
        heap.RemoveAt(heap.Length - 1);
        int count = heap.Length;
        if (count == 0)
            return top;

        int i = 0;
        while (true)
        {
            int child = i * 2 + 1;
            if (child >= count)
                break;
            if (child + 1 < count && heap[child + 1].x < heap[child].x)
                child++;
            if (last.x <= heap[child].x)
                break;
            heap[i] = heap[child];
            i = child;
        }
        heap[i] = last;
        return top;
    }
}
