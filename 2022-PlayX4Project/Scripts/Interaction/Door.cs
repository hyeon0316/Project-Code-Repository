using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interaction
{
    public bool CanOpen;

    // Update is called once per frame
    void Update()
    {
        StartInteract();
    }

    public override void StartInteract()
    {
        if (CanInteract)
        {
            ActionBtn.transform.position = this.GetComponent<BoxCollider>().transform.position + new Vector3(-0.7f, 1f, -0.5f);

            if (CanOpen)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    FindObjectOfType<SoundManager>().Play("Object/Door", SoundType.Effect);
                    FindObjectOfType<Inventory>().DeleteMaterial();
                    this.GetComponent<Animator>().SetTrigger("OpenDoor");
                    this.GetComponent<BoxCollider>().enabled = false;
                    ActionBtn.SetActive(false);
                    CanInteract = false;
                }
            }
            else if(!CanOpen)
            {
                //todo:열 수 없다는 대사만 띄우기
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    FindObjectOfType<SoundManager>().Play("Object/LockDoor", SoundType.Effect);
                    Debug.Log("Lock");
                }
            }
        }
    }
}
