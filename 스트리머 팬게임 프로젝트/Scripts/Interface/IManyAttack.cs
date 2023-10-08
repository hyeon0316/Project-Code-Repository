using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 여러번 공격
/// </summary>
public interface IManyAttack
{
    IEnumerator AttackCo(BattleCharacter player, int damage);
}
