using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FootPrint : MonoBehaviour, IPoolable
{
    public int InstanceId { get; set; }

    [SerializeField] private float _printSpeed;


    /// <summary>
    /// 발자국 오브젝트 생성
    /// </summary>
    public void Print()
    {
        StartCoroutine(PrintCo());
    }

    private IEnumerator PrintCo()
    {
        transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), _printSpeed);
        yield return new WaitForSeconds(_printSpeed);
        transform.DOScale(new Vector3(0, 0, 0), _printSpeed);
        yield return new WaitForSeconds(_printSpeed);
        ObjectPoolManager.Instance.ReturnObject(this.gameObject);
    }

}
