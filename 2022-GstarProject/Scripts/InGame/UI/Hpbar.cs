using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Hpbar : MonoBehaviour
{
    [SerializeField] private Slider _hpBarSlider;
    [SerializeField] private TextMeshProUGUI _nameText;

    private Camera _cam;


    private void Awake()
    {
        _cam = Camera.main;
        gameObject.GetComponent<Canvas>().worldCamera = _cam;
    }

    public void SetHpBar(int maxHp, int hp, string name)
    {
        _hpBarSlider.value = hp / (float)maxHp;
        _nameText.text = name;
    }

    public void ShowHpBar()
    {
        this.gameObject.SetActive(true);
    }

    public void CloseHpBar()
    {
        this.gameObject.SetActive(false);
    }

    public void SetNpc(string name)
    {
        _nameText.text = name;
    }

    private void LateUpdate()
    {
        this.transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x,
            _cam.transform.rotation.eulerAngles.y, transform.rotation.z));
    }
}
