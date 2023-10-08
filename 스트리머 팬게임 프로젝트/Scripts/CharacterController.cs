using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum AnimType
{
    Use,
    Hit,
    Victory,
    Defeat,
    
    /// <summary>
    /// 단답벌레 전용
    /// </summary>
    UseYes,
    
    /// <summary>
    /// 단답벌레 전용
    /// </summary>
    UseNo,
}

/// <summary>
/// 애니메이션 캐릭터
/// </summary>
public class CharacterController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private ECharacterType _curCharacterType;
    [SerializeField] private ECreepType _curCreepType;
    [SerializeField] private Animator _anim;
    
    private Image _image;

    [SerializeField] private Image _emoteImage;
    private Sequence _sequence;

    /// <summary>
    /// 관전되는 캐릭터인지
    /// </summary>
    private bool _isView;
    
    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    /// <summary>
    /// 현재 관전중인 캐릭터인지 아닌지 체크
    /// </summary>
    public void SetView(bool isView)
    {
        _isView = isView;
    }

    /// <summary>
    /// 이모티콘 사용
    /// </summary>
    public void UseEmotion(Sprite useSprite)
    {
        _emoteImage.sprite = useSprite;
        _sequence = DOTween.Sequence()
            .SetAutoKill(false)
            .OnStart(() =>
            {
                _emoteImage.transform.localScale = Vector3.zero;
            })
            .Append(_emoteImage.transform.DOScale(1, 1).SetEase(Ease.Linear))
            .Join(_emoteImage.transform.DORotate(new Vector3(0, 0, 25), 1f).SetEase(Ease.InSine))
            .Insert(1f, _emoteImage.transform.DORotate(new Vector3(0, 0, -25), 1f).SetEase(Ease.InSine))
            .Insert(2f, _emoteImage.transform.DORotate(new Vector3(0, 0, 25), 1f).SetEase(Ease.InSine))
            .Append(_emoteImage.transform.DOScale(0, 1).SetEase(Ease.Linear))
            .Join(_emoteImage.transform.DORotate(new Vector3(0, 0, -25), 1f).SetEase(Ease.InSine));
    }
    
    public void SetNickName(string name)
    {
        _nameText.text = name;
        Debug.Log(_nameText.text);
    }

    public void PlayAnim(AnimType type)
    {
        switch (type)
        {
            case AnimType.Use:
                _anim.SetTrigger(Global.UseTrigger);
                break;
            case AnimType.Hit:
                _anim.SetTrigger(Global.HitTrigger);
                break;
            case AnimType.Victory:
                _anim.SetTrigger(Global.VictoryTrigger);
                break;
            case AnimType.Defeat:
                _anim.SetTrigger(Global.DefeatTrigger);
                break;
            case AnimType.UseYes:
                _anim.SetTrigger(Global.UseYesTrigger);
                break;
            case AnimType.UseNo:
                _anim.SetTrigger(Global.UseNoTrigger);
                break;
        }
    }

    public void PlayAttackSound()
    {
        if (_isView)
        {
            SoundManager.Instance.PlayEffect(EffectType.Attack);
        }
    }
    
    public void PlayHitSound()
    {
        if (_isView)
        {
            switch (_curCharacterType)
            {
                case ECharacterType.Woowakgood:
                    SoundManager.Instance.PlayEffect(EffectType.WakgoodHit);
                    break;
                case ECharacterType.Ine:
                    SoundManager.Instance.PlayEffect(EffectType.IneHit);
                    break;
                case ECharacterType.Jingburger:
                    SoundManager.Instance.PlayEffect(EffectType.JingburgerHit);
                    break;
                case ECharacterType.Lilpa:
                    SoundManager.Instance.PlayEffect(EffectType.LilpaHit);
                    break;
                case ECharacterType.Jururu:
                    SoundManager.Instance.PlayEffect(EffectType.JururuHit);
                    break;
                case ECharacterType.Gosegu:
                    SoundManager.Instance.PlayEffect(EffectType.GoseguHit);
                    break;
                case ECharacterType.Viichan:
                    SoundManager.Instance.PlayEffect(EffectType.ViichanHit);
                    break;
                case ECharacterType.Empty: //Creep일 경우
                    switch (_curCreepType)
                    {
                        case ECreepType.Shrimp:
                            SoundManager.Instance.PlayEffect(EffectType.ShrimpHit);
                            break;
                        case ECreepType.NegativeMan:
                            SoundManager.Instance.PlayEffect(EffectType.NegativeManHit);
                            break;
                        case ECreepType.Hodd:
                            SoundManager.Instance.PlayEffect(EffectType.HoddHit);
                            break;
                        case ECreepType.Wakpago:
                            SoundManager.Instance.PlayEffect(EffectType.WakpagoHit);
                            break;
                        case ECreepType.ShortAnswer:
                            SoundManager.Instance.PlayEffect(EffectType.ShortAnswerHit);
                            break;
                        case ECreepType.ChunSik:
                            SoundManager.Instance.PlayEffect(EffectType.ChunSikHit);
                            break;
                        case ECreepType.KwonMin:
                            SoundManager.Instance.PlayEffect(EffectType.KwonMinHit);
                            break;
                    }
                    break;
            }
        }
    }
    
    /// <summary>
    /// 캐릭터를 반전시켜야 할 경우
    /// </summary>
    public void ReverseImage()
    {
        transform.localScale = new Vector3(-0.8f, 0.8f, 1);
        _nameText.transform.localScale = new Vector3(-1, 1, 1);
    }


    public void SetImageColor(Color color)
    {
        _image.color = color;
    }
    /// <summary>
    /// 알파값을 통해 점차 사라지게 함
    /// </summary>
    public void SetAlphaTransition()
    {
        _image.DOFade(0, 1);
    }
}
