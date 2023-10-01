using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum QuestState
{
    Startable,
    Progressing,
    Completable,
    Complete
}
public enum QuestType
{
    FindNpc,
    KillEnemy
}

[CreateAssetMenu(fileName = "New Quest", menuName = "New Quest/Quest")]
public class QuestData : ScriptableObject
{
    public QuestState state = QuestState.Startable;
    public QuestType type = QuestType.KillEnemy;
    public int ID;
    public string Name;
    [TextArea(2, 6)]
    public string Content;
    public string Target;
    public CollectObjective collectObjectives;
    public Rewards[] rewards;

    public bool IsCompleteObjectives//퀘스트 완료 조건 확인
    {
        get
        {
            if (!collectObjectives.IsComplete)
                return false;
            return true;
        }
    }
}



[Serializable]
public abstract class Objective
{
    
    public PoolType[] EnemyID;
    public int amount;
    public int currentAmount  { get; set; }

    public bool IsComplete { get { return currentAmount >= amount; } }//재료를 다 모을 시

    public int NpcId;
}

[Serializable]
public class CollectObjective : Objective
{
    
    public void UpdateCount()
    {
        currentAmount++;
    }
}

[Serializable]
public class Rewards
{
    public ItemData ItemReward;
    public int ItemRewardCount = 1;
    public int EXPReward = 0;

    public void Reward()
    {
        if (ItemReward != null)
        {
            GameObject.Find("");
            QuestManager.Instance._inventory.Add(ItemReward, ItemRewardCount);
        }
         //Inventory.inst.AcquireItem(ItemReward, ItemRewardCount);

        if (EXPReward != 0)
        { 
        }
            //Player.inst.ExpPlus(EXPReward);
    }
}

