using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType
{
    /// <summary>
    /// 반복 재생되는 하는 브금
    /// </summary>
    Bgm,
    
    /// <summary>
    /// 한번씩만 재생되는 효과음
    /// </summary>
    SFX,
}

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private SetSounds _setSounds;
    [SerializeField] private AudioSource[] _audioSources;

    /// <summary>
    /// bgm 재생
    /// </summary>
    public void PlayBGM(BGMType soundName, float pitch = 1.0f)
    {
        AudioClip audioClip = GetBGMClip(soundName);
        PlayBgmOrEffect(audioClip, SoundType.Bgm, pitch);
    }

    /// <summary>
    /// bgm 체크박스
    /// </summary>
    public void SetActiveBGM()
    {
        AudioSource audioSource = _audioSources[(int) SoundType.Bgm];
        if(audioSource.isPlaying)
            audioSource.Pause();
        else
            audioSource.Play();
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    public void PlayEffect(EffectType soundName, float pitch = 1.0f)
    {
        AudioClip audioClip = GetEffectClip(soundName);
        PlayBgmOrEffect(audioClip, SoundType.SFX, pitch);
    }
    
    private void PlayBgmOrEffect(AudioClip audioClip, SoundType type, float pitch = 1.0f)
    {
        if (audioClip == null)
            return;

        if (type == SoundType.Bgm) // BGM 배경음악 재생
        {
            AudioSource audioSource = _audioSources[(int) SoundType.Bgm];
            if (audioSource.isPlaying) //BGM은 중첩되면 안되기에 재생중인게 있다면 정지
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else if (type == SoundType.SFX) // Effect 효과음 재생
        {
            AudioSource audioSource = _audioSources[(int) SoundType.SFX];
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }

    private AudioClip GetBGMClip(BGMType bgm)
    {
        AudioClip clip = null;
        switch (bgm)
        {
            case BGMType.Title:
                clip = _setSounds.TitleBGM;
                break;
            case BGMType.Lobby:
                clip = _setSounds.LobbyBGM;
                break;
            case BGMType.Loading:
                clip = _setSounds.LodingBGM;
                break;
            case BGMType.Select:
                clip = _setSounds.SelectBGM;
                break;
            case BGMType.CutScene:
                clip = _setSounds.CutSceneBGM;
                break;
            case BGMType.Ready:
                clip = _setSounds.ReadyBGM;
                break;
            case BGMType.Battle:
                clip = _setSounds.BattleBGM;
                break;
        }

        return clip;
    }
    
    private AudioClip GetEffectClip(EffectType bgm)
    {
        AudioClip clip = null;
        switch (bgm)
        {
            case EffectType.CheckBox:
                clip = _setSounds.CheckBox;
                break;
            case EffectType.MatchStart:
                clip = _setSounds.MatchStart;
                break;
            case EffectType.ItemDrag:
                clip = _setSounds.ItemDrag;
                break;
            case EffectType.ItemDragEnd:
                clip = _setSounds.ItemDragEnd;
                break;
            case EffectType.CharacterClick:
                clip = _setSounds.CharacterClick;
                break;
            case EffectType.WakgoodSelect:
                clip = _setSounds.WakgoodSelect;
                break;
            case EffectType.IneSelect:
                clip = _setSounds.IneSelect;
                break;
            case EffectType.JingburgerSelect:
                clip = _setSounds.JingburgerSelect;
                break;
            case EffectType.LilpaSelect:
                clip = _setSounds.LilpaSelect;
                break;
            case EffectType.JururuSelect:
                clip = _setSounds.JururuSelect;
                break;
            case EffectType.GoseguSelect:
                clip = _setSounds.GoseguSelect;
                break;
            case EffectType.ViichanSelect:
                clip = _setSounds.ViichanSelect;
                break;
            case EffectType.RouletteUse:
                clip = _setSounds.RouletteUse;
                break;
            case EffectType.ItemDrop:
                clip = _setSounds.ItemDrop;
                break;
            case EffectType.ItemEquip:
                clip = _setSounds.ItemEquip;
                break;
            case EffectType.Upgrade:
                clip = _setSounds.Upgrade;
                break;
            case EffectType.Upgrade2:
                clip = _setSounds.Upgrade2;
                break;
            case EffectType.ReCombination:
                clip = _setSounds.ReCombination;
                break;
            case EffectType.Page:
                clip = _setSounds.Credit;
                break;
            case EffectType.Bag:
                clip = _setSounds.Bag;
                break;
            case EffectType.NameInput:
                clip = _setSounds.NameInput;
                break;
            case EffectType.SelectTimer:
                clip = _setSounds.SelectTimer;
                break;
            case EffectType.EntryBattle:
                clip = _setSounds.EntryBattle;
                break;
            case EffectType.WakgoodHit:
                clip = _setSounds.WakgoodHit;
                break;
            case EffectType.IneHit:
                clip = _setSounds.IneHit;
                break;
            case EffectType.JingburgerHit:
                clip = _setSounds.JingburgerHit;
                break;
            case EffectType.LilpaHit:
                clip = _setSounds.LilpaHit;
                break;
            case EffectType.JururuHit:
                clip = _setSounds.JururuHit;
                break;
            case EffectType.GoseguHit:
                clip = _setSounds.GoseguHit;
                break;
            case EffectType.ViichanHit:
                clip = _setSounds.ViichanHit;
                break;
            case EffectType.ShrimpHit:
                clip = _setSounds.ShrimpHit;
                break;
            case EffectType.NegativeManHit:
                clip = _setSounds.NegativeManHit;
                break;
            case EffectType.HoddHit:
                clip = _setSounds.HoddHit;
                break;
            case EffectType. WakpagoHit:
                clip = _setSounds.WakpagoHit;
                break;
            case EffectType.ShortAnswerHit:
                clip = _setSounds.ShortAnswerHit;
                break;
            case EffectType.ChunSikHit:
                clip = _setSounds.ChunSikHit;
                break;
            case EffectType.KwonMinHit:
                clip = _setSounds.KwonMinHit;
                break;
            case EffectType.Seconds:
                clip = _setSounds.Seconds;
                break;
            case EffectType.Attack:
                clip = _setSounds.Attack;
                break;
            case EffectType.Heal:
                clip = _setSounds.Heal;
                break;
            case EffectType.Defense:
                clip = _setSounds.Defense;
                break;
            case EffectType.Blood:
                clip = _setSounds.Blood;
                break;
            case EffectType.CounselLicense:
                clip = _setSounds.CounselLicense;
                break;
            case EffectType.Dodge:
                clip = _setSounds.Dodge;
                break;
            case EffectType.Bomb:
                clip = _setSounds.Bomb;
                break;
            case EffectType.WakEnter:
                clip = _setSounds.WakEnter;
                break;
            case EffectType.DiaSword:
                clip = _setSounds.DiaSword;
                break;
            case EffectType.Weaken:
                clip = _setSounds.DiaSword;
                break;
        }
        return clip;
    }
    
    
    /// <summary>
    /// 게임이 오래 지속 될때 새로운 사운드가 계속 추가되어 많아져서 한번 초기화가 필요할때 쓰이는 함수
    /// </summary>
    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
    }
}