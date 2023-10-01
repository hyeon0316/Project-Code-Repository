using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootPrinter : MonoBehaviour
{
    private CapsuleCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<CapsuleCollider>();
        _collider.enabled = false;
    }

    public void ActiveFoot(bool IsActive)
    {
        _collider.enabled = IsActive;
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Snow"))
        {
            GameObject foot = ObjectPoolManager.Instance.GetObject(PoolType.SnowFootPrint);
            foot.transform.position = transform.position;
            foot.GetComponent<FootPrint>().Print();
        }
    }
}
