using System;
using UnityEngine;

/// <summary>
/// VatBaker가 생성하는 VAT 메타데이터. 텍스처 한 행에 정점 TexWidth개, 프레임 하나가 RowsPerFrame행을 차지
/// </summary>
public class VatAnimData : ScriptableObject
{
    public const int IdleClip = 0;
    public const int WalkClip = 1;
    public const int AttackClip = 2;

    [Serializable]
    public struct Clip
    {
        public int StartFrame;
        public int FrameCount;
        public float Fps;
        public bool Loop;
    }

    public Texture2D PositionTex;
    public Texture2D NormalTex;
    public int VertexCount;
    public int TexWidth;
    public int RowsPerFrame;
    public Clip[] Clips; //IdleClip, WalkClip, AttackClip 순서
}
