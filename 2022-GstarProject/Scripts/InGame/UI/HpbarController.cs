using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 모든 HpBar에 관련한 컨트롤러
/// </summary>
public class HpbarController : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI Name;

    [Header("화면에 고정 될 것인지 캐릭터 위에 띄울 것인지 결정")]
    [SerializeField] private bool _isFixed;

    private Camera _cam;
    private void Awake()
    {
        _cam = Camera.main;
        gameObject.GetComponent<Canvas>().worldCamera = _cam;
    }

    public void SetHpBar(int maxHp, string name)
    {
        slider.maxValue = maxHp;
        slider.value = slider.maxValue;
        Name.text = name;
    }
    
    public void SetHpBar(int maxHp)
    {
        slider.maxValue = maxHp;
        slider.value = slider.maxValue;
    }

    public void UpdateHpBar(int amount)
    {
        slider.value = amount;
    }

    /// <summary>
    /// 플레이어 체력바 업데이트
    /// </summary>
    public void UpdateHpBar(int value, string hpText, int maxHp)
    {
        slider.maxValue = maxHp;
        slider.value = value;
        Name.text = hpText;
    }

    public void ShowHpBar()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void CloseHpBar()
    {
        gameObject.SetActive(false);
    }
    
    public void SetNpc(string name)
    {
        Name.text = name;
    }
    
    private void Update()
    {
        if(!_isFixed)
        {
            if (_cam == null)
            {
                _cam = Camera.main;
                gameObject.GetComponent<Canvas>().worldCamera = _cam;
            }

            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x,
                _cam.transform.rotation.eulerAngles.y, transform.rotation.z));
        }
    }
}
