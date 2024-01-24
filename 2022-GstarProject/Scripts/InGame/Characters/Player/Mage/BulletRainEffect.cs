using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRainEffect : MonoBehaviour, IPoolable
{
    public int InstanceId { get; set; }
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void DelayDisable()
    {
        SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BulletRainHit);
        Invoke(nameof(DisableEffect), 1.5f);
    }

    private void DisableEffect()
    {
        ObjectPoolManager.Instance.ReturnObject(this.gameObject);
    }
}
