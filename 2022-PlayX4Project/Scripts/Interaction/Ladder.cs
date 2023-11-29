using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : Interaction
{
    private static bool _isLadder = false;//사다리를 탈 수 있을때

    public override void StartInteract()
    {
        _isLadder = !_isLadder;
        PlayerManager.Instance.Player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        PlayerManager.Instance.Player.ChangeLadder(this.gameObject, _isLadder);
    }
}
