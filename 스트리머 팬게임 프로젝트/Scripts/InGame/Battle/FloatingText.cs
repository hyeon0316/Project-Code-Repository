using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private GameObject _textPrefab;

    private Sequence _sequence;

    public void ShowFloating(int value)
    {
        if(value == 0)
            return;
        StartCoroutine(ShowDamageFloatingCo(value));
    }

    private IEnumerator ShowDamageFloatingCo(int value)
    {
        TextMeshProUGUI obj = Instantiate(_textPrefab, transform).GetComponent<TextMeshProUGUI>();
        obj.text = $"{value}"; 
        obj.transform.DOLocalMoveY(150, 1);
        obj.DOFade(1, 0.5f);
        yield return new WaitForSeconds(0.5f);
        obj.DOFade(0, 0.5f);
        yield return new WaitForSeconds(0.5f);
        Destroy(obj.gameObject);
    }

}
