using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float Power;

    public float Speed = 2f;

    private float _offTime;
    // Start is called before the first frame update
    public void OnEnable()
    {
        _offTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _offTime += Time.deltaTime;
        if(_offTime > 20f)
        {
            this.gameObject.SetActive(false);
        }
        this.transform.Translate(Vector3.left* Speed * Time.deltaTime);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<I_hp>().Gethit(Power , 1);
            this.gameObject.SetActive(false);
        }
    }

}
