using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed class CutSceneSystem : MonoBehaviour
{  
    [SerializeField] private List<string> _eventSentences_SecondFloor;
    [SerializeField] private List<string> _eventSentences_BossRoom;
    [SerializeField] private GameObject _enemyPos;

    private bool _isSecondFloorEvent = true;
    private bool _isBossRoomEvent = true;

    public void PlayCutScene(int playList)
    {
        if (playList == 1 && _isSecondFloorEvent)
        {
            _isSecondFloorEvent = false;
            StartCoroutine(SecondFloorCutSceneCo());
        }
        else if (playList == 2 && _isBossRoomEvent)
        {
            _isBossRoomEvent = false;
            StartCoroutine(BossCutSceneCo());
        }
    }

    /// <summary>
    /// 2층 이벤트컷씬 실행 
    /// </summary>
    /// <returns></returns>
    private IEnumerator SecondFloorCutSceneCo()
    {
        DialogueManager.Instance.CurNpc = CachingManager.Instance().SecondFloorNpc;
        PlayerManager.Instance.Player.StopMove();
        PlayerManager.Instance.SetActivePlayerUI(false);
        yield return new WaitForSeconds(1f);
        DialogueManager.Instance.ActiveLetterBox();
        yield return new WaitForSeconds(1f);
        if (PlayerManager.Instance.Player.transform.GetChild(0).localScale.x > 0)
        {
           PlayerManager.Instance.Player.ChangeDirection(false);
        }

        CameraManager.Instance.SetTarget(_enemyPos);
        DialogueManager.Instance.SetPanelPos(DialogueManager.Instance.CurNpc.transform.position + new Vector3(0.7f, 1.7f, -1.7f));
        DialogueManager.Instance.OnDialogue(_eventSentences_SecondFloor);
    }

    /// <summary>
    /// 보스방 진입 시 이벤트컷씬 실행 
    /// </summary>
    /// <returns></returns>
    private IEnumerator BossCutSceneCo()
    {
        SoundManager.Instance.Play("BossBGM", SoundType.Bgm);
        PlayerManager.Instance.SetActivePlayerUI(false);
        PlayerManager.Instance.Player.StopMove();
        DialogueManager.Instance.ActiveLetterBox();
        yield return new WaitForSeconds(0.5f);
        CameraManager.Instance.SetTarget(CachingManager.Instance().BossCameraPos);
        yield return new WaitForSeconds(0.5f);
        DialogueManager.Instance.SetPanelPos(CachingManager.Instance().BossCameraPos.transform.position);
        DialogueManager.Instance.OnDialogue(_eventSentences_BossRoom);
    }
}
