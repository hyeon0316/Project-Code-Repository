using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전역 변수 모음
/// </summary>
public static class Global
{
    public static readonly int LoadingTime = 3;
    public static readonly string MissText = "Miss";
    public static readonly float PotionUseCondition = 0.7f;

    #region 애니메이션 관련
    public static readonly string SelectTrigger = "Select";
    public static readonly string BackTrigger = "Back";
    public static readonly string MoveBlend = "Move";

    public static readonly int MaxComboAttack = 4; // 기본공격의 총 콤보 수
    public static readonly int InitAttackCount = -1; // 기본공격의 처음단계 상태

    public static readonly int Idle = Animator.StringToHash("Idle");
    public static readonly int Walk = Animator.StringToHash("Walk");
    public static readonly int Attack = Animator.StringToHash("Attack");
    public static readonly int Attack1 = Animator.StringToHash("Attack1");
    public static readonly int Attack2 = Animator.StringToHash("Attack2");
    public static readonly int Attack3 = Animator.StringToHash("Attack3");
    public static readonly int Attack4 = Animator.StringToHash("Attack4");
    public static readonly int LongAttack = Animator.StringToHash("LongAttack");
    public static readonly int Scream = Animator.StringToHash("Scream");
    public static readonly int Dead = Animator.StringToHash("Dead");
    public static readonly int SpikeAttack = Animator.StringToHash("SpikeAttack");
    public static readonly int BulletRain = Animator.StringToHash("BulletRain");
    public static readonly int ChainLightning = Animator.StringToHash("ChainLightning");
    public static readonly int WindAttack = Animator.StringToHash("WindAttack");
    public static readonly int WideAreaBarrage = Animator.StringToHash("WideAreaBarrage");
    #endregion
}
