using Unity.Mathematics;

public struct JobSpawnArea
{
    public float3 Center;
    public float3 HalfExtents;
    public float BackDistance;
    public int FieldIndex; //복귀용 Flow Field 인덱스

    public bool Contains(float3 pos)
    {
        float2 d = math.abs(pos.xz - Center.xz);
        return d.x <= HalfExtents.x && d.y <= HalfExtents.z;
    }
}
