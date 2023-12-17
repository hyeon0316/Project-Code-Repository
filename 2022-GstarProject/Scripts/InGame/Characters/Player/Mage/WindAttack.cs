using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class WindAttack : SkillAttack
{
    [SerializeField] private float _moveSpeed;

    [Header("데미지가 적용되기 까지 시간")]
    [SerializeField] private float _dealTime;

    private Dictionary<Collider, IEnumerator> _targets = new Dictionary<Collider, IEnumerator>();

    public void SetTransform(Transform target)
    {
        this.transform.position = new Vector3(target.position.x, target.transform.position.y, target.position.z); ;
    }

    public void CallEvent()
    {
        StartCoroutine(MoveCo());
        Invoke(nameof(DisableObject), 4f);
    }

    private IEnumerator MoveCo()
    {
        Vector3 movePos = DataManager.Instance.Player.transform.forward;
        while (true)
        {
            this.transform.Translate(movePos * _moveSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
    }

    private void DisableObject()
    {
        StopCoroutine(MoveCo());
        ObjectPoolManager.Instance.ReturnObject(PoolType.WindAttack, this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (!_targets.ContainsKey(other))
            {
                _targets.Add(other, TakeBarrageDamageCo(other.GetComponent<Creature>()));
                StartCoroutine(_targets[other]);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (_targets.ContainsKey(other))
            {
                StopCoroutine(_targets[other]);
                _targets.Remove(other);
            }
        }
    }

    private IEnumerator TakeBarrageDamageCo(Creature enemy)
    {
        int count = 0;
        while (count < 10)
        {
            if (enemy.IsDead)
            {
                break;
            }

            yield return new WaitForSeconds(_dealTime);
            enemy.TryGetDamage(DataManager.Instance.Player.Stat, this);
            count++;
        }
    }
}
