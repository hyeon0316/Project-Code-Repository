using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class NpcMove : MonoBehaviour
{

	Vector3 maxPos;
	Vector3 minPos;
	Vector3 targetPos;
	public float xVar;
	public float zVar;
	public float speed;
	public int state;
	Animator animator;
	// Use this for initialization

	void Start()
	{
		maxPos = new Vector3(this.transform.position.x + xVar, this.transform.position.y, this.transform.position.z + zVar);
		minPos = new Vector3(this.transform.position.x - xVar, this.transform.position.y, this.transform.position.z - zVar);
		targetPos.y = this.transform.position.y;
		state = 1;
		targetPos.x = Random.Range(maxPos.x, minPos.x);
		targetPos.z = Random.Range(maxPos.z, minPos.z);
		targetPos.y = 0;
		animator = GetComponentInChildren<Animator>();
		StartCoroutine(SetState());
	}



	// Update is called once per frame

	void Update()
	{
		if (state == 1&&!Player.inst.gameManager.isAction) //움직임
		{
			animator.SetBool("isMove", true);
			Vector3 dir;
			dir = targetPos - transform.position;
			dir.y = 0;
			transform.forward = dir;
			
			transform.position += dir.normalized * Time.deltaTime * speed;
			if (Vector3.Distance(transform.position, targetPos) < 2f)
			{
				animator.SetBool("isMove", false);
				//Debug.Log(Vector3.Distance(transform.position, targetPos));
				state = 0;
			}
		}
		else
        {
			animator.SetBool("isMove", false);
		}
	}
    private void OnTriggerEnter(Collider other)
    {
		if(other.tag == "Map")
			state = 0;
    }
    IEnumerator SetState()
	{
		float a = Random.Range(10, 7);
		yield return new WaitForSeconds(a);
		state = 1;
		if (state == 1)
		{
			targetPos.x = Random.Range(maxPos.x, minPos.x);
			targetPos.z = Random.Range(maxPos.z, minPos.z);
			targetPos.y = this.transform.position.y;
		}
		StartCoroutine(SetState());
	}



}