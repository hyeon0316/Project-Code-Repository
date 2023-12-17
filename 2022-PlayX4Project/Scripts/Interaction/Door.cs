using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Door : Interaction
{
    public override void StartInteract()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (PlayerManager.Instance.Inventory.CheckSecretKey())
            {
                SoundManager.Instance.Play("Object/Door", SoundType.Effect);
                this.GetComponent<Animator>().SetTrigger("OpenDoor");
                this.GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                SoundManager.Instance.Play("Object/LockDoor", SoundType.Effect);
                Debug.Log("Lock");
            }
        }
    }
}
