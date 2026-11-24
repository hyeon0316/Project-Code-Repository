using Unity.Mathematics;

/// <summary>
/// 인스턴싱 버퍼 원소 1개 (80B). 셰이더의 InstanceData와 레이아웃이 같아야 함
/// </summary>
public struct EnemyInstanceData
{
    public const int Stride = sizeof(float) * 20;

    public float4x4 Matrix;
    public float4 Anim; //x: 시작 프레임, y: 프레임 수, z: 현재 프레임(클립 기준), w: 루프 여부(1/0)
}
