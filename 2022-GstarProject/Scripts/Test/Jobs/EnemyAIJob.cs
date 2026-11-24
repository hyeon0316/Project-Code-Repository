using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// 개체별 상태 머신 + 조향. 결과는 DesiredPositions에 기록 (Positions는 이웃 탐색용으로 읽기 전용)
/// </summary>
[BurstCompile]
public struct EnemyAIJob : IJobParallelFor
{
    private const int MaxSeparationNeighbours = 16;
    private const int WanderPickTries = 8;

    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeMultiHashMap<int, int> Hash;
    [ReadOnly] public NativeArray<JobSpawnArea> Areas;
    [ReadOnly] public NativeArray<int> AreaIndex;
    [ReadOnly] public NativeArray<byte> Cost;
    [ReadOnly] public NativeArray<float2> ChaseDirections;
    [ReadOnly] public NativeArray<int> ChaseIntegration;
    [ReadOnly] public NativeArray<float2> ReturnDirections;

    public NativeArray<JobEnemyState> States;
    public NativeArray<float> Timers;
    public NativeArray<float3> Targets;
    public NativeArray<int> Hp;
    public NativeArray<Random> Randoms;
    public NativeArray<quaternion> Rotations;
    [WriteOnly] public NativeArray<Vector3> DesiredPositions;
    public NativeQueue<int>.ParallelWriter DeadQueue;

    public FlowGridInfo Grid;
    public JobEnemyConfig Config;
    public float HashCellSize;
    public float3 PlayerPos;
    public float DeltaTime;

    public void Execute(int i)
    {
        float3 pos = Positions[i];
        JobSpawnArea area = Areas[AreaIndex[i]];
        Random rnd = Randoms[i];
        JobEnemyState state = States[i];
        float timer = Timers[i] - DeltaTime;
        float3 target = Targets[i];
        float2 moveDir = float2.zero;
        float2 lookDir = float2.zero;

        int2 cell = Grid.CellOf(pos);
        bool inGrid = Grid.InBounds(cell);
        int cellIndex = inGrid ? Grid.Index(cell) : 0;

        if (Hp[i] <= 0 && state != JobEnemyState.Dead)
        {
            state = JobEnemyState.Dead;
            DeadQueue.Enqueue(i);
        }

        switch (state)
        {
            case JobEnemyState.Idle:
                if (timer <= 0f)
                {
                    if (rnd.NextBool())
                    {
                        timer = rnd.NextInt(2, 6);
                    }
                    else
                    {
                        target = PickWanderTarget(area, ref rnd);
                        timer = Config.StuckTimeOut;
                        state = JobEnemyState.Wander;
                    }
                }
                break;

            case JobEnemyState.Wander:
                {
                    float2 to = target.xz - pos.xz;
                    if (math.lengthsq(to) <= 0.25f || timer <= 0f)
                    {
                        state = JobEnemyState.Idle;
                        timer = 1f;
                    }
                    else
                    {
                        moveDir = math.normalize(to);
                    }
                    break;
                }

            case JobEnemyState.Chase:
                {
                    float2 toPlayer = PlayerPos.xz - pos.xz;
                    float dist = math.length(toPlayer);
                    bool tooFar = math.distance(pos.xz, area.Center.xz) > area.BackDistance;
                    bool unreachable = !inGrid || ChaseIntegration[cellIndex] == FlowField.Unreachable;

                    if (tooFar || unreachable)
                    {
                        state = JobEnemyState.Return;
                    }
                    else if (dist <= Config.AttackRadius)
                    {
                        state = JobEnemyState.Attack;
                        timer = Config.AttackCooldown;
                        lookDir = toPlayer;
                    }
                    else
                    {
                        moveDir = dist < Grid.CellSize * 2f ? toPlayer / dist : ChaseDirections[cellIndex];
                        if (math.all(moveDir == float2.zero))
                        {
                            moveDir = toPlayer / dist;
                        }
                    }
                    break;
                }

            case JobEnemyState.Attack:
                lookDir = PlayerPos.xz - pos.xz;
                if (timer <= 0f)
                {
                    state = JobEnemyState.Chase;
                }
                break;

            case JobEnemyState.Return:
                if (area.Contains(pos))
                {
                    Hp[i] = Config.MaxHp;
                    state = JobEnemyState.Idle;
                    timer = 1f;
                }
                else
                {
                    moveDir = inGrid ? ReturnDirections[area.FieldIndex * Grid.CellCount + cellIndex] : float2.zero;
                    if (math.all(moveDir == float2.zero))
                    {
                        moveDir = math.normalizesafe(area.Center.xz - pos.xz);
                    }
                }
                break;
        }

        float2 velocity = moveDir * Config.MoveSpeed;
        if (state != JobEnemyState.Dead)
        {
            velocity += Separation(i, pos) * Config.MoveSpeed;
        }

        if (math.all(lookDir == float2.zero))
        {
            lookDir = moveDir;
        }
        if (math.lengthsq(lookDir) > 0.0001f)
        {
            Rotations[i] = quaternion.LookRotationSafe(new float3(lookDir.x, 0f, lookDir.y), math.up());
        }

        DesiredPositions[i] = pos + new float3(velocity.x, 0f, velocity.y) * DeltaTime;
        States[i] = state;
        Timers[i] = timer;
        Targets[i] = target;
        Randoms[i] = rnd;
    }

    /// <summary>
    /// 걸을 수 있는 셀이 나올 때까지 영역 안에서 랜덤 지점 선택
    /// </summary>
    private float3 PickWanderTarget(JobSpawnArea area, ref Random rnd)
    {
        float3 candidate = area.Center;
        for (int t = 0; t < WanderPickTries; t++)
        {
            float2 offset = rnd.NextFloat2(-area.HalfExtents.xz, area.HalfExtents.xz);
            candidate = area.Center + new float3(offset.x, 0f, offset.y);
            int2 cell = Grid.CellOf(candidate);
            if (Grid.InBounds(cell) && Cost[Grid.Index(cell)] != FlowField.Blocked)
            {
                break;
            }
        }
        return candidate;
    }

    /// <summary>
    /// 3x3 해시 셀 안의 가까운 개체로부터 밀어내는 방향
    /// </summary>
    private float2 Separation(int self, float3 pos)
    {
        float2 push = float2.zero;
        float radius = Config.SeparationRadius;
        int2 baseCell = (int2)math.floor(pos.xz / HashCellSize);
        int found = 0;

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                int key = SpatialHash.Key(baseCell + new int2(x, y));
                if (!Hash.TryGetFirstValue(key, out int other, out var it)) continue;
                do
                {
                    if (other == self) continue;
                    float2 diff = pos.xz - Positions[other].xz;
                    float distSq = math.lengthsq(diff);
                    if (distSq >= radius * radius || distSq < 0.000001f) continue;

                    float dist = math.sqrt(distSq);
                    push += diff / dist * (1f - dist / radius);
                    if (++found >= MaxSeparationNeighbours) return push;
                }
                while (Hash.TryGetNextValue(out other, ref it));
            }
        }
        return push;
    }
}
