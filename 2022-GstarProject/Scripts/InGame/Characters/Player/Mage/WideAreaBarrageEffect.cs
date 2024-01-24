using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;
using Random = UnityEngine.Random;

public sealed class WideAreaBarrageEffect : SkillAttack, IPoolable
{
    public int InstanceId { get; set; }
    [Header("데미지가 적용되기 까지 시간")]
    [SerializeField] private float _dealTime;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private Dictionary<Collider, IEnumerator> _targets = new Dictionary<Collider, IEnumerator>();


    public void SetTransform(Transform target)
    {
        this.transform.position = new Vector3(target.position.x, target.transform.position.y, target.position.z);
    }

    public void CallEvent()
    {
        SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.WideArea);
        Invoke(nameof(DisableObject), 7f);
    }

    private void DisableObject()
    {
        ObjectPoolManager.Instance.ReturnObject(this.gameObject);
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
            if (enemy == null)
                break;

            yield return new WaitForSeconds(_dealTime);
            enemy.TryGetDamage(DataManager.Instance.Player.Stat, this);
            SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.WideAreaHit);
            count++;
        }
    }
}
