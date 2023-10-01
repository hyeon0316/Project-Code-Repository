using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class QuestInGameUI : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI NeedObject;
    public GameObject Fin;
    private int questNum;
    private int questIndex;
    private bool questType;
    public Animator questing;
    // Start is called before the first frame update

    private void Start()
    {
        Fin.SetActive(false);
    }
    public void UpdateUI(QuestData data)
    {

        Name.text = data.Name;
        
        
        if (data.type == QuestType.FindNpc)
        {
            questNum = data.collectObjectives.NpcId;
            questIndex = data.ID;
            NeedObject.text = data.Target + " 찾아가기 ";
            questType = true;
        }
        else
        {

            questNum = data.ID;
            questIndex = data.ID;
            NeedObject.text = data.Target + " 처치(" + data.collectObjectives.currentAmount + "/" + data.collectObjectives.amount + ")";
            questType = false;
        }
        if (data.IsCompleteObjectives)
        {
            SoundManager.Instance.EffectPlay(EffectSoundType.questfin);
            DataManager.Instance.Player.IsQuest = false;
            QuestManager.Instance.SetAniQuest(false);
            NeedObject.text = "완료";
            Fin.SetActive(true);
        }
    }
    public void OnMoveSpawn()
    {
       
        DataManager.Instance.Player.CancelAutoHunt();
        Transform _tr;
        if (questType)
        {
            _tr = MapManager.Instance.GetSpwan(questIndex);
           DataManager.Instance.Player.UseTeleport(_tr);
        }
        else
        {
            _tr = MapManager.Instance.GetSpwan(questIndex);
            DataManager.Instance.Player.UseTeleport(_tr);
        }
        Invoke("OnClickQuest", 2f);
    }
    public void OnClickQuest()
    {
        if (DataManager.Instance.Player.gameObject.transform.position.y > 50)
        {
            DataManager.Instance.Player.CancelAutoHunt();
            Transform _tr;
            if (questType)
            {
                _tr = MapManager.Instance.GetNpcData(questNum);
                DataManager.Instance.Player.SetAutoQuest(_tr);
                MapManager.Instance.TargetNpc = _tr.GetComponent<NpcData>();
            }
            else
            {
                _tr = MapManager.Instance.GetEnemySpwan(questNum);
                DataManager.Instance.Player.SetAutoQuest(_tr);
            }
        }
    }
    public void OnFin()
    {
        SoundManager.Instance.EffectPlay(EffectSoundType.queststart);
        QuestManager.Instance.NextQuest();
        Fin.SetActive(false);
    }
    public void QuestAni(bool _s)
    {
        questing.SetBool("0", _s);
    }
}
