using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public enum ECharacterType : Byte
{
    Woowakgood,
    Ine,
    Jingburger,
    Lilpa,
    Jururu,
    Gosegu,
    Viichan,
    Empty
}

public class Character : MonoBehaviour
{

    [SerializeField]
    private ECharacterType _curCharType;

    public ECharacterType CurCharType => _curCharType;

    [Header("눌리기 전 이미지")] [SerializeField] private Sprite _normalSprite;
    [Header("눌렸을때 이미지")] [SerializeField] private Sprite _pressSprite;

    
    [SerializeField] private string _name;

    [TextArea]
    [SerializeField] private string _description;

    [SerializeField] private Image _image;


    /// <summary>
    /// 버튼이 눌리기전, 눌렸을때 스프라이트 변경
    /// </summary>
    public void SetButtonSprite(bool isActive)
    {
        Assert.IsNotNull(_image);
        Assert.IsNotNull(_normalSprite);
        Assert.IsNotNull(_pressSprite);
        _image.sprite = isActive ? _normalSprite : _pressSprite;
    }


}
