using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control : MonoBehaviour
{
    [SerializeField] private GameObject[] _controlPages;

    private int _pageIndex;

    private void Awake()
    {
        SetControlPage(0);

    }

    /// <summary>
    /// 왼쪽 화살표 클릭
    /// </summary>
    public void SetLeftPage()
    {
        _pageIndex--;
        if (_pageIndex < 0)
            _pageIndex = 4;
        
        SetControlPage(_pageIndex);
    }
    
    /// <summary>
    /// 오른쪽 화살표 클릭
    /// </summary>
    public void SetRightPage()
    {
        _pageIndex++;
        if (_pageIndex >= _controlPages.Length)
            _pageIndex = 0;
        SetControlPage(_pageIndex);
    }

    private void SetControlPage(int index)
    {
        for (int i = 0; i < _controlPages.Length; i++)
        {
            _controlPages[i].SetActive(i == index);
        }
    }
}
