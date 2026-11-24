using Unity.Mathematics;

public static class SpatialHash
{
    public static int Key(float3 pos, float cellSize)
    {
        return Key((int2)math.floor(pos.xz / cellSize));
    }

    public static int Key(int2 cell)
    {
        return (int)math.hash(cell);
    }
}
