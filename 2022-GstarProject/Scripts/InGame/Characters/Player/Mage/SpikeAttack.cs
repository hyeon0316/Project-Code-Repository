using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SpikeAttack : SkillAttack
{
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void SetTransform(Transform target)
    {
        this.transform.position = new Vector3(target.position.x, target.transform.position.y, target.position.z); ;
    }

    public void CallEvent()
    {
        SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.SpikeAttack);
        Invoke(nameof(DisableObject), 3f);
    }

    private void DisableObject()
    {
        ObjectPoolManager.Instance.ReturnObject(PoolType.VolcanicSpike, this.gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            other.GetComponent<Creature>().TryGetDamage(DataManager.Instance.Player.Stat, this);
        }
    }
}
