using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootPrinter : MonoBehaviour
{
    [SerializeField] private GameObject _footPrintPrefab;
    private CapsuleCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<CapsuleCollider>();
        _collider.enabled = false;
        ObjectPoolManager.Instance.Init(_footPrintPrefab, 2);
    }

    public void ActiveFoot(bool IsActive)
    {
        _collider.enabled = IsActive;
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Snow"))
        {
            GameObject foot = ObjectPoolManager.Instance.GetObject(_footPrintPrefab);
            foot.transform.position = this.transform.position;
            foot.GetComponent<FootPrint>().Print();
        }
    }
}
