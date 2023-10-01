using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : LivingEntity
{
    public static Player inst = null;
    //인벤토리
    [SerializeField]
    public Inventory inventory;


    [SerializeField]
    private Transform chBody;

    public Slider playerHpBarSlider;
    public Slider playerMpBarSlider;
    public Text playerHpText;
    public Text playerMpText;
    public bool townS;
    private Camera camera;
    public GameObject npcCam;
    public static bool slotCountClear = false;

    public Text nowMap;
    public bool isMove;
    public bool isSkill;
    private Vector3 destination;
    public Animator animator;
    public bool attack;
    public float attackSpeed;
    public GameObject firePoint;
    public GameObject skillQFP;

    public GameObject skill_Q;
    public GameObject skill_W;
    public GameObject skill_E;
    public GameObject skill_R;

    float qMana;
    float wMana;
    float eMana;
    float rMana;


    public GameObject skill_TP;
    private float startingDP = 0;
    private float startingPower = 0;

    public float dP;
    public float power;

    private float time_Q;
    private float time_W;
    private float time_E;
    private float time_R;

    private float time_Q_1;

    public bool isSkillQ;
    public bool isSkillW;
    public bool isSkillE;
    public bool isSkillR;
    public bool isSkillTP;

    bool isGotM;

    float skill1Time = 10f;
    // Start is called before the first frame update
    public LayerMask npcLayer;
    public GameManager gameManager;
    public bool isTalk;
    public GameObject coolTimeQ;
    public GameObject coolTimeW;
    public GameObject coolTimeE;
    public GameObject coolTimeR;
    public GameObject coolTimeF;

    private GameObject tempSkill1;
    private GameObject tempSkill2;
    private float time_current;
    private float time_start;
    RaycastHit hit1;
    private int layerMask;
    float tpDis;
    public Quest questIng = null;

    public GameObject TrapTarget; //2페이지 이후 플레이어 머리 위에 해골표시

    public Text manaCaution;
    public Text coolCaution;
    private bool cautionTime;

    public Text levelText;
    int level;
    public int exp;
    public int startingEx;
    public Slider exSlider; //경험치 슬라이더
    public Text exText;//경험치 표시

    public Text questTitleText;
    public Text questProText;
    public Text questconText;
    public Rigidbody rigidbody;

    public GameObject mousePoint;
    public bool chest400;
    public bool chest500;
    public bool chest600;
    public bool chest700;

    public GameObject bloodScreen;
    public GameObject h;
    public GameObject healing;

    public GameObject levelUpEffect;
    int subExp;//받은 경험치 값이 정해진 경험치 통 값 보다 넘어갈때 그 넘어간 값을 다음 레벨때 더해줌
    private void Awake()
    {
        if (inst == null) // 싱글톤
        {
            inst = this;
        }
        rigidbody = GetComponent<Rigidbody>();
        isTalk = false;
        animator = GetComponentInChildren<Animator>();
        camera = Camera.main;
        attack = false;
        dP = startingDP;
        power = startingPower;

        time_Q = 3f;
        time_W = 10f;
        time_E = 9f;
        time_R = 25f;
        isGotM = false;
        time_Q_1 = 2f;
        isSkillQ = false;
        isSkillW = false;
        isSkillE = false;
        isSkillR = false;
        isSkillTP = true;
        tpDis = 5f;
        level = 1;
        qMana = 50;
        wMana = 100;
        eMana = 150;
        rMana = 400;
        isSkill = false;
        exp = 0;
        startingEx = 100;
        mousePoint.SetActive(false);

        chest400 = false;
        chest500 = false;
        chest600 = false;
        chest700 = false;

    }
    void Start()
    {
        QuestManager.inst.CheckQuest();
        SetLevel();
    }
    // Update is called once per frame
    void Update()
    {
        NpcS();
        if (!gameManager.isAction&&!isSkill)
        {
            GetPos();
            if (!SystemBase.gamePaused)
            {
                Move();
                Tp();
                Attack();

                if (SceneManager.GetActiveScene().name != "Town")
                {

                    SkillQ();
                    SkillW();
                    SkillE();
                    SkillR();

                }
            }
        }
        SetHpMp();
        Help();

    }
    void Help()
    {
        if (Input.GetKey(KeyCode.H)&& Input.GetKey(KeyCode.LeftControl))
        {
            TrapTarget.SetActive(false);
            isMove = false;
            isSkill = true;
            rigidbody.useGravity = false;
            animator.SetBool("isMove", false);
            rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            transform.position = new Vector3(-1.7f, 2f, 26);
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            LoadingSceneManager.LoadScene("Town");
        }
    }
    void NpcS()
    {
        if (Input.GetMouseButtonUp(1) )
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, npcLayer))
            {
                if (hit.collider.tag == "NPC")
                {
                    mousePoint.SetActive(false);
                    if (hit.collider.gameObject.GetComponent<ObjData>().isNpc ==
                        true && Vector3.Distance(transform.position,
                        hit.collider.transform.position) < 5f)
                    {
                        if (GameObject.Find("NameCanvas"))
                        {
                            h = GameObject.Find("NameCanvas");
                            h.SetActive(false);
                        }
                        if (hit.collider.gameObject.GetComponent<NpcMove>())
                            hit.collider.gameObject.GetComponent<NpcMove>().state = 0;
                        Vector3 npcVector = transform.position - hit.collider.gameObject.transform.position;
                        npcVector.x = 0;
                        npcVector.z = 0;
                        npcVector.Normalize();
                        hit.collider.gameObject.transform.LookAt(this.transform.position);
                        Quaternion q = hit.collider.gameObject.transform.rotation;
                        q.x = 0;
                        q.z = 0;
                        hit.collider.gameObject.transform.rotation = q;
                        mousePoint.SetActive(false);
                        isMove = false;
                        animator.SetBool("isMove", false);
                        npcCam.SetActive(true);
                        if (questIng != null)
                        {
                            questProText.text = "";
                            string questText;
                            questText = "";
                            foreach (var obj in questIng.collectObjectives)
                            {
                                obj.UpdateItemCount();
                                questText += obj.item.itemName + "\n" + obj.currentAmount + " / " + obj.amount + "\n";
                            }
                            questProText.text = questText;

                            if (questIng.IsCompleteObjectives && QuestManager.inst.questActionIndex < questIng.qusetComplte)
                            {
                                if (QuestManager.inst.questActionIndex != questIng.qusetComplte)
                                    QuestManager.inst.questActionIndex = questIng.qusetComplte;
                            }
                        }
                        gameManager.Action(hit.collider.gameObject);
                    }
                    else if (Vector3.Distance(transform.position,
                        hit.collider.transform.position) < 5f)
                    {
                        isMove = false;
                        animator.SetBool("isMove", false);
                        mousePoint.SetActive(false);
                        gameManager.Action(hit.collider.gameObject);
                    }
                }

                else if(hit.collider.tag == "Chest" && Vector3.Distance(transform.position,
                        hit.collider.transform.position) < 7f)
                {
                    isMove = false;
                    animator.SetBool("isMove", false);
                    mousePoint.SetActive(false);
                    gameManager.Action(hit.collider.gameObject);
                    int check1 = hit.collider.gameObject.GetComponent<ObjData>().id;
                    ObjData hitObjData = hit.collider.gameObject.GetComponent<ObjData>();
                    if (check1 == 400 && !chest400)
                    {
                        inventory.AcquireItem(hitObjData._item[0]);
                        chest400 = true;
                        questProText.text = "";
                        string questText;
                        questText = "";
                        foreach (var obj in questIng.collectObjectives)
                        {
                            obj.UpdateItemCount();
                            questText += obj.item.itemName + "\n" + obj.currentAmount + " / " + obj.amount + "\n";
                            Debug.Log(questText);
                        }
                        questProText.text = questText;
                        if (questIng.IsCompleteObjectives)
                        {
                            if (QuestManager.inst.questActionIndex != questIng.qusetComplte)
                                SoundManager.inst.SFXPlay("QuestFi", SoundManager.inst.etcList[0]);

                            FindNpc.inst.npcorMap = true;
                            NpcManager.inst.Setmap();
                            QuestManager.inst.questActionIndex = questIng.qusetComplte;
                        }
                    }
                    if (check1 == 500 && !chest500)
                    {
                        inventory.AcquireItem(hitObjData._item[0]);
                        inventory.AcquireItem(hitObjData._item[1]);
                        chest500 = true;
                    }
                    if (check1 == 600 && !chest600)
                    {
                        inventory.AcquireItem(hitObjData._item[0]);
                        inventory.AcquireItem(hitObjData._item[1]);
                        chest600 = true;
                    }
                    if (check1 == 700 && !chest700)
                    {
                        inventory.AcquireItem(hitObjData._item[0]);
                        inventory.AcquireItem(hitObjData._item[1]);
                        chest700 = true;
                    }
                   
                }

            }
        }
    }
    IEnumerator levelUp()
    {
        SoundManager.inst.SFXPlay("levelup2", SoundManager.inst.uiList[3]);
        levelUpEffect.SetActive(true);
        ++level;
        startingEx += 100;
        exp = subExp;
        startingHealth += 100;
        startingMana += 50;
        power += 10;
        health = startingHealth;
        mana = startingMana;
        dP += 5;
        yield return new WaitForSeconds(1.5f);
        levelUpEffect.SetActive(false);
        
    }
    void SetLevel()
    {
        if (startingEx <= exp) //경험치를 다채울때 레벨업(사운드)
        {
            subExp = exp - startingEx;
            StartCoroutine(levelUp());
        }
        levelText.text = string.Format("LV. {0}", level);
        if (level < 10)
        {
            exText.text = string.Format("{0}/{1}", exp, startingEx);
            exSlider.maxValue = startingEx;
            exSlider.value = exp;
        }
        else if (level == 10)
        {
            exText.text = string.Format("MAX");
            exSlider.value = startingEx;
        }
        
    }
    void SetHpMp()
    {
        playerHpBarSlider.maxValue = startingHealth;
        playerHpBarSlider.value = health;
        playerMpBarSlider.maxValue = startingMana;
        playerMpBarSlider.value = mana;
        playerHpText.text = string.Format("{0}/{1}", health, startingHealth);
        playerMpText.text = string.Format("{0}/{1}", mana, startingMana);
        if(health<startingHealth/2)
            bloodScreen.SetActive(true);
        else
            bloodScreen.SetActive(false);
        if (health < 0)
            health = 0;
        else if (mana < 0)      
            mana = 0;
        
    }
    private void OnTriggerEnter(Collider other)//아이템 획득
    {
        if (other.gameObject.tag.Equals("Item"))
        {
            inventory.AcquireItem(other.transform.GetComponent<ItemPickUp>().item);
            if (questIng != null)
            {
                questProText.text = "";
                string questText;
                questText = "";
                foreach (var obj in questIng.collectObjectives)
                {
                    obj.UpdateItemCount();
                    questText += obj.item.itemName + "\n" + obj.currentAmount + " / " + obj.amount + "\n";
                }
                questProText.text = questText;
                if (questIng.IsCompleteObjectives)
                {
                    if(QuestManager.inst.questActionIndex != questIng.qusetComplte)
                        SoundManager.inst.SFXPlay("QuestFi", SoundManager.inst.etcList[0]);
                    FindNpc.inst.npcorMap = true;
                    NpcManager.inst.Setmap();
                    QuestManager.inst.questActionIndex = questIng.qusetComplte;
                }
            }
            Destroy(other.gameObject);
        }
    }
    
    bool RectCheck(Vector3 _vector,float x1,float y1,float x2,float y2)
    {
        if(_vector.x>x1&&_vector.y<y1&&_vector.x<x2&&_vector.y>y2)
            return true;
        return false;
    }
    
    void GetPos()
    {
        if (Input.GetMouseButton(1))
        {
            
            if (Inventory.inventoryActivated && RectCheck(Input.mousePosition, 1230, 821, 1645, 251))
            {
                Debug.Log("inventory" + Input.mousePosition);
                return;
            }
            if (Information.informationActivated && RectCheck(Input.mousePosition, 215, 823, 629, 250))
            {
                Debug.Log("Information" + Input.mousePosition);
                return;
            }
            if(RectCheck(Input.mousePosition, 8, 1065, 129, 980))//조작법 버튼 누를 때
            {
                return;
            }
            if(ControlToolTip.cToolTipOn && RectCheck(Input.mousePosition, 1344, 1026, 1436, 923))//조작툴팁 x버튼 누를 때
            {
                return;
            }
            
            RaycastHit hit;
            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
            {
                mousePoint.SetActive(true);
                mousePoint.transform.position = hit.point;
                    SetDestination(hit.point);
            }
        }
    }
    void Attack()
    {
        if ((Input.GetKey(KeyCode.A) || Input.GetMouseButtonDown(0)) && Time.time >= SpawnProjectilesScript.inst.timeToFire)
        {
            Debug.Log("userSlot" + Input.mousePosition); //좌표값 체크
            if (Inventory.inventoryActivated && RectCheck(Input.mousePosition, 1230, 821, 1645, 251))
            {
                Debug.Log("inventory" + Input.mousePosition);
                return;
            }
            if (Information.informationActivated && RectCheck(Input.mousePosition, 215, 823, 629, 250))
            {
                Debug.Log("Information" + Input.mousePosition);
                return;
            }
            if (RectCheck(Input.mousePosition, 8, 1065, 129, 980))//조작법 버튼 누를 때
            {
                return;
            }
            if (ControlToolTip.cToolTipOn && RectCheck(Input.mousePosition, 1344, 1026, 1436, 923))
            {
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
            {
                SoundManager.inst.SFXPlay("Attack", SoundManager.inst.skList[5]);
                mousePoint.SetActive(false);
                var dir = hit.point - animator.transform.position;
                dir.y = 0f;
                animator.transform.forward = dir;
                firePoint.transform.forward = dir;
            }
            isMove = false;
            animator.SetBool("isMove", false);
            animator.SetTrigger("attack");

            SpawnProjectilesScript.inst.timeToFire = Time.time + attackSpeed / SpawnProjectilesScript.inst.effectToSpawn.GetComponent<ProjectileMoveScript>().fireRate;
            SpawnProjectilesScript.inst.SpawnVFX();
        }
    }
    IEnumerator Caution(Text caution)//마나부족 혹은 스킬 재사용 경고
    {
        caution.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        caution.gameObject.SetActive(false);
        cautionTime = false;
    }

    void SkillQ()
    {
        if (Input.GetKeyDown(KeyCode.Q) && isSkillQ && mana >= qMana)
        {
            mousePoint.SetActive(false);
            mana -= qMana;
            isSkillQ = false;
            StartCoroutine(SkillQCount(time_Q));
            coolTimeQ.GetComponent<CoolTime>().Reset_CoolTime(time_Q);
        }  
        else if(Input.GetKeyDown(KeyCode.Q) && !isSkillQ)
        {
            if (!cautionTime)
            {
                StartCoroutine(Caution(coolCaution));
                cautionTime = true;
                return;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            if (mana < qMana && !cautionTime)
            {
                StartCoroutine(Caution(manaCaution));
                cautionTime = true;
                return;
            }
        }
    }

    IEnumerator SkillQCount(float dealy)
    {
        RaycastHit hit;
        GameObject QQ;
        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
        {
            var dir = hit.point - animator.transform.position;
            dir.y = 0;
            animator.transform.forward = dir;
            skillQFP.transform.forward = dir;
        }
        else
        {
            QQ = Instantiate(skill_Q, skillQFP.transform.position, Quaternion.identity);
        }
        animator.SetTrigger("SkillQ");
        isMove = false;
        animator.SetBool("isMove", false);
        isSkill = true;
        yield return new WaitForSeconds(0.8f);
        SoundManager.inst.SFXPlay("Q", SoundManager.inst.skList[0]);
        isSkill = false;
        
        QQ = Instantiate(skill_Q, skillQFP.transform.position, Quaternion.identity);
        QQ.transform.forward = skillQFP.transform.forward;
        yield return new WaitForSeconds(2.5f);
        Destroy(QQ.gameObject);
        yield return new WaitForSeconds(dealy - 3.3f);

        isSkillQ = true;
        coolTimeQ.GetComponent<CoolTime>().End_CoolTime();
    }

    void SkillW()
    {
        if (Input.GetKeyDown(KeyCode.W) && isSkillW && mana >= wMana)
        {

            mousePoint.SetActive(true);
            mana -= wMana;
            isSkillW = false;
            isGotM = true;
            skill_W.SetActive(true);
            StartCoroutine(SkillWCount(time_W));
            coolTimeW.GetComponent<CoolTime>().Reset_CoolTime(time_W);
        }
        else if (Input.GetKeyDown(KeyCode.W) && !isSkillW)
        {
            if (!cautionTime)
            {
                StartCoroutine(Caution(coolCaution));
                cautionTime = true;
                return;
            }
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            if (mana < wMana && !cautionTime)
            {
                StartCoroutine(Caution(manaCaution));
                cautionTime = true;
                return;
            }
        }
    }

    IEnumerator SkillWCount(float dealy)
    {
        SoundManager.inst.SFXPlay("WSkill",SoundManager.inst.skList[1]);
        yield return new WaitForSeconds(3f);
        skill_W.SetActive(false);
        isGotM = false;
        yield return new WaitForSeconds(dealy-3f);

        isSkillW = true;
        coolTimeW.GetComponent<CoolTime>().End_CoolTime();
    }

    void SkillE()
    {
       
        if (Input.GetKeyDown(KeyCode.E) && isSkillE && mana >= eMana)
        {
            mousePoint.SetActive(false);
            mana -= eMana;
            isSkillE = false;
            StartCoroutine(SkillECount(time_E));
            coolTimeE.GetComponent<CoolTime>().Reset_CoolTime(time_E);
        }
        else if (Input.GetKeyDown(KeyCode.E) && !isSkillE)
        {
            if (!cautionTime)
            {
                StartCoroutine(Caution(coolCaution));
                cautionTime = true;
                return;
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (mana < eMana && !cautionTime)
            {
                StartCoroutine(Caution(manaCaution));
                cautionTime = true;
                return;
            }
        }
    }
    IEnumerator SkillECount(float dealy)
    {
        RaycastHit hit;
        GameObject QQ;
        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
        {
            var dir = hit.point - animator.transform.position;
            dir.y = 0;
            animator.transform.forward = dir;
        }
        isMove = false;
        animator.SetBool("isMove", false);
        animator.SetTrigger("SkillE");
        isSkill = true;
        yield return new WaitForSeconds(2f);
        
        QQ = Instantiate(skill_E, new Vector3(hit.point.x,this.transform.position.y+1,hit.point.z), Quaternion.identity);
        yield return new WaitForSeconds(1f);
        isSkill = false;

        yield return new WaitForSeconds(3f);
        Destroy(QQ.gameObject);
        yield return new WaitForSeconds(dealy - 6f);

        isSkillE = true;
        coolTimeE.GetComponent<CoolTime>().End_CoolTime();
    }
    void SkillR()
    {      
        if (Input.GetKeyDown(KeyCode.R) && isSkillR && mana >= rMana)
        {
            mousePoint.SetActive(false);
            mana -= rMana;
            isSkillR = false;
            StartCoroutine(SkillRCount(time_R));
            coolTimeR.GetComponent<CoolTime>().Reset_CoolTime(time_R);
        }
        else if (Input.GetKeyDown(KeyCode.R) && !isSkillR)
        {
            if (!cautionTime)
            {
                StartCoroutine(Caution(coolCaution));
                cautionTime = true;
                return;
            }
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (mana < rMana && !cautionTime)
            {
                StartCoroutine(Caution(manaCaution));
                cautionTime = true;
                return;
            }
        }
    }
    IEnumerator SkillRCount(float dealy)
    {
        RaycastHit hit;
        GameObject QQ;
        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
        {
            var dir = hit.point - animator.transform.position;
            dir.y = 0;
            animator.transform.forward = dir;
            isMove = false;
            animator.SetBool("isMove", false);
            QQ = Instantiate(skill_R, hit.point, Quaternion.identity);
        }
        else
        {
            QQ = Instantiate(skill_R, transform.position, Quaternion.identity);
        }
        SoundManager.inst.SFXPlay("R", SoundManager.inst.skList[3]);
        yield return new WaitForSeconds(5f);
        Destroy(QQ.gameObject);
        yield return new WaitForSeconds(dealy - 5f);

        isSkillR = true;
        coolTimeR.GetComponent<CoolTime>().End_CoolTime();
    }
    /*
    void Tp()
    {
        if (Input.GetKey(KeyCode.F) && isSkillTP)
        {
            RaycastHit hit;
            
            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
            {
                tempSkill1 = ObjectPoolManager.inst.GetObjectFromPool("TP", transform.position, Quaternion.Euler(-90, 0, 0));
                var dir = hit.point - animator.transform.position;
                dir.y = 0;
                animator.transform.forward = dir;
                layerMask = 1 << 10;
                Vector3 anipo = animator.transform.position;
                anipo.y += 1f; 
                if (Physics.Raycast(anipo, animator.transform.forward, out hit1, tpDis, layerMask))
                {
                    transform.position += dir.normalized * hit1.distance;
                }
                else
                {
                    transform.position += dir.normalized * tpDis;
                }
                tempSkill2 = ObjectPoolManager.inst.GetObjectFromPool("TP", transform.position, Quaternion.Euler(-90, 0, 0));
                isMove = false;
                animator.SetBool("isMove", false);
                StartCoroutine(SkillTPCount());
                coolTimeF.GetComponent<CoolTime>().Reset_CoolTime(1.5f);
            }

        }
    }
    IEnumerator SkillTPCount()
    {

        SoundManager.inst.SFXPlay("TP", SoundManager.inst.skList[4]);
        isSkillTP = false;
        yield return new WaitForSeconds(1.5f);
        isSkillTP = true;
        coolTimeF.GetComponent<CoolTime>().End_CoolTime();
        ObjectPoolManager.inst.ReturnObjectToPool("TP", tempSkill1);
        ObjectPoolManager.inst.ReturnObjectToPool("TP", tempSkill2);
    }
            */

    void Tp()
    {
        if (Input.GetKey(KeyCode.F) && isSkillTP)
        {
            RaycastHit hit;

            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
            {
                tempSkill1 = Instantiate(skill_TP, transform.position, Quaternion.Euler(-90, 0, 0));
                var dir = hit.point - animator.transform.position;
                dir.y = 0;
                animator.transform.forward = dir;
                layerMask = 1 << 10;
                Vector3 anipo = animator.transform.position;
                anipo.y += 1f;
                if (Physics.Raycast(anipo, animator.transform.forward, out hit1, tpDis, layerMask))
                {
                    transform.position += dir.normalized * hit1.distance;
                }
                else
                {
                    transform.position += dir.normalized * tpDis;
                }
                tempSkill2 = Instantiate(skill_TP, transform.position, Quaternion.Euler(-90, 0, 0));
                isMove = false;
                animator.SetBool("isMove", false);
                StartCoroutine(SkillTPCount());
                coolTimeF.GetComponent<CoolTime>().Reset_CoolTime(1.5f);
            }

        }
    }
    IEnumerator SkillTPCount()
    {

        SoundManager.inst.SFXPlay("TP", SoundManager.inst.skList[4]);
        isSkillTP = false;
        yield return new WaitForSeconds(1.5f);
        isSkillTP = true;
        coolTimeF.GetComponent<CoolTime>().End_CoolTime();
        Destroy(tempSkill1);
        Destroy(tempSkill2);
    }
    private void Move()
    {
        attack = animator.GetBool("attack");
        if (isMove && !attack)
        {
            var dir = destination - transform.position;

            dir.y = 0f;

            transform.position += dir.normalized * Time.deltaTime * 10f;

            animator.transform.forward = dir;
            firePoint.transform.forward = dir;
        }

        if (GetDistance(transform.position.x, transform.position.z, destination.x, destination.z) < 0.5f)
        {
            isMove = false;
            animator.SetBool("isMove", false);
            mousePoint.SetActive(false);
        }
    }
    float GetDistance(float x1, float y1, float x2, float y2)
    {
        // [과정1] 종점(x2, y2) - 시작점(x1, y1)
        float width = x2 - x1;
        float height = y2 - y1;

        // [과정2] 거리(크기)의 스칼라값을 구하기 위해 피타고라스 정리 사용
        float distance = width * width + height * height;
        distance = Mathf.Sqrt(distance);

        return distance;
    }

    private void SetDestination(Vector3 dest)
    {
        destination = dest;
        isMove = true;
        animator.SetBool("isMove", true);
        
    }
    public override void Die()
    {
        //
        TrapTarget.SetActive(false);
        rigidbody.useGravity = false;
        isSkill = true;
        isMove = false;
        animator.SetBool("isMove", false);
        this.transform.position = new Vector3(-1.7f, 2f, 26);
        LoadingSceneManager.LoadScene("Town");
        health = startingHealth; //수정해야함
        mana = startingMana;
    }
    public override void OnDamage(float damage)
    {
        //
        if (!isGotM)
        {
            damage -= dP;
            if(dP >damage)
            {
                damage = 10;
            }
            if (health < startingHealth / 2)
                bloodScreen.SetActive(true);
            base.OnDamage(damage);
            
        }
    }

    public void CheckPotion(Item _item)
    {
        if (_item.itemName == "파워엘릭서")
        {
            health = startingHealth;
            mana = startingMana;
        }
        if (_item.itemName == "엘릭서")
        {
            health += (startingHealth / 2);
            mana += (startingMana / 2);
        }
    }
    public void HealHp(Item _item) //체력포션 사용
    {
        
        if (startingHealth <= health)
        {
            return;
        }
        CheckPotion(_item);
        slotCountClear = true;

        SoundManager.inst.SFXPlay("potionDrink", SoundManager.inst.uiList[2]);
        health += _item.itemHp;
        Healing();
        if (health > startingHealth)
            health = startingHealth;

        if (health > startingHealth / 2)
            bloodScreen.SetActive(false);
    }

    public void HealMp(Item _item)//마나포션 사용
    {
        if (startingMana <= mana)
        {
            return;
        }
        CheckPotion(_item);
        slotCountClear = true;
        SoundManager.inst.SFXPlay("potionDrink", SoundManager.inst.uiList[2]);
        mana += _item.itemMp;
        Healing();
        if (mana > startingMana)
            mana = startingMana;
    }
    public void Healing()
    {
        healing.SetActive(true);
        StartCoroutine("Healob");

    }
    IEnumerator Healob()
    {
        yield return new WaitForSeconds(2f);
        healing.SetActive(false);
    }
    public void EquipEffect(Item _item)
    {
        dP += _item.itemDp;
        power += _item.itemPower;
        startingHealth += _item.startingHp;
        health += _item.startingHp;
        startingMana += _item.startingMp;
        mana += _item.startingMp;
    }
   
    public void TakeOffEffect(Item _item)
    {
        dP -= _item.itemDp;
        power -= _item.itemPower;
        startingHealth -= _item.startingHp;
        health -= _item.startingHp;
        startingMana -= _item.startingMp;
        mana -= _item.startingMp;
    }

    public void ExpPlus(int exp2)
    {
        if(level >=10)
        {
            exp = startingEx;
            return;
        }
        exp += exp2;
        SetLevel();
    }
}

