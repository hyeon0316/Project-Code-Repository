using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyIcons
{
    Arrow,
    AttackZ,
    AttackX,
    Jump,
    Shift,
    SkillA,
    SkillS,
    SkillD,
}
public class Tutorial : MonoBehaviour
{
    private Player _player;
    public GameObject[] Keys;
    public GameObject ManualWindow;
    public GameObject PracticeEnemy;
    public GameObject PlayerUICanvas;
    public Item DropItem;
    
    private void Awake()
    {
        _player = FindObjectOfType<Player>();
    }

    private void Start()
    {
        Keys[(int)KeyIcons.Arrow].SetActive(true);
    }

    private void Update()
    {
        TargetPlayer();
        SwitchOffKey();
    }

    private void SwitchOffKey()
    {
        if (Keys[(int) KeyIcons.AttackZ].activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Keys[(int) KeyIcons.AttackZ].SetActive(false);
            }
        }
        else if (Keys[(int) KeyIcons.AttackX].activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                Keys[(int) KeyIcons.AttackX].SetActive(false);
            }
        }
        else if (Keys[(int) KeyIcons.Jump].activeSelf)
        {
            if (Input.GetKey(KeyCode.C))
            {
                Keys[(int) KeyIcons.Jump].SetActive(false);
            }
        }
        else if (Keys[(int) KeyIcons.Shift].activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Keys[(int) KeyIcons.Shift].SetActive(false);
            }
        }
        else if (Keys[(int) KeyIcons.SkillA].activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Keys[(int) KeyIcons.SkillA].SetActive(false);
            }
        }
        else if (Keys[(int) KeyIcons.SkillS].activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Keys[(int) KeyIcons.SkillS].SetActive(false);
            }
        }
        else if (Keys[(int) KeyIcons.SkillD].activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                Keys[(int) KeyIcons.SkillD].SetActive(false);
            }
        }
    }

    /// <summary>
    /// 플레이어 머리 위에 나타내기 위함
    /// </summary>
    private void TargetPlayer()
    {
        for (int i = 0; i < Keys.Length; i++)
        {
            if (Keys[i].activeSelf)
            {
                Keys[i].transform.position = _player.transform.position + Vector3.up;
                break;
            }
        }
    }

    public void CloseButton()
    {
        ManualWindow.SetActive(false);
    }

}
