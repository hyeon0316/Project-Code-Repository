using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DropItemInfo
{
    public Item DropItem;
    public int DropItemCount;
}

public sealed class Chest : Interaction
{
    [SerializeField] DropItemInfo[] _dropItems;
    [SerializeField] private GameObject _dropItemObj;
    [SerializeField] private Transform _dropObjPos;

    public override void StartInteract()
    {
        SoundManager.Instance.Play("Object/Chest", SoundType.Effect);
        this.GetComponent<BoxCollider>().enabled = false;
        this.GetComponent<Animator>().SetTrigger("Open");

        GetDropItems();
    }

    private void GetDropItems()
    {
        foreach (var item in _dropItems)
        {
            Instantiate(_dropItemObj, _dropObjPos.position, _dropObjPos.rotation);
            PlayerManager.Instance.Inventory.AddItem(item.DropItem, item.DropItemCount);
        }
    }
}
