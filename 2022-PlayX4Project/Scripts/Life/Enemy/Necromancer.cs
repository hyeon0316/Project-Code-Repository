using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

public class Necromancer : Enemy, IEnemyMove
{ 
    [SerializeField] private string[] _eventSentences;

    private NavMeshAgent _enemyNav;
    private int _selectPattern;
    private bool _canSpecialSummon;
    public GameObject Portal;
    public static bool IsSkill;
    public float summonTime;

    protected override void Awake()
    {
        base.Awake();
        summonTime = 10f;
        _canSpecialSummon = true;
        _enemyNav = this.GetComponent<NavMeshAgent>();
        Portal = GameObject.Find("PortalParent");
    }

    private void Start()
    {
        Moving();
    }

    private void Update()
    {
        LookPlayer();
        Summon();
        if (_canSpecialSummon && Hp <= MaxHp / 2)
        {
            StartCoroutine(SpecialSummon());
            _canSpecialSummon = false;
        }
    }


    public void Summon()
    {
        if (!DialogueManager.Instance.IsTalking)
        {
            summonTime -= Time.deltaTime;
            if (summonTime < 0)
            {
                summonTime = 7f;
                StartCoroutine(NomalSummon());
            }
        }
    }

    /// <summary>
    /// 모든 행동패턴
    /// </summary>
    public void Moving()
    {
        if (DialogueManager.Instance.IsTalking)
        {
            _selectPattern = 1;
        }
        else
        {
            _selectPattern = Random.Range(-1, 4);
        }

        switch (_selectPattern)
        {
            case -1:
            case 0:
                Debug.Log("추적");
                _enemyNav.SetDestination(PlayerManager.Instance.Player.transform.position);
                _enemyNav.isStopped = false;
                _anim.SetBool("IsWalk", true);
                break;
            case 1:
                Debug.Log("정지");
                _enemyNav.isStopped = true;
                _anim.SetBool("IsWalk", false);
                break;
            case 2:
                if (Hp <= MaxHp / 2)
                {
                    Debug.Log("회복");
                    StartCoroutine(Heal());
                }
                break;
            default:
                break;
        }
        Invoke(nameof(Moving), 2.5f);
    }

    private void LookPlayer()
    {
        if (!_anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Skill"))
        {
            if (!_anim.GetCurrentAnimatorStateInfo(0).IsName("Stop") && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Dead"))
            {
                if (PlayerManager.Instance.Player.transform.position.x > this.transform.position.x)
                {
                    transform.GetComponentInChildren<SpriteRenderer>().flipX = false;
                }
                else
                {
                    transform.GetComponentInChildren<SpriteRenderer>().flipX = true;
                }
            }
        }
    }
    
    /// <summary>
    /// 일반몬스터 4마리 중 랜덤 소환
    /// </summary>
    private IEnumerator NomalSummon()
    {
        _enemyNav.isStopped = true;
        _anim.SetBool("IsWalk", false);
        _anim.SetTrigger("Skill2");
        yield return new WaitForSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);
        Portal.transform.GetChild(0).gameObject.SetActive(true);
        _anim.SetBool("IsWalk", true);
    }

    /// <summary>
    /// 중간보스 소환(한번만 사용 예정)
    /// </summary>
    private IEnumerator SpecialSummon()
    {
        IsSkill = true;
        CancelInvoke(nameof(Moving));
        _enemyNav.isStopped = true;
        _anim.SetBool("IsWalk", false);
        _anim.SetTrigger("Skill1");
        yield return new WaitForSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);
        Portal.transform.GetChild(0).gameObject.SetActive(true);
        Debug.Log("특수소환!");
        Invoke(nameof(Moving), 3f);
    }

    private IEnumerator Heal()
    {
        _enemyNav.isStopped = true;
        _anim.SetBool("IsWalk", false);
        _anim.SetTrigger("Skill3");
        yield return new WaitForSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);
    }
    
    private IEnumerator BloodCo()
    {
        GameObject obj = Instantiate(CachingManager.Instance().BloodObj, this.transform.position, Quaternion.identity);
        obj.transform.localScale = new Vector3(1f, 1f, 1f);
        yield return new WaitForSeconds(0.7f);
        Destroy(obj);
    }


    protected override IEnumerator DeadEventCo()
    {
        _anim.SetTrigger("Stop");
        CancelInvoke(nameof(Moving));
        _enemyNav.isStopped = true;
        _enemyNav.enabled = false;
        DialogueManager.Instance.OnDialogue(_eventSentences.ToList());
        DialogueManager.Instance.SetPanelPos(GameObject.Find("Demon_Page1").transform.position + new Vector3(0.7f, 0.7f, 0));
        yield return new WaitForSeconds(3f);
        DialogueManager.Instance.SetActiveTalkPanel(false);
        _anim.SetTrigger("Dead");
        while (true)
        {
            _enemyNav.path.ClearCorners();
            if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Dead")
                && _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                GameObject boss2 = Resources.Load<GameObject>("Prefabs/Enemy/Demon_Page2");
                GameObject go = Instantiate(boss2, this.transform.position, this.transform.rotation);
                go.transform.parent = GameObject.Find("SummonEnemysTr").transform;
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        Destroy(this.transform.gameObject, Time.deltaTime);
    }
    

    protected override void CallDamageEvent()
    {
        throw new NotImplementedException();
    }

}
