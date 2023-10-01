using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    using UnityEngine.UI;

public enum EffectSoundType
{
    equip,
    money,
    questfin,
    queststart,
    enforce,
    BulletRain,
    BulletRainHit,
    ChainLightning,
    SpikeAttack,
    WideArea,
    WideAreaHit,
    WindAttack,
    NormalAttack,
    BossAttack,
    BossThrowing,
    BossRoar,
    BossDie
}

public enum PlayerSoundType
{
    Attak1,
    Attak2,
    Attak3,
    Jump1,
    Jump2,
    Jump3,
    Die,
    NormalAttack,
    Drink
}
[System.Serializable]
public class EffectSound
{
    public EffectSoundType Enum;
    public AudioClip clip;
}
[System.Serializable]
public class PlayerSound
{
    public PlayerSoundType Enum;
    public AudioClip clip;
}

public class SoundManager : Singleton<SoundManager>
{
    // Start is called before the first frame update
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    [SerializeField] private AudioSource[] effectAudios;
    [SerializeField] private AudioSource bgmAudio;
    [SerializeField] private AudioSource playerAudio;
    [SerializeField] private EffectSound[] effectSounds;
    [SerializeField] private PlayerSound[] playerSounds;
    [SerializeField] private AudioClip[] bgmClips;


    public void SetBGMSetting(float sliderVal)
    {
        bgmAudio.volume = sliderVal;
        Debug.Log(sliderVal);
    }
    public void SetPlayerSetting(float sliderVal)
    {
        playerAudio.volume = sliderVal;
        Debug.Log(sliderVal);
    }
    public void SetEffectSetting(float sliderVal)
    {
        for (int i = 0; i < effectAudios.Length; i++)
        {
            effectAudios[i].volume = sliderVal;
        }
        Debug.Log(sliderVal);
    }
    /// <summary>
    /// 3D EffectSound 재생 (자신이 AudioSOurce를 가지고 있을 경우)
    /// </summary>
    public void EffectPlay(AudioSource _au, EffectSoundType effectSound)
    {
        for (int i = 0; i < effectSounds.Length; i++)
        {
            if (effectSounds[i].Enum == effectSound)
            {
                _au.clip = effectSounds[i].clip;
                _au.Play();
                return;
            }

        }
        Debug.Log(effectSound + "사운드X");
    }
    /// <summary>
    /// 2D EffectSound 재생
    /// </summary>
    public void EffectPlay(EffectSoundType effectSound)
    {
        for (int i = 0; i < effectSounds.Length; i++)
        {
            if (effectSounds[i].Enum == effectSound)
            {
                for (int j = 0; j < effectSounds.Length; j++)
                {
                    if (!effectAudios[j].isPlaying)
                    {
                        effectAudios[j].clip = effectSounds[i].clip;
                        effectAudios[j].Play();
                        return;
                    }
                }
                Debug.Log("모든 EffectAudioSound사용중");
                return;
            }

        }
        Debug.Log(effectSound + "사운드X");
    }
    /// <summary>
    /// 플레이어 Voice
    /// </summary>
    public void PlayerPlay(PlayerSoundType PlayerSound)
    {
        for (int i = 0; i < playerSounds.Length; i++)
        {
            if (playerSounds[i].Enum == PlayerSound)
            {
                playerAudio.clip = playerSounds[i].clip;
                playerAudio.Play();
                return;
            }

        }
        Debug.Log(PlayerSound + "사운드X");
    }
    /// <summary>
    /// 0 로비 1 던전 2시티
    /// </summary>
    public void BgmPlay(int index)
    {
        if (bgmAudio.isPlaying && bgmAudio.clip == bgmClips[index])
        {
        }
        else
        {
            bgmAudio.clip = bgmClips[index];
            bgmAudio.Play();
        }
    }
}