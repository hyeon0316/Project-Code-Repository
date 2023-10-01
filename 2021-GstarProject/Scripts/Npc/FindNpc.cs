using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FindNpc : MonoBehaviour
{
    public static FindNpc inst = null;
    public GameObject _target;
    public Sprite[] _sprite;//화살표, 별표
    public bool npcorMap;
    // Start is called before the first frame update
    void Awake()
    {
        if (inst == null)
            inst = this;
        npcorMap = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (npcorMap)
        {
            if (GameObject.Find("PowerIconRandomv2"))
            {
                this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = _sprite[0];
                GameObject _target1 = GameObject.Find("PowerIconRandomv2");
                Vector3 targetPosition;
                targetPosition = new Vector3(_target1.transform.position.x, transform.position.y, _target1.transform.position.z);
                transform.LookAt(targetPosition);
            }
            else
            {
                this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = _sprite[1];
            }
        }
        else
        {
            if (_target != null)
            {
                this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = _sprite[0];
                Vector3 targetPosition;
                targetPosition = new Vector3(_target.transform.position.x, transform.position.y, _target.transform.position.z);
                transform.LookAt(targetPosition);
            }
            else
            {
                this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = _sprite[1];
            }
        }            
    }
}
