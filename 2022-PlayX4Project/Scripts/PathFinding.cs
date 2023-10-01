using System;
using System.Collections;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public Vector3 TargetObj;
    public bool IsKeep;//NavItem이 오브젝트 근처에 계속 머물건지에 대한 변수로 사용
    private Vector3 _playerTr;
    

    private void Awake()
    {
        _playerTr = FindObjectOfType<Player>().transform.position;
    }

    void Start()
    {
        StartCoroutine(PathFindingCo());
    }

    private IEnumerator PathFindingCo()
    {
        float time = 0;
        while (true)
        {
            if (time > 1)
            {
                Destroy(this.gameObject);
                break;
            }

            time += 0.1f * Time.deltaTime;
            this.transform.position = Vector3.Lerp(_playerTr, TargetObj, time);
            yield return null;
        }
    }
}
