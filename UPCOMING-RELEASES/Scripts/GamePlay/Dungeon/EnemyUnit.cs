using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class EnemyUnit : DungeonUnit
{
    public static AssetID AssetID = new("Assets/Prefabs/Character/Enemy.prefab");
    public int ExecuteSkillIndex { get; private set; }
    public EEnemyType Type { get; private set; }

    public void Init(string id, DungeonCharacterRecord record = null)
    {
        ResetState();
        SetHighLight(false);
        ID = id;
        string typeStr = id.Substring(0, id.Length - 1);
        var type = Enum.Parse<EEnemyType>(typeStr);
        var baseInfo = SODatabase.GetEnemyBaseInfo(type);
        State = new Enemy(type, ContentsManager.Instance.Get<DungeonContents>().CurScalingID);
        Name = baseInfo.Name;
        CurHp = Mathf.RoundToInt(State.MaxHp.ResultValue);
        SetFacingRight(false);
        Animator.SetController(baseInfo.DungeonAnimator);
        InitPassiveSkill(baseInfo.PassiveSkillGroup);
        InitActiveSkill(baseInfo.ActiveSkills);
        ApplyOnBattleStartPassive();
        if (record != null)
        {
            CurHp = record.Hp;
            TransformIndex = record.TransformIndex;
        }
        Type = type;
    }

    /// <summary>
    /// 현재 사용가능한 스킬 계산
    /// </summary>
    public virtual (List<DungeonUnit> targets, int newPosIndex) GetPatternInfo()
    {
        var skillStates = ContentsManager.Instance.Get<DungeonContents>().CurSkillTargetStates;
        var results = new List<DungeonUnit>();
        int swapIndex = -1;
        for (int i = 0; i < skillStates.Count; i++)
        {
            if (skillStates[i].TargetsByPosDic.Count == 0) //이 스킬은 타겟이 없으면 패스
                continue;

            var targets = skillStates[i].TargetsByPosDic[TransformIndex];
            ExecuteSkillIndex = i;
            if (targets.Count != 0) //현재자리 우선
            {
                foreach (var t in targets)
                    results.Add((DungeonUnit)t);
                break;
            }
            else //다른 자리 탐색
            {
                var targetsDic = skillStates[i].TargetsByPosDic;
                var validKeys = new List<int>();
                foreach (var pair in targetsDic)
                {
                    if (pair.Value.Count != 0)
                        validKeys.Add(pair.Key);
                }
                if (validKeys.Count != 0)
                {
                    swapIndex = validKeys[UnityEngine.Random.Range(0, validKeys.Count)];
                    foreach (var t in targetsDic[swapIndex])
                        results.Add((DungeonUnit)t);
                }
                break;
            }
        }

        return (results, swapIndex);
    }

    public async UniTask ExecutePattern(List<DungeonUnit> targets, ISkillCamera camera)
    {
        var skill = m_ActiveSkills[ExecuteSkillIndex].Data;
        var context = new SkillMotionContext(this, targets.Cast<ISkillVisual>().ToList(), camera);
        await skill.PlayMotion(context);
        foreach (var effectID in skill.EffectIDs)
        {
            var effect = BDatabase.GetEffect(effectID);
            if (effect == null) continue;
            foreach (var target in targets)
                EffectExecutor.Execute(effect, new EffectContext(this, target));
        }
    }

    public override void Dead()
    {
        GameEventReporter.Report(EGameEventType.KillEnemy, ID);
        base.Dead();
    }
}
