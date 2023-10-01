using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolDown : MonoBehaviour
{
    private Player _player;
    
    [Header("SKillA")]
    public KeyCode KeyA;
    public GameObject CoolDownA;
    private Image _fillImageA;
    private Text _coolTimeTextA;
    private bool _isCoolDownA;
    private float _timeA;
    private float _maxTimeA;
    
    [Header("SKillS")]
    public KeyCode KeyS;
    public GameObject CoolDownS;
    private Image _fillImageS;
    private Text _coolTimeTextS;
    private bool _isCoolDownS;
    private float _timeS;
    private float _maxTimeS;
    
    [Header("SKillD")]
    public KeyCode KeyD;
    public GameObject CoolDownD;
    private Image _fillImageD;
    private Text _coolTimeTextD;
    private bool _isCoolDownD;
    private float _timeD;
    public float TimeD
    {
        set { _timeD = value; }
    }
    private float _maxTimeD;
    
    private void Awake()
    {
        _player = FindObjectOfType<Player>();
        
        _fillImageA = CoolDownA.transform.GetChild(0).GetComponent<Image>();
        _coolTimeTextA = CoolDownA.transform.GetChild(1).GetComponent<Text>();
        
        _fillImageS = CoolDownS.transform.GetChild(0).GetComponent<Image>();
        _coolTimeTextS = CoolDownS.transform.GetChild(1).GetComponent<Text>();
        
        _fillImageD = CoolDownD.transform.GetChild(0).GetComponent<Image>();
        _coolTimeTextD = CoolDownD.transform.GetChild(1).GetComponent<Text>();
    }

    private void LateUpdate()
    {
        UseA();
        UseS();
        UseD();
    }

    private void UseA()
    {
        if (Input.GetKeyDown(KeyA) && !_isCoolDownA)
        {
            Debug.Log(KeyA);
            CoolDownA.SetActive(true);
            _timeA = _player.CountTimeList[3];
            _maxTimeA = _timeA;
            _fillImageA.fillAmount = _timeA / _maxTimeA;
            _isCoolDownA = true;
        }

        if (_isCoolDownA)
        {
            _timeA -= Time.deltaTime;
            _fillImageA.fillAmount = _timeA / _maxTimeA;
            _coolTimeTextA.text = $"{_timeA:N1}";

            if (_timeA <= 0)
            {
                _timeA = 0;
                CoolDownA.SetActive(false);
                _isCoolDownA = false;
            }
        }
    }

    private void UseS()
    {
        if (Input.GetKeyDown(KeyS) && !_isCoolDownS)
        {
            Debug.Log(KeyS);
            CoolDownS.SetActive(true);
            _timeS = _player.CountTimeList[2];
            _maxTimeS = _timeS;
            _fillImageS.fillAmount = _timeS / _maxTimeS;
            _isCoolDownS = true;
        }
        
        if (_isCoolDownS)
        {
            _timeS -= Time.deltaTime;
            _fillImageS.fillAmount = _timeS / _maxTimeS;
            _coolTimeTextS.text = $"{_timeS:N1}";

            if (_timeS <= 0)
            {
                _timeS = 0;
                CoolDownS.SetActive(false);
                _isCoolDownS = false;
            }
        }
    }

    private void UseD()
    {
        if (Input.GetKeyDown(KeyD) && !_isCoolDownD)
        {
            Debug.Log(KeyD);
            CoolDownD.SetActive(true);
            _timeD = _player.CountTimeList[4];
            _maxTimeD = _timeD;
            _fillImageD.fillAmount = _timeD / _maxTimeD;
            _isCoolDownD = true;
        }

        if (_isCoolDownD)
        {
            _timeD -= Time.deltaTime;
            _fillImageD.fillAmount = _timeD / _maxTimeD;
            _coolTimeTextD.text = $"{_timeD:N1}";

            if (_timeD <= 0)
            {
                _timeD = 0;
                CoolDownD.SetActive(false);
                _isCoolDownD = false;
            }
        }
    }
}
