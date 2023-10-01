using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoolDown : MonoBehaviour
{
    [SerializeField] private Image _imageFill;
    
    [Header("스킬 쿨타임")]
    [SerializeField] private float _coolDownMax;

    private float _coolDown;
    private float _imageAmount;

    [SerializeField] private TextMeshProUGUI _coolDownText;

    public bool IsCoolDown { get; set; }

    private void Update()
    {
        if (IsCoolDown)
        {
            _coolDown -= Time.deltaTime;
            _imageAmount += Time.deltaTime;
            _imageFill.fillAmount = _imageAmount / _coolDownMax;
            _coolDownText.text = $"{_coolDown:N1}";

            if (_coolDown <= 0)
            {
                IsCoolDown = false;
                _imageFill.gameObject.SetActive(false);
                _coolDown = 0;
            }
        }
    }

    public void SetCoolDown()
    {
        _coolDown = _coolDownMax;
        _imageFill.gameObject.SetActive(true);
        _imageFill.fillAmount = 0;
        _imageAmount = 0;
        IsCoolDown = true;
    }
    
    
}
