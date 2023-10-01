using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TalkManager talkManager;
    public QuestManager questManager;
    public GameObject talkPanel;
    public GameObject skillBG;
    public TypeEffect talk;
    public GameObject scanObject;//ObjData를 담는 변수
    public bool isAction;
    public int talkIndex;

    public void Awake()
    {
        scanObject = null;
    }
    private void Update()
    {
        if((Input.GetKeyDown(KeyCode.Space)|| Input.GetMouseButtonDown(0)) && scanObject!=null && isAction)//대화 진행
        {
            ObjData objData = scanObject.GetComponent<ObjData>();
            Talk(objData.id, objData.isNpc);
            talkPanel.SetActive(isAction);
        }
    }
    public void Action(GameObject scanObj)//마우스 우클릭으로 NPC에게 대화를 걸때 사용될 함수
    {
        scanObject = scanObj;
        ObjData objData = scanObject.GetComponent<ObjData>();
        Talk(objData.id, objData.isNpc);
        talkPanel.SetActive(isAction);
        skillBG.SetActive(false);
    }

    void Talk(int id, bool isNpc)//대화
    {
        int questTalkIndex;
        string talkData;

        if (talk.isAnim)
        {
            talk.SetMsg("");
            return;
        }
        else
        {
            questTalkIndex = questManager.GetQuestTalkIndex();
            talkData = talkManager.GetTalk(id + questTalkIndex, talkIndex);
        }

        if (talkData == null)
        {
            isAction = false;
            talkIndex = 0;
            Player.inst.npcCam.SetActive(false);
            skillBG.SetActive(true);
            if (Player.inst.h != null)
                Player.inst.h.SetActive(true);//대화중에 꺼주었던 머리위에 표시되는 nameCanVas를 다시 켜줌    
            questManager.CheckQuest(id);
            return;
        }
        talk.SetMsg(talkData);
        talkIndex++;
        isAction = true;
    }
}
