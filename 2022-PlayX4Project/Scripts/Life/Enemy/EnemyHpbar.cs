using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class EnemyHpbar : MonoBehaviour
{
    void Start()
    {
        HpbarReset();
    }

    public void HpbarReset()
    {
        for(int i = 0; i < this.transform.childCount; i++)
        {
            SetMaxhp(transform.GetChild(i));
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void Onhpber(int index)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            if(i == index)
            {
                transform.GetChild(index).gameObject.SetActive(true);
                continue;
            }
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void SetMaxhp(Transform transform)
    {
        for(int i = 1; i <= 3; i++) { 
        transform.GetChild(i).GetComponent<Image>().fillAmount = 1;
        }
    }

    public void SwitchHPbar(int index,float hp,float beforehp,bool Dead = false)
    {
        switch (index)
        {
            //어쌔신
            case 1:
                Viewhpbar(0, hp, beforehp, Dead);
                break;
            //마법사
            case 2:
                Viewhpbar(1, hp, beforehp, Dead);
                break;
            //빅
            case 3:
                Viewhpbar(2, hp, beforehp, Dead);
                break;
            //촉수
            case 4:
                Viewhpbar(3, hp, beforehp, Dead);
                break;
            //중간보스
            case 10:
                Viewhpbar(4, hp, beforehp, Dead);
                break;
            //보스1페
            case 100:
                Viewhpbar(5, hp, beforehp, Dead);
                break;
            //보스 2페
            case 101:
                Viewhpbar(6, hp, beforehp, Dead);
                break;
        }
    }

    

    private void Viewhpbar(int index, float hp, float beforehp,bool Dead)
    {
        Onhpber(index);
        StopAllCoroutines();
        if (!Dead) { 
            StartCoroutine( LerpHpbar(transform.GetChild(index), hp, beforehp));
        }
        else
        {
            StartCoroutine(DeadLerpHpbar(transform.GetChild(index), hp, beforehp));
        }
    }



    IEnumerator LerpHpbar(Transform hpbar ,float hp, float beforehp)
    {
        int count = 0;
        hpbar.GetChild(3).GetComponent<Image>().fillAmount = beforehp;
        hpbar.GetChild(2).GetComponent<Image>().fillAmount = beforehp;
        
        do
        {
            if (count++ > 50)
            {
                count = 0;
                break;
            }
            hpbar.GetChild(3).GetComponent<Image>().fillAmount =Mathf.Clamp01(Mathf.Lerp( hpbar.GetChild(3).GetComponent<Image>().fillAmount, hp, 0.1f));
            hpbar.GetChild(2).GetComponent<Image>().fillAmount = Mathf.Clamp01( Mathf.Lerp( hpbar.GetChild(2).GetComponent<Image>().fillAmount, hp, 0.2f));
            yield return 0;
        } while (Mathf.Abs(hpbar.GetChild(3).GetComponent<Image>().fillAmount - hp) > 0.01f);
        do
        {
            if (count++ > 50)
            {
                count = 0;
                break;
            }
            hpbar.GetChild(1).GetComponent<Image>().fillAmount = Mathf.Clamp01(Mathf.Lerp(hpbar.GetChild(1).GetComponent<Image>().fillAmount, hp, 0.3f));
            yield return 0;
        } while (Mathf.Abs(hpbar.GetChild(1).GetComponent<Image>().fillAmount - hp) > 0.0001f);

        //yield return 0;
    }


    IEnumerator DeadLerpHpbar(Transform hpbar, float hp, float beforehp)
    {
        int count = 0;
        float _hp = hp;
       

        hpbar.GetChild(3).GetComponent<Image>().fillAmount = beforehp;
        hpbar.GetChild(2).GetComponent<Image>().fillAmount = beforehp;
        hpbar.GetChild(1).GetComponent<Image>().fillAmount = beforehp;
        do
        {
            if(count++ > 20)
            {
                count = 0;
                break;
            }
            Debug.Log("hpber1");
            hpbar.GetChild(3).GetComponent<Image>().fillAmount = Mathf.Clamp01(Mathf.Lerp(hpbar.GetChild(3).GetComponent<Image>().fillAmount, _hp, 0.1f));
            hpbar.GetChild(2).GetComponent<Image>().fillAmount = Mathf.Clamp01(Mathf.Lerp(hpbar.GetChild(2).GetComponent<Image>().fillAmount, _hp, 0.2f));
            yield return 0;
        } while (Mathf.Abs(hpbar.GetChild(3).GetComponent<Image>().fillAmount - _hp) > 0.01f);
        hpbar.GetChild(3).GetComponent<Image>().fillAmount = 0;
        hpbar.GetChild(2).GetComponent<Image>().fillAmount = 0;

        do
        {
            if (count++ > 20)
            {
                count = 0;
                break;
            }
            Debug.Log("hpber2");
            hpbar.GetChild(1).GetComponent<Image>().fillAmount = Mathf.Clamp01(Mathf.Lerp(hpbar.GetChild(1).GetComponent<Image>().fillAmount, _hp, 0.1f));
            yield return 0;
        } while (Mathf.Abs(hpbar.GetChild(1).GetComponent<Image>().fillAmount - _hp) > 0.01f);
      
        hpbar.GetChild(1).GetComponent<Image>().fillAmount = 0;
        hpbar.gameObject.SetActive(false);
        //yield return 0;
    }







}
