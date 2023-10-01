using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public QuestSlotUI[] _questSlots;
    public void SetQuest(QuestData[] quest)
    {
        for(int a = 0;a< quest.Length;a++)
        {
            _questSlots[a].Name.text = quest[a].Name;
            _questSlots[a].questData = quest[a];
        }
    }
}
