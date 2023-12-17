using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FixedHpBar : MonoBehaviour
{
    [SerializeField] private Slider _hpBarSlider;
    [SerializeField] private TextMeshProUGUI _hpText;

    public void SetHpBar(int maxHp, int hp)
    {
        _hpBarSlider.value = hp / (float)maxHp;
        if (_hpText != null)
        {
            _hpText.text = $"{hp} / {maxHp}";
        }
    }
}
