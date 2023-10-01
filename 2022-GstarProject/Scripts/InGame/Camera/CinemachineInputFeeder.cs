using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class CinemachineInputFeeder : MonoBehaviour
{
    [SerializeField] private CameraRotate _touchInput;

    private Vector2 _lookInput;

    [SerializeField] private float _touchSpeedSensitivityX = 3f;
    [SerializeField] private float _touchSpeedSensitivityY = 3f;

    private string _touchXMapTo = "Mouse X";
    private string _touchYMapTo = "Mouse Y";

    private void Start()
    {
        CinemachineCore.GetInputAxis = GetInputAxis;
        SoundManager.Instance.BgmPlay(2);
    }
    
    private float GetInputAxis(string axisName)
    {
        _lookInput = _touchInput.PlayerJoystickOutputVector();

        if (axisName == _touchXMapTo)
            return _lookInput.x / _touchSpeedSensitivityX;

        if (axisName == _touchYMapTo)
            return _lookInput.y / _touchSpeedSensitivityY;

        return Input.GetAxis(axisName);
    }
}

