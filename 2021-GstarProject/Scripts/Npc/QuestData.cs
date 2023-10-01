using System.Collections;
using System.Collections.Generic;

public class QuestData 
{
    public string questName;//퀘스트창에 표시 될 퀘스트명
    public int[] npcId;//npc분류

    public QuestData(string name, int[] npc)
    {
        questName = name;
        npcId = npc;
    }
}
