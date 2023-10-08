using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Roulette : MonoBehaviour
{
    [SerializeField] private RouletteType _curType;
    [SerializeField] private TextMeshProUGUI _rouletteCountText;

    [SerializeField] private RouletteButton _button;

    private int _rouletteCount;

    public int RouletteCount => _rouletteCount;

    private bool _isRoulette;


    /// <summary>
    /// 버튼 활성화 여부
    /// </summary>
    public void InteractableButton(bool isActive)
    {
        _button.InteractableButton(isActive);
    }

    /// <summary>
    /// 뽑기권 개수 설정
    /// </summary>
    public void SetRouletteCount(int count)
    {
        _rouletteCount = count;
        _rouletteCountText.text = $"{_rouletteCount}";
    }

    private void RouletteDelayCo()
    {
        _isRoulette = false;
    }

    /// <summary>
    /// 버튼클릭으로 뽑기권 사용
    /// </summary>
    public void UseRoulette()
    {
        Debug.Log($"UseRoulette-_rouletteCount : {_rouletteCount}");
        Debug.Log($"UseRoulette-_isRoulette : {_isRoulette}");
        if (_rouletteCount > 0 && !_isRoulette)
        {
            _isRoulette = true;
            Invoke(nameof(RouletteDelayCo), 0.5f);
            SoundManager.Instance.PlayEffect(EffectType.RouletteUse);
            Player player = PlayerManager.Instance.Players[0];

            Debug.Log($"UseRoulette-_curType : {_curType}");
            switch (_curType)
            {
                case RouletteType.Normal:
                    NetworkManager.Instance.SendRequestNormalItemTicket(player.ID);
                    break;
                case RouletteType.Advanced:
                    NetworkManager.Instance.SendRequestAdvancedItemTicket(player.ID);
                    break;
                case RouletteType.Top:
                    NetworkManager.Instance.SendRequestTopItemTicket(player.ID);
                    break;
                case RouletteType.Supreme:
                    NetworkManager.Instance.SendRequestSupremeItemTicket(player.ID);
                    break;
            }
        }
    }

}
