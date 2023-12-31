using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    [Range(0.1f,1)]
    [SerializeField] private float _doSpeed;

    private SkinnedMeshRenderer meshRenderer;

    private void Awake(){
        meshRenderer = this.GetComponent<SkinnedMeshRenderer>();
    }

    /// <summary>
    /// 물체를 서서히 나타낸다.
    /// </summary>
    public void DissolveIn()
    {
        meshRenderer.material.SetFloat("_Cutoff", 1);
        meshRenderer.material.DOFloat(0, "_Cutoff", _doSpeed + 2);
    }

    /// <summary>
    /// 물체를 서서히 지운다.
    /// </summary>
    public async UniTask DissolveOutAsync()
    {
        await meshRenderer.material.DOFloat(1, "_Cutoff", _doSpeed);
    }
}