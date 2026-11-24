using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// 상태를 VAT 클립으로 매핑하고 재생 시간 진행. 상태가 바뀌면 처음부터 재생
/// </summary>
[BurstCompile]
public struct AnimStateJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<JobEnemyState> States;
    public NativeArray<JobEnemyState> PrevStates;
    public NativeArray<float> AnimTime;
    [WriteOnly] public NativeArray<float4> AnimParams;

    public VatAnimData.Clip Idle;
    public VatAnimData.Clip Walk;
    public VatAnimData.Clip Attack;
    public float DeltaTime;

    public void Execute(int i)
    {
        JobEnemyState state = States[i];
        float time = state == PrevStates[i] ? AnimTime[i] + DeltaTime : 0f;
        PrevStates[i] = state;
        AnimTime[i] = time;

        VatAnimData.Clip clip;
        switch (state)
        {
            case JobEnemyState.Wander:
            case JobEnemyState.Chase:
            case JobEnemyState.Return:
                clip = Walk;
                break;
            case JobEnemyState.Attack:
                clip = Attack;
                break;
            default: //Idle, Dead (Dead는 같은 프레임에 리스폰되어 보이지 않음)
                clip = Idle;
                break;
        }

        float frame = time * clip.Fps;
        frame = clip.Loop ? frame % clip.FrameCount : math.min(frame, clip.FrameCount - 1);
        AnimParams[i] = new float4(clip.StartFrame, clip.FrameCount, frame, clip.Loop ? 1f : 0f);
    }
}
