using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MapContentUI : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI Name;
    [SerializeField] private GameObject[] _obj;
    [SerializeField] private TextMeshProUGUI[] _text;
    [SerializeField] private GameObject obj;
    public EnemySpawnArea[] enemyarea { get; set; }
    private EnemySpawnArea _selectArea;
    [SerializeField] private TextMeshProUGUI ContentText;
    public int SpwanIndex { get; set; }
    public Transform SpawnTransform { get; set; }
    
    // Start is called before the first frame update
    // Update is called once per frame
    public void Awake()
    {
        for (int i = 0; i < _obj.Length;i++)
        {
            _obj[i].SetActive(false);
        }
    }
    public void SetContentUI()
    {
        Init();
        for (int i = 0; i < enemyarea.Length; i++)
        {
            _obj[i].SetActive(true);
            _text[i].text = enemyarea[i].MapName;
        }
    }
    private void Init()
    {
        _selectArea = enemyarea[0];
        SetContentText();
    }
    private void SetContentText()
    {
        string str = "";
        for(int i=0;i<_selectArea.EnemyName.Length;i++)
        {
            str += i+1 +"."+_selectArea.EnemyName[i] + "\n";
        }
        ContentText.text = str;
    }
    public void Btn1()
    {
        _selectArea = enemyarea[0];
        SetContentText();
    }
    public void Btn2()
    {
        _selectArea = enemyarea[1];
        SetContentText();
    }

    public void BtngoWalk()
    {
        DataManager.Instance.Player.CancelAutoHunt();
        DataManager.Instance.Player.IsQuest = false;
        QuestManager.Instance.SetAniQuest(false);
        DataManager.Instance.Player.SetAutoQuest(_selectArea.gameObject.transform);
        QuestManager.Instance.SetAniQuest(false);
        obj.SetActive(false);
    }
    public void BtngoSpwan()
    {
        DataManager.Instance.Player.CancelAutoHunt();
        Transform _tr;
        _tr = MapManager.Instance.GetSpwan(SpwanIndex);
        DataManager.Instance.Player.UseTeleport(_tr);
        obj.SetActive(false);
        Invoke("BtngoWalk", 2f);
    }
}
