using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class action : MonoBehaviour
{
    public Animator Animator;


    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("w"))
        {
            Animator.SetBool("run",true);
        }
        if(Input.GetKeyUp("w"))
        {
            Animator.SetBool("run", false);    

        }
    }
}
