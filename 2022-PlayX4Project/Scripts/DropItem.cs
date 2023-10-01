using System.Collections;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    private bool _canGet;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ComeOutChestCo());
    }

    // Update is called once per frame
    void Update()
    {
        MovetoPlayer();
    }

    private void MovetoPlayer()
    {
        if (_canGet)
        {
            this.transform.position += (FindObjectOfType<Player>().transform.position - this.transform.position) * 0.03f;

            if (Vector3.Distance(this.transform.position, FindObjectOfType<Player>().transform.position) < 0.4f)
            {
                Destroy(this.gameObject);
                _canGet = false;
            }
        }
    }

    private IEnumerator ComeOutChestCo()
    {
        Vector3 startPos = this.transform.position;
        Vector3 goalPos = startPos + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.5f, 1f), Random.Range(-0.3f, 0.3f));

        float time = 0f;
        
        while (true)
        {
            time += Time.deltaTime;
            this.transform.position = Vector3.Lerp(startPos, goalPos, time);
            if (time >= 1f)
            {
                this.transform.position = goalPos;
                break;
            }
            yield return null;
        }

        _canGet = true;
        yield return null;
    }

   

    private void DelayGetItem()
    {
        _canGet = true;
    }
}
