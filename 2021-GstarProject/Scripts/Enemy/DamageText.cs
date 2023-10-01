using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    private RectTransform rectParent; //부모의 rectTransform 변수를 저장할 변수
    private RectTransform rectDamage;
    public Vector3 offset = Vector3.zero;
    public Transform enemyTr; //적 캐릭터의 위치
    private Camera uiCamera;
    private Canvas canvas;
    private float moveSpeed;
    private float alphaSpeed;
    private float destroyTime;
    TextMeshProUGUI text;
    Color alpha;
    public float damage;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponentInParent<Canvas>(); //부모가 가지고있는 canvas 가져오기, Enemy HpBar canvas임
        uiCamera = canvas.worldCamera;
        rectParent = canvas.GetComponent<RectTransform>();
        rectDamage = this.gameObject.GetComponent<RectTransform>();

        moveSpeed = 2.0f;
        alphaSpeed = 2.0f;
        destroyTime = 2.0f;

        text = GetComponent<TextMeshProUGUI>();
        alpha = text.color;
        text.text = damage.ToString();
        Invoke("DestroyObject", destroyTime);      
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyTr != null)
        {
            var screenPos = Camera.main.WorldToScreenPoint(enemyTr.position + offset);
            var localPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectParent, screenPos, uiCamera, out localPos); //
            rectDamage.localPosition = localPos; 
        }

        //transform.Translate(new Vector2(0, moveSpeed * Time.deltaTime)); // 텍스트 위치

        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaSpeed); // 텍스트 알파값
        text.color = alpha;
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}
