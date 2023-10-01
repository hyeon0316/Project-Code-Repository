using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUI : MonoBehaviour
{
    [SerializeField]
    private MapContentUI mapContentUI;

    [SerializeField] private EnemySpawnArea[] q1_1;
    [SerializeField] private EnemySpawnArea[] q1_2;
    [SerializeField] private EnemySpawnArea[] q2_1;
    [SerializeField] private EnemySpawnArea[] q2_2;

    [SerializeField]
    private GameObject[] _targetBoxs;
    // Start is called before the first frame update

    private void Start()
    {
        mapContentUI.enemyarea = q1_1;
        mapContentUI.Name.text = "°ñ·½ ¼­½ÄÁö1";
        mapContentUI.SetContentUI();
        mapContentUI.SpwanIndex = 4;
        SetTarget(0);
    }
    public void SetUI1_1()
    {
        mapContentUI.enemyarea = q1_1;
        mapContentUI.Name.text = "°ñ·½ ¼­½ÄÁö1";
        mapContentUI.SetContentUI();
        mapContentUI.SpwanIndex = 4;
        SetTarget(0);
    }
    public void SetUI1_2()
    {
        mapContentUI.enemyarea = q1_2;
        mapContentUI.Name.text = "°ñ·½ ¼­½ÄÁö2";
        mapContentUI.SetContentUI();
        mapContentUI.SpwanIndex = 6;
        SetTarget(1);
    }
    public void SetUI2_1()
    {
        mapContentUI.enemyarea = q2_1;
        mapContentUI.Name.text = "°íºí¸° ¼­½ÄÁö";
        mapContentUI.SetContentUI();
        mapContentUI.SpwanIndex = 9;
        SetTarget(2);
    }
    public void SetUI2_2()
    {
        mapContentUI.enemyarea = q2_2;
        mapContentUI.Name.text = "°ñ·½ ¹«´ý";
        mapContentUI.SetContentUI();
        mapContentUI.SpwanIndex = 12;
        SetTarget(3);
    }

    private void SetTarget(int index)
    {
        for (int i = 0; i < _targetBoxs.Length; i++)
        {
            if (i == index)
                _targetBoxs[i].SetActive(true);
            else
                _targetBoxs[i].SetActive(false);
        }
    }
    public void ExitBtn()
    {
        gameObject.SetActive(false);
    }
}
