using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public sealed class Demon : Enemy, IEnemyMove, IEnemyAttack
{
    private Enemystate _state;
    private EnemyAttack _enemyAttack;
    private NavMeshAgent _enemyNav;
    public float Attackcrossroad;
    private float _attackDelay;

    private SpriteRenderer _spriteRenderer;
    public Material HitMaterial;
    private Material _defaultMaterial;

    public Transform PollParent;
    
    public GameObject FireCollect;
    public GameObject BombEffect;
    public GameObject FireBall;
    
    private int _bombCount;
    private Queue<GameObject> _poolingBomb = new Queue<GameObject>();
    private Queue<GameObject> _poolingEffect = new Queue<GameObject>();
    private Queue<GameObject> _poolingFireBall = new Queue<GameObject>();
    private List<GameObject> _useBomb = new List<GameObject>();
    private float _teleportTimer;
    
    private float _areaSkillTimer;

    private float _launchSkillTimer;

    protected override void Awake()
    {
        base.Awake();
        _enemyAttack = this.GetComponentInChildren<EnemyAttack>();
        _enemyNav = this.GetComponent<NavMeshAgent>();
        _enemyNav.stoppingDistance = Attackcrossroad;

        _spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        _defaultMaterial = _spriteRenderer.material;
        PollParent = GameObject.Find("BossEnemypollParent").transform;
        InitBomb(10);
        InitFireBall(20);
        _state = Enemystate.Idle;
    }


    // Update is called once per frame
    void Update()
    {
        Debug.Log(_state);
        if (_state != Enemystate.Dead && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Start"))
        {
            if (_attackDelay > 0)
                _attackDelay -= Time.deltaTime;

            LookPlayer();
            ChangeState();
        }
        Moving();
    }

    public void OnDestroy()
    {
        PlayerManager.Instance.Inventory.AddMaterial("EndingKey");
    }

    private void InitBomb(int initCount)
    {
        for (int i = 0; i < initCount; i++)
        {
            _poolingBomb.Enqueue(CreateNewBomb());
            _poolingEffect.Enqueue(CreateNewEffect());
        }
    }

    private void InitFireBall(int initCount)
    {
        for (int i = 0; i < initCount; i++)
        {
            _poolingFireBall.Enqueue(CreateNewFireBall());
        }
    }
    private GameObject CreateNewBomb()
    {
        var newBomb = Instantiate(FireCollect, PollParent);
        newBomb.name = "Bomb";
        newBomb.gameObject.SetActive(false);

        return newBomb;
    }

    private GameObject CreateNewEffect()
    {
        var newEffect = Instantiate(BombEffect, PollParent);
        newEffect.name = "Effect";
        newEffect.gameObject.SetActive(false);

        return newEffect;
    }

    private GameObject CreateNewFireBall()
    {
        var newFireBall = Instantiate(FireBall, PollParent);
        newFireBall.name = "FireBall";
        newFireBall.gameObject.SetActive(false);

        return newFireBall;
    }

    private void ReturnBomb(GameObject bomb)
    {
        bomb.gameObject.SetActive(false);
        _poolingBomb.Enqueue(bomb);
        
    }
    public void ReturnEffect(GameObject effect)
    {
        effect.gameObject.SetActive(false);
        _poolingEffect.Enqueue(effect);
    }

    public void ReturnFireBall(GameObject fireBall)
    {
        fireBall.gameObject.SetActive(false);
        _poolingFireBall.Enqueue(fireBall);

    }
    
    private void DropBomb()
    {
        int rand = Random.Range(0, 2);
        if (rand == 1)
        {
           if(_useBomb.Count < 2)
            ++_bombCount;
            if(_bombCount >= 3) { 
                for(int i= 0;i< 2; i++) {
                    if (_poolingBomb.Count > 0)
                    {
                        var obj = _poolingBomb.Dequeue();
                        _useBomb.Add(obj);
                        obj.gameObject.SetActive(true);
                        StartCoroutine(DropPosCo(obj));
                    }
                   
                }
            }
            else
            {
                return;
            }
        }

        if (_bombCount >=3)
        {
            _enemyNav.isStopped = true;
            _state = Enemystate.Skill2;
            StartCoroutine(BombSkillCo());
            _bombCount = 0;
        }
    }

    /// <summary>
    /// 폭탄이 자기 위치 기준으로 시작하여 랜덤 방향으로 드롭되도록 하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator DropPosCo(GameObject bomb)
    {
        float time = 0;
        var pos1 = transform.position;
        var pos = transform.position + (Vector3.right *Random.Range(-1,1) * 3f) + Vector3.up;
        while (time <= 1f)
        {
            time += Time.deltaTime;
            yield return new WaitForFixedUpdate();
            bomb.transform.position = Vector3.Slerp(pos1, pos, time);
        }
    }

    private IEnumerator BombSkillCo()
    {
        yield return new WaitForSeconds(1f);
        float time = 0;
        
        while (time <= 0.5f)
        {
            for (int i = 0; i < _useBomb.Count; i++)
            {
                time += Time.deltaTime;
                
                _useBomb[i].transform.position = Vector3.Slerp(_useBomb[i].transform.position,PlayerManager.Instance.Player.transform.position + Vector3.down, 0.1f);
                yield return new WaitForFixedUpdate();
            }
        }
        //공중에 있는 폭탄이 플레이어를 약간 추적후 폭팔한다.

        _anim.SetTrigger("Skill1");
        yield return new WaitForSeconds(1f);
        Debug.Log("폭발!");
        
        for (int i = 0; i < 2; i++)
        {
            var effectObj = _poolingEffect.Dequeue();   
            effectObj.transform.position = GameObject.Find("Bomb").transform.position;
            effectObj.gameObject.SetActive(true);
            
            ReturnBomb(GameObject.Find("Bomb"));
        }
        _bombCount = 0;
        _useBomb.Clear();
        _enemyNav.isStopped = false;
        _state = Enemystate.Find;
    }
    

    private void ChangeState()
    {
        if (!_anim.GetCurrentAnimatorStateInfo(0).IsName("Skill1") && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Skill2") &&
            !_anim.GetCurrentAnimatorStateInfo(0).IsName("Skill3"))
        {
            if (_state != Enemystate.Skill2)
            {
                if(Vector3.Distance(PlayerManager.Instance.Player.transform.position, this.transform.position) < 5f) { 
                    if (Vector3.Distance(PlayerManager.Instance.Player.transform.position, this.transform.position) < Attackcrossroad + 0.25f)
                    {
                        _areaSkillTimer += Time.deltaTime;
                        _teleportTimer = 0;
                        if (_attackDelay <= 0)
                        {
                            _areaSkillTimer = 0f;
                            _state = Enemystate.Attack;
                        }
                        else
                        {
                            _state = Enemystate.Idle;
                        }
                    }
                    else
                    {
                        _state = Enemystate.Find;
                    }
                }
                else
                {
                     _teleportTimer += Time.deltaTime;
                     _areaSkillTimer = 0;

                     if (_teleportTimer >= 5f)
                     {
                       // this.transform.position = PlayerObj.transform.position;
                       _state = Enemystate.Find;
                       
                     }else {
                            if(Mathf.Abs(PlayerManager.Instance.Player.transform.position.z - transform.position.z ) < 0.3f) { 
                                _state = Enemystate.Range;
                            }
                            else
                            {
                                _state = Enemystate.Find;
                                //_teleportTimer = 0;
                            }
                    }
                }
            }
        }
        
        if (_areaSkillTimer >= 2f)
        {
            PlayerManager.Instance.Player.KnockBack(this.transform.position);
            _state = Enemystate.Skill;
            _anim.SetTrigger("Skill2");
            _areaSkillTimer = 0;
        }
    }
    public void Moving()
    {
        if (_state == Enemystate.Find)
        {
            _enemyNav.isStopped = false;
            _enemyNav.speed = Speed;
            _enemyNav.SetDestination(PlayerManager.Instance.Player.transform.position);
            _anim.SetBool("IsWalk", true);
        }
        else if (_state == Enemystate.Attack)
        {
            _enemyNav.isStopped = true;
            _anim.SetBool("IsWalk", false);
            _anim.SetTrigger("Attack");
            _attackDelay = 3f;
        }
        else if (_state == Enemystate.Idle)
        {
            _enemyNav.isStopped = true;
            _anim.SetBool("IsWalk", false);
        }
        else if (_state == Enemystate.Dead)
        {
            _anim.SetBool("IsWalk", false);
            _anim.SetTrigger("Dead");
            //StartCoroutine(DeadAniPlayer());
        }
        else if (_state == Enemystate.Skill)
        {
            _anim.SetBool("IsWalk", false);
            _attackDelay = 3f;
            _enemyNav.isStopped = true;
        }
        else if (_state == Enemystate.Skill2)
        {
            
            
        }else if(_state == Enemystate.Range)
        {
            _anim.SetTrigger("Fire1");
            _attackDelay = 3f;
            _enemyNav.isStopped = true;
            
        }
    }
    private IEnumerator BloodCo()
    {
        GameObject obj = Instantiate(CachingManager.Instance().BloodObj, this.transform.position, Quaternion.identity);
        obj.transform.localScale = new Vector3(1f, 1f, 1f);
        yield return new WaitForSeconds(0.7f);
        Destroy(obj);
    }
    
    private IEnumerator HitCo()
    {
        _spriteRenderer.material = HitMaterial;
        yield return new WaitForSeconds(0.2f);
        _spriteRenderer.material = _defaultMaterial;
    }
    
    /// <summary>
    /// 실제 데미지를 주는 함수(공격 애니메이션 따로)
    /// </summary>
    public void Attack(float dmg)
    {
        if (_enemyAttack.IshitPlayer)
        {
            Debug.LogFormat("{0},{1}", this.name, "hit");
            if(_state == Enemystate.Skill)PlayerManager.Instance.Player.KnockBack(this.transform.position);
           PlayerManager.Instance.Player.GetDamage(Power,dmg);
        }
    }

    /// <summary>
    /// 원거리 공격
    /// </summary>
    public void LaunchFireBall()
    {
        var fireBallObj = _poolingFireBall.Dequeue();
        if(this.transform.GetChild(0).localScale.x < 0)
        {
            fireBallObj.transform.rotation = new Quaternion(0,180,0,0);
        }
        else
        {
            fireBallObj.transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        fireBallObj.transform.position = GameObject.Find("FirePos").transform.position;
        fireBallObj.gameObject.SetActive(true);
    }
    
    private void LookPlayer()
    {
        if (!_anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Skill1") &&
            !_anim.GetCurrentAnimatorStateInfo(0).IsName("Skill2") && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Skill3"))
        {
            Vector3 thisScale = new Vector3(2.5f, 2.5f, 1);
            if (PlayerManager.Instance.Player.transform.position.x > this.transform.position.x)
            {
                this.transform.GetChild(0).localScale = new Vector3(-thisScale.x, thisScale.y, thisScale.z);
            }
            else
            {
                this.transform.GetChild(0).localScale = new Vector3(thisScale.x, thisScale.y, thisScale.z);
            }
        }
    }

    protected override void CallDamageEvent()
    {
        int rand = Random.Range(0, 2);
        switch (rand)
        {
            case 0:
                SoundManager.Instance.Play("Enemy/Demon/DemonHit1", SoundType.Effect);
                break;
            case 1:
                SoundManager.Instance.Play("Enemy/Demon/DemonHit2", SoundType.Effect);
                break;
        }
        StartCoroutine(HitCo());
        DropBomb();
    }

    protected override IEnumerator DeadEventCo()
    {
        GameEvent.CallEndingChest();
        PollParent.gameObject.SetActive(false);
        _enemyNav.path.ClearCorners();
        _enemyNav.enabled = false;
        while (true)
        {
            _enemyNav.path.ClearCorners();
            _enemyNav.enabled = false;
            if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Dead")
                && _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        _enemyNav.enabled = false;
        Destroy(this.transform.gameObject, Time.deltaTime);
    }

}
