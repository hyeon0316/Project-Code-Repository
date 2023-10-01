using System;
using UnityEngine;

public enum QuestState
{
    Startable,
    Progressing,
    Completable,
    Complete
}

[CreateAssetMenu(fileName = "New Quest", menuName = "New Item/Quest")]
public class Quest : ScriptableObject
{
    public QuestState state = QuestState.Startable;
    public string title;

    [TextArea(2, 6)]
    public string content;
    public int qusetComplte;
    public CollectObjective[] collectObjectives;
    public Rewards[] rewards;
    
    public bool IsCompleteObjectives//퀘스트 완료 조건 확인
    {
        get
        {
            foreach (var o in collectObjectives)
            {
                if (!o.IsComplete)
                    return false;
            }
            return true;
        }
    }
}

[Serializable]
public abstract class Objective
{
    public Item item;
    public int amount;
    public int currentAmount { get; set; }

    public bool IsComplete { get { return currentAmount >= amount; } }//재료를 다 모을 시
}

[Serializable]
public class CollectObjective : Objective
{
    public void UpdateItemCount()
    {
        currentAmount = Inventory.inst.GetItemCount(item);
    }
}

[Serializable]
public class Rewards
{
    public Item ItemReward;
    public int ItemRewardCount=1;
    public int EXPReward=0;

    public void Reward()
    {
        if (ItemReward != null)
            Inventory.inst.AcquireItem(ItemReward,ItemRewardCount);

        if (EXPReward != 0)
            Player.inst.ExpPlus(EXPReward);
    }
}
