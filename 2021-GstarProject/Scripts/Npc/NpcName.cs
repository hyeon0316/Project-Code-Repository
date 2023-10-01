using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NpcName : MonoBehaviour
{
    public Canvas nameCanvas;
    public Vector3 nameOffset = new Vector3(0f, 5f, 0);
    public GameObject nameText;
    public TextMeshProUGUI nameObject;

    // Start is called before the first frame update
    void Start()
    {
        SetName();
    }

    void SetName()
    {
        nameCanvas = GameObject.Find("NameCanvas").GetComponent<Canvas>();
        GameObject namePrefab = Instantiate<GameObject>(nameText, transform.position, Quaternion.identity, nameCanvas.transform);
        var _namePrefab = namePrefab.GetComponent<EnemyHpBar>();

        _namePrefab.enemyTr = this.gameObject.transform;
        _namePrefab.offset = nameOffset;

        nameObject = _namePrefab.GetComponent<TextMeshProUGUI>();
    }
}
