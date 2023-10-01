using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EnemyStat", menuName = "New Stat/EnemyStat",order = 1)]
public class EnemyStatData : ScriptableObject
{
    [Header("�ִ� ü��")]
    public int MaxHp;
    [Header("����")]
    public int Defense;
    [Header("ȸ��")]
    public int Dodge;
    [Header("����")]
    public int HitPercent;
    [Header("�޴� ��� ������ ���ҷ�")]
    public int ReduceDamage;
    [Header("�̵��ӵ�")]
    public int MoveSpeed;
    [Header("�⺻ ���ݷ�")]
    public int Attack;
}
