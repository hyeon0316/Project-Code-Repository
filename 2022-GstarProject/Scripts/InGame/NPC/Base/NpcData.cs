using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcData : MonoBehaviour
{
    public int ID;
    public string Name;
    public GameObject Questing;
    public void OnQeust() => Questing.SetActive(true);
    public void OffQeust() => Questing.SetActive(false);
    [SerializeField]private HpbarController _hp;
    private void Awake()
    {
        _hp.SetNpc(Name);
    }
}
