﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SoundType
{
    /// <summary>
    /// 반복 재생되는 하는 브금
    /// </summary>
    Bgm,
    
    /// <summary>
    /// 한번씩만 재생되는 효과음
    /// </summary>
    Effect,
    
    /// <summary>
    /// enum의 개수를 체크하기 위해 추가
    /// </summary>
    MaxCount, 
}

public sealed class SoundManager : Singleton<SoundManager>
{
    private AudioSource[] _audioSources = new AudioSource[(int) SoundType.MaxCount];
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        string[] soundNames = System.Enum.GetNames(typeof(SoundType)); // "Bgm", "Effect"

        for (int i = 0; i < soundNames.Length - 1; i++)
        {
            GameObject go = new GameObject { name = soundNames[i] };
            _audioSources[i] = go.AddComponent<AudioSource>();
            go.transform.parent = this.transform;
        }

        _audioSources[(int)SoundType.Bgm].loop = true; // bgm 재생기는 무한 반복 재생
    }

    /// <summary>
    /// 사운드 재생 시 사용 될 함수
    /// </summary>
    public void Play(string path, SoundType type, float pitch =1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        PlayBgmOrEffect(audioClip, type, pitch);
    }
    
    public void PlayBgmOrEffect(AudioClip audioClip, SoundType type, float pitch = 1.0f)
    {
        if (audioClip == null)
            return;

        if (type == SoundType.Bgm) // BGM 배경음악 재생
        {
            Debug.Log(_audioSources);
            AudioSource audioSource = _audioSources[(int) SoundType.Bgm];
            Debug.Log(_audioSources.Length);
            if (audioSource.isPlaying) //BGM은 중첩되면 안되기에 재생중인게 있다면 정지
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else if (type == SoundType.Effect) // Effect 효과음 재생
        {
            AudioSource audioSource = _audioSources[(int) SoundType.Effect];
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }
    
    /// <summary>
    /// audioClip 반환 함수
    /// </summary>
    /// <param name="path">Sounds폴더 안의 사운드 경로</param>
    /// <param name="type">사운드 타입</param>
    /// <returns></returns>
    AudioClip GetOrAddAudioClip(string path, SoundType type)
    {
        AudioClip audioClip = null;

        if (!path.Contains("Sounds/"))
            path = $"Sounds/{path}"; // 경로를 Sounds폴더로 설정

        if (type == SoundType.Bgm) // BGM 배경음악 클립 붙이기
        {
            audioClip = Resources.Load<AudioClip>(path);
        }
        else if (type == SoundType.Effect)
        {
            if (!_audioClips.ContainsKey(path)) 
            {
                audioClip = Resources.Load<AudioClip>(path);
                _audioClips.Add(path, audioClip);
            }
            else if (_audioClips.ContainsKey(path))//해당 사운드가 이미 존재 할 경우
            {
                audioClip = Resources.Load<AudioClip>(path);
            }
        }

        if (audioClip == null)
            Debug.Log($"AudioClip Missing ! {path}");

        return audioClip;
    }
    
    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        _audioClips.Clear();
    }

    public void PlayHitSound(HitSoundType type)
    {
        switch (type)
        {
            case HitSoundType.ZHit1:
                SoundManager.Instance.Play("Player/ZAttackHit1", SoundType.Effect);
                break;
            case HitSoundType.ZHit2:
                SoundManager.Instance.Play("Player/ZAttackHit2", SoundType.Effect);
                break;
            case HitSoundType.XHit:
                SoundManager.Instance.Play("Player/XAttackHit", SoundType.Effect);
                break;
            case HitSoundType.AHit:
                SoundManager.Instance.Play("Player/BulletHit", SoundType.Effect);
                break;
            case HitSoundType.SHit:
                SoundManager.Instance.Play("Player/DashAttackHit", SoundType.Effect);
                break;
        }
    }

    public void ApplyBgmVolume(float bgm)
    {
        _audioSources[0].volume = bgm;
    }

    public void ApplyEffectVolume(float effect)
    {
        _audioSources[1].volume = effect;
    }
}

