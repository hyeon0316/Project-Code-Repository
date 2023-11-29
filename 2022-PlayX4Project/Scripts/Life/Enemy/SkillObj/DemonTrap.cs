using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonTrap : MonoBehaviour
{
    private int _power = 10;

    private float _timer;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Life>().GetDamage(_power , 1);
        }
    }

    private void OnEnable()
    {
        _timer = 0;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= 0.2f)
            FindObjectOfType<Demon>().ReturnEffect(this.gameObject);

    }
}
