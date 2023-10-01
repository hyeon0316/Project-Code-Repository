using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public GameObject[] Walf;
    
    public GameObject[] EnemyPos;

    private DialogueManager _dialogueManager;

    private string[] _eventSentences_SecondFloor;
    private string[] _eventSentences_BossRoom;

    private void Awake()
    {
        _dialogueManager = FindObjectOfType<DialogueManager>();
        _eventSentences_SecondFloor = new string[]{"살려줘!!!", "이 녀석들이 날 납치했어!!!", "Stop"};
        _eventSentences_BossRoom = new string[]
            {"......", "처음 보는 한낱 조무래기가 잘도 여기까지 왔구나..", "긴 말은 필요 없다.", "죽어라.", "Stop"};
    }

    /// <summary>
    /// 다른 콜리더는 끄고 필요한 콜리더만 켜주는 함수
    /// </summary>
    /// <param name="nextCollider">활성화 시킬 콜리더</param>
    public void ActivateCollider(string nextCollider)
    {
        for (int i = 0; i < GameObject.Find("Colliders").transform.childCount; i++)
        {
            GameObject.Find("Colliders").transform.GetChild(i).gameObject.SetActive(false);
        }
        GameObject.Find("Colliders").transform.Find(nextCollider).gameObject.SetActive(true);
    }

    /// <summary>
    /// 각 맵의 몬스터들을 클리어시 워프를 사용 가능
    /// </summary>
    public void ActivateWalf(int enemyPosIndex,int walfIndex)
    {
        if (EnemyPos[enemyPosIndex].transform.childCount == 0)
        {
            Walf[walfIndex].SetActive(true);
            if (enemyPosIndex == 0)
            {
                Walf[++walfIndex].SetActive(true);
                Walf[++walfIndex].SetActive(true);
            }
        }
    }

    public void PlayCutScene(int playList)
    {
        if(playList == 1)
            StartCoroutine(SecondFloorCutSceneCo());
        else if(playList == 2)
            StartCoroutine(BossCutSceneCo());
    }

    /// <summary>
    /// 2층 이벤트컷씬 실행 
    /// </summary>
    /// <returns></returns>
    private IEnumerator SecondFloorCutSceneCo()
    {
        GameObject.Find("PlayerUICanvas").SetActive(false);
        yield return new WaitForSeconds(1f);
        EnemyCounter.IsPlayerStop = false;
        FindObjectOfType<Player>().PlayerAnim.SetBool("IsRun", false);
        _dialogueManager.TalkStart();
        yield return new WaitForSeconds(1f);
        if (FindObjectOfType<Player>().transform.GetChild(0).localScale.x > 0)
        {
            FindObjectOfType<Player>().ChangeDirection(false);
        }

        FindObjectOfType<CameraManager>().Target = GameObject.Find("EnemyPos_Second");
        _dialogueManager.TalkPanel.transform.position = FindObjectOfType<NpcTalk>().transform.position + new Vector3(0.7f, 1.7f, -1.7f);
        _dialogueManager.OnDialogue(_eventSentences_SecondFloor);
    }

    /// <summary>
    /// 보스방 진입 시 이벤트컷씬 실행 
    /// </summary>
    /// <returns></returns>
    private IEnumerator BossCutSceneCo()
    {
        GameObject.Find("PlayerUICanvas").SetActive(false);
        FindObjectOfType<Necromancer>().IsCutScene = true;
        yield return new WaitForSeconds(0.5f);
        EnemyCounter.IsPlayerStop = false;
        FindObjectOfType<Player>().PlayerAnim.SetBool("IsRun", false);
        _dialogueManager.TalkStart();
        yield return new WaitForSeconds(0.5f);
        FindObjectOfType<CameraManager>().Target = GameObject.Find("Demon_Page1");
        yield return new WaitForSeconds(0.5f);
        _dialogueManager.TalkPanel.transform.position = GameObject.Find("Demon_Page1").transform.position + new Vector3(0.7f, 0.7f, 0);
        _dialogueManager.OnDialogue(_eventSentences_BossRoom);
    }
}
