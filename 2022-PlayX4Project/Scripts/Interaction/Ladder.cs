using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : Interaction
{

    public Player Player;

    private static bool _isLadder;//사다리를 탈 수 있을때

    protected override void Awake()
    {
        base.Awake();
        Player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void Update()
    {
        StartInteract();
    }
    
    public override void StartInteract()
    {
        if (CanInteract)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Player.GetComponent<Rigidbody>().velocity = Vector3.zero;
                Player.ChangeLadder(this.gameObject, true);
                Invoke("LadderOn", 0.1f);
            }

            if (_isLadder)
            {
                Debug.Log("사다리에서 탈출");
                ActionBtn.gameObject.SetActive(false);
                Player.ChangeLadder(this.gameObject, false);
                _isLadder = false;
            }
            else
            {
                ActionBtn.gameObject.SetActive(true);
                ActionBtn.transform.position = this.transform.position + new Vector3(0f, -1f, -0.5f);
            }
        }
    }

    private void LadderOn()
    {
        Debug.Log("참");
        _isLadder = true;
    }
    
}
