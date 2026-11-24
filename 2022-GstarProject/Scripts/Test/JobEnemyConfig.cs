using System;


[Serializable]
public struct JobEnemyConfig
{
    public float MoveSpeed;
    public float AttackRadius;
    public float AttackCooldown;
    public float SeparationRadius;
    [UnityEngine.Header("스폰지역 중심에서 이 거리를 넘으면 복귀")]
    public float BackDistance;
    [UnityEngine.Header("배회 목표에 도달하지 못할 때 포기하는 시간")]
    public float StuckTimeOut;
    public int MaxHp;

    public static JobEnemyConfig Default => new JobEnemyConfig
    {
        MoveSpeed = 3f,
        AttackRadius = 1.5f,
        AttackCooldown = 1.5f,
        SeparationRadius = 0.8f,
        BackDistance = 30f,
        StuckTimeOut = 8f,
        MaxHp = 100,
    };
}
