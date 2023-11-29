using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpGauge : MonoBehaviour
{
    private Image _GaugeImage;

    public float LerpSpeed;
    private void Awake()
    {
        _GaugeImage = this.GetComponent<Image>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _GaugeImage.fillAmount =PlayerManager.Instance.Player.HpRatio;
    }

    // Update is called once per frame
    void Update()
    {
        ManageHp();
    }

    private void ManageHp()
    {
        _GaugeImage.fillAmount = Mathf.Lerp(_GaugeImage.fillAmount,PlayerManager.Instance.Player.HpRatio, Time.deltaTime * LerpSpeed);
    }
}
