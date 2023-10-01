using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interaction
{
    public Item DropItem;
    public int ItemCount;

    private GameObject _dropItem;
    private Transform _dropPos;
    private FadeImage _fade;
    
    protected override void Awake()
    {
        base.Awake();
        _dropItem = Resources.Load<GameObject>("Prefabs/DropItem");
        _dropPos = this.transform.GetChild(0).GetComponent<Transform>();
        _fade = GameObject.Find("Canvas").transform.Find("FadeImage").GetComponent<FadeImage>();
    }

    private void Update()
    {
        StartInteract();
    }
    
    public override void StartInteract()
    {
        if (CanInteract)
        {
            ActionBtn.SetActive(true);
            ActionBtn.transform.position = this.transform.position + new Vector3(0f, 1.5f, 0.5f);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                FindObjectOfType<SoundManager>().Play("Object/Chest",SoundType.Effect);
                ActionBtn.SetActive(false);
                this.GetComponent<BoxCollider>().enabled = false;
                this.GetComponent<Animator>().SetTrigger("Open");
                FindObjectOfType<Inventory>().AddUsed(DropItem, ItemCount);

                for (int i = 0; i < ItemCount; i++)
                {
                    Instantiate(_dropItem, _dropPos.position, _dropPos.rotation);
                }

                if(GameObject.Find("Colliders").transform.Find("Collider_SecondFloor").gameObject.activeSelf)
                {
                    FindObjectOfType<Player>().PlayerAnim.SetBool("IsRun", false);
                    FindObjectOfType<Inventory>().AddMaterial("SecretKey");
                    FindObjectOfType<Door>().CanOpen = true;
                    FindObjectOfType<Player>().IsStop = true;
                    _fade.FadeIn();
                    Invoke("DelayFadeOut", 2f);
                }
                CanInteract = false;
            }
        }
    }

    private void DelayFadeOut()
    {
        FindObjectOfType<Player>().transform.position = GameObject.Find("RevivalPos").transform.position;
        FindObjectOfType<CameraManager>().transform.position = FindObjectOfType<Player>().transform.position + Vector3.up;
        _fade.FadeOut();
        FindObjectOfType<Player>().IsStop = false;
        
        FindObjectOfType<NavSpawner>().SpawnNav(FindObjectOfType<GameManager>().Walf[4].transform.position);
    }
}
