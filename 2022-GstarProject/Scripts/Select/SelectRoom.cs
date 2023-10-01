using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum JobType
{
    Knight,
    Mage
}
public class SelectRoom : MonoBehaviour
{
    private JobType _selectJobType;
    
    [Header("캐릭터 상세 정보")]
    [SerializeField] private TextMeshProUGUI _jobNameText;
    [SerializeField] private TextMeshProUGUI _jobDescriptionText;

    [SerializeField] private LoadingSceneController _loading;
    [SerializeField] private GameObject _guide;

    private IEnumerator _guideCo;
    /// <summary>
    /// 선택한 캐릭터에 대한 정보를 보여줌
    /// </summary>
    public void SetCharacterInfo(SelectCharacter character)
    {
        _jobNameText.text = character.JobName;
        _jobDescriptionText.text = character.JobDescription;

        _selectJobType = character.CurJobType;
    }


    /// <summary>
    /// 최종선택한 캐릭터에 대한 데이터를 저장
    /// </summary>
    public void SaveCharacterInfo()
    {
        if (_selectJobType == JobType.Knight)
        {
            if (_guideCo == null)
            {
                _guideCo = SetGuideCo();
                StartCoroutine(_guideCo);
            }
        }
        else
        {
            DataManager.Instance.SelectJobType = _selectJobType;
            _loading.LoadAsyncScene();
        }
    }

    private IEnumerator SetGuideCo()
    {
        WaitForSeconds delay = new WaitForSeconds(1f);
        _guide.SetActive(true);
        yield return delay;
        _guide.SetActive(false);
        _guideCo = null;

    }
    
}
