using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestContentUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI Content;
    [SerializeField] private TextMeshProUGUI NeedObject;
    [SerializeField] private TextMeshProUGUI Reward1;
    [SerializeField] private TextMeshProUGUI Reward2;
    [SerializeField] private Image[] itemslots;

    public void UpdateUI(QuestData data)
    {
        Name.text = data.Name;
        Content.text = data.Content;
        if(!data.IsCompleteObjectives)
        if (data.type == QuestType.KillEnemy)
        {
            
            NeedObject.text = data.Target +" 처치(" + data.collectObjectives.currentAmount + "/" + data.collectObjectives.amount + ")";
        }
        else
        {
            NeedObject.text = data.Target+"찾아가기";
        }

        for (int i = 0; i < itemslots.Length; i++)
        {
            itemslots[i].gameObject.SetActive(false);
        }
        if (data.rewards[0].ItemReward == null)
        {
            
        }
        else
        {
            for (int a = 0; a < data.rewards.Length; a++)
            {
                itemslots[a].gameObject.SetActive(true);
            }
            for (int a = 0; a < data.rewards.Length; a++)
            {
                itemslots[a].sprite = data.rewards[a].ItemReward.IconSprite;
            }
        }
        int rewardresult=0;
        for (int a = 0; a < data.rewards.Length; a++)
        {
            rewardresult += data.rewards[a].EXPReward;
        }
        Reward2.text = "EXP : " + rewardresult.ToString();
    }
}
