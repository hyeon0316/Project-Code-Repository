using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceCharacter : MonoBehaviour
{
    public int NetworkID { get; set; }
    
    /// <summary>
    /// 캐릭터 선택 완료했는지 에 대한 bool값
    /// </summary>
    public bool IsReady { get; set; }
    
    [Header("선택시 표시 될 자신의 캐릭터")]
    [SerializeField] private Image _pickImage;
    
    [Header("표시되는 캐릭터 목록")]
    [SerializeField] private Sprite[] _sprite;

    [SerializeField] private TextMeshProUGUI _nickNameText;

    [SerializeField] private GameObject _selectDone;
    public Image PickImage => _pickImage;


    /// <summary>
    /// 캐릭터 선택시 픽창에 표시
    /// </summary>
    public void ChangeCharacterImage(int type)
    {
        if(!IsReady)
            _pickImage.color = new Color(255, 255, 255, 1);
        
        _pickImage.sprite = _sprite[type];
    }

    public void Init()
    {
        SelectDone(false);
    }

    /// <summary>
    /// 선택을 확정하기 전과 후
    /// </summary>
    public void SelectDone(bool isDone)
    {
        _pickImage.color = isDone ? new Color(110/255f, 110/255f, 110/255f, 255/255f) : new Color(255, 255, 255, 0);
        _selectDone.SetActive(isDone);
    }
    
    /// <summary>
    /// 유저가 지정한 닉네임으로 설정
    /// </summary>
    public void SetUserNickName(string nickName)
    {
        _nickNameText.text = nickName;
    }

}
