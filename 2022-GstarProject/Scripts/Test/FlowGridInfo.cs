using System;
using Unity.Mathematics;

/// <summary>
/// XZ 평면 격자 (높이 무시)
/// </summary>
[Serializable]
public struct FlowGridInfo
{
    public float3 Origin;   //격자 좌하단 모서리
    public float CellSize;
    public int2 Size;

    public int CellCount => Size.x * Size.y;

    public int2 CellOf(float3 pos)
    {
        return (int2)math.floor((pos.xz - Origin.xz) / CellSize);
    }

    public bool InBounds(int2 cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < Size.x && cell.y < Size.y;
    }

    public int Index(int2 cell)
    {
        return cell.y * Size.x + cell.x;
    }

    public float3 CellCenter(int2 cell)
    {
        return new float3(Origin.x + (cell.x + 0.5f) * CellSize, Origin.y, Origin.z + (cell.y + 0.5f) * CellSize);
    }
}
