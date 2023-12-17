using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;

public abstract class Player : Creature
{
    /// <summary>
    /// 퀘스트를 자동 진행중인지 확인
    /// </summary>
    public bool IsQuest { get; set; }

    [SerializeField] private PlayerStatObj _statObj;
    [Header("자신의 체력 바")]
    [SerializeField] protected FixedHpBar _hpBar;
    [Header("기본 공격 콤보가 취소되는 시간")]
    [SerializeField] protected float _setNormalAttackCancelTime;
    [Header("스킬 쿨타임 모음")]
    [SerializeField] protected CoolDown[] _skillCoolDown;
    [Header("적 탐색 범위")]
    [SerializeField] protected float _searchRadius;
    [Header("Npc 대화 가능 범위")]
    [SerializeField] protected float _npcTalkRadius;
    [Header("오토모드일때 추가 적 탐색 범위")]
    [SerializeField] protected float _autoModeSearch;
    [Header("캐릭터가 이동할때마다 나타나는 이펙트")]
    [SerializeField] private FootPrinter[] _foots;
    [SerializeField] protected UsePortion _portion;
    [SerializeField] private VariableJoystick _joystick;
    [SerializeField] private Fade _fade;
    [SerializeField] private TargetPanel _targetPanel;
    [SerializeField] private Transform _cameraArm;
    [SerializeField] private GameObject _autoCancelButton;
    [SerializeField] private GameObject _deadCanvas;
    [SerializeField] private GameObject _respawnButton;
    [SerializeField] private GameObject _autoActiveEffect;

    protected bool _isAttacking;
    protected bool _isAutoHunt;

    protected bool _canNextNormalAttack;
    protected int _comboCount;
    protected float _normalAttackCancelTime;

    private IEnumerator _moveCo;
    private Transform _teleportPos;

    protected delegate void UseActionType();
    protected UseActionType[] _useSkills;
    protected Queue<UseActionType> _autoSkill = new Queue<UseActionType>();
    protected List<Transform> _targets = new List<Transform>(); //탐색된 적의 정보


    protected virtual void Awake()
    {
        Stat = new PlayerStat(_statObj);
        DataManager.Instance.Player = this;
        Init();
    }

    protected override void Start()
    {
        base.Start();
        Stat.CurHp.Subscribe(curHp => _hpBar.SetHpBar(Stat.MaxHp, curHp));
        Stat.CurHp.Where(_ => _isAutoHunt).Subscribe(curHp => 
        { 
            if(curHp <= Stat.MaxHp * Global.PotionUseCondition)
            {
                _portion.UsePotion();
            }
        });
    }

    private void Update()
    {
        CheckInitCombo();
        TouchGetTarget();
    }

    private void FixedUpdate()
    {
        Move();
    }
    
    private void UseSkill(UseActionType useActionType)
    {
        useActionType();
    }
    
    /// <summary>
    /// 오토모드때 우선적으로 사용할 스킬들을 셋팅
    /// </summary>
    private void SetPrioritySkill()
    {
        for (int i = 0; i < _useSkills.Length; i++)
        {
            if (_skillCoolDown[i].IsCoolDown) 
            {
                continue;
            }

            if (!_autoSkill.Contains(_useSkills[i]))
            {
                _autoSkill.Enqueue(_useSkills[i]);
            }
        }
    }

    private async UniTask ExecuteAutoHunt()
    {
        while (true)
        {
            if (!_isAutoHunt)
            {
                break;
            }

            if (!_isAttacking)
            {
                SetPrioritySkill();
                if (_autoSkill.Count == 0) 
                {
                    UseNormalAttack();
                }
                else
                {
                    UseSkill(_autoSkill.Dequeue());
                }
                _isAttacking = true;
            }
            await UniTask.NextFrame();
        }
    }
    
    private void Move()
    {
        if (!IsDead && !_isAttacking)
        {
            float x = _joystick.Horizontal;
            float z = _joystick.Vertical;

            Vector3 moveVec = new Vector3(x, 0, z);
            this.transform.Translate(Vector3.forward * _joystick.Direction.magnitude * Stat.MoveSpeed * Time.fixedDeltaTime);

            if (moveVec.sqrMagnitude == 0)
            {
                return;
            }

            Vector3 camAngle = _cameraArm.transform.rotation.eulerAngles;
            Vector3 camDirAngle = Quaternion.LookRotation(moveVec).eulerAngles;
            Vector3 resultAngle = Vector3.up * (camAngle.y + camDirAngle.y);
            this.transform.rotation = Quaternion.Euler(resultAngle);
            
            SetMoveAnim(_joystick.Direction.magnitude);
        }
    }

    
    public void SetMoveAnim(float blend)
    {
        _anim.SetFloat(Global.MoveBlend, blend);
    }

    /// <summary>
    /// 마을 귀환 버튼
    /// </summary>
    public void UseTownTeleport()
    {
        UseTeleport(MapManager.Instance.GetSpwan(1));
    }

    /// <summary>
    /// 순간이동
    /// </summary>
    public void UseTeleport(Transform movePos)
    {
        _teleportPos = movePos;
        _fade.FadeInOut(Teleport);
    }

    private void Teleport()
    {
        CancelCurAction();
        this.transform.position = _teleportPos.position;
    }
    
    private void Teleport(Transform movePos)
    {
        CancelCurAction();
        this.transform.position = movePos.position;
    }
    
    /// <summary>
    /// 자동사냥 모드 버튼
    /// </summary>
    public void SetAutoHunt()
    {
        if (!_isAutoHunt)
        {
            CancelAutoQuest();
            _isAttacking = false;
            _searchRadius *= _autoModeSearch;
            _isAutoHunt = true;
            _autoCancelButton.SetActive(true);
            _autoActiveEffect.SetActive(true);
            ExecuteAutoHunt().Forget();
        }
        else
        {
            CancelAutoHunt();
        }
    }

    /// <summary>
    /// 오토모드 취소 버튼
    /// </summary>
    public void CancelAutoHunt()
    {
        if (_isAutoHunt)
        {
            _autoActiveEffect.SetActive(false);
            SetMoveAnim(0);
            _autoCancelButton.SetActive(false);
            _searchRadius /= _autoModeSearch;
            _isAutoHunt = false;
            _autoSkill.Clear();
        }
        StopMoveCo();
    }

    public void CancelCurAction()
    {
        CancelAutoQuest();
        
        if (_isAutoHunt)
        {
            CancelAutoHunt();
        }
        else
        {
            StopMoveCo();
        }
    }

    /// <summary>
    /// 직접 선택하여 타겟지정
    /// </summary>
    private void TouchGetTarget()
    {
        if (!_isAutoHunt)
        {
            if (Input.GetMouseButtonDown(0)) 
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Enemy")))
                {
                    if (Math.Pow(_searchRadius, 2) > (transform.position - hit.transform.position).sqrMagnitude)
                    {
                        _targets.Clear();
                        _targets.Add(hit.transform);
                        _targetPanel.SetTargetBox(_targets[0]);
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(this.transform.position, _searchRadius);
    }
    
    /// <summary>
    /// 공격할 우선순위 타겟들을(searchCount 수 만큼) 지정
    /// </summary>
    protected void AddTarget(int searchCount, UseActionType useActionType)
    {
        SetMoveAnim(0);
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, _searchRadius, LayerMask.GetMask("Enemy"));

        _targets.Clear();
        if (colliders.Length != 0)
        {
            var searchList = colliders.OrderBy(col => (this.transform.position - col.transform.position).sqrMagnitude)
                .ToList(); //가까운 순으로 정렬

            for (int i = 0; i < searchCount; i++)
            {
                if (i == searchList.Count) //찾고자 하는 타겟 수가 실제 존재하는 타겟 수 보다 적을 경우
                    break;

                _targets.Add(searchList[i].transform);
            }
            ActionFromDistance(useActionType , _targets[0]);
        }
        else
        {
            _isAttacking = false;
            CancelAutoHunt();
        }
        
    }

    /// <summary>
    /// 거리 상관없이 퀘스트 자동진행
    /// </summary>
    public void SetAutoQuest(Transform target)
    {
        if (!IsDead)
        {
            if (!IsQuest)
            {
                CancelAutoHunt(); //중간에 다른 행동을 하고 있었을때 캔슬
                QuestFromDistance(target.gameObject.layer == LayerMask.NameToLayer("NPC") ? TalkNpc : SetAutoHunt, target);
                IsQuest = true;
            }
            else //퀘스트 자동진행 중에 한번더 클릭 될 경우 진행 취소
            {
                StopMoveCo();
                IsQuest = false;
            }
            QuestManager.Instance.SetAniQuest(IsQuest);
        }
    }

    private void TalkNpc()
    {
        QuestManager.Instance.CheckNpcQuest(MapManager.Instance.TargetNpc.ID);
    }

    /// <summary>
    /// 기본 공격 사용
    /// </summary>
    public void UseNormalAttack()
    {
        if (!_isAttacking && !IsDead)
        {
            if (_targets.Count != 0)
            {
                ActionFromDistance(() => NormalAttack().Forget(), _targets[0]);
            }
            else
            {
                AddTarget(1, () => NormalAttack().Forget());
            }
        }
    }

    /// <summary>
    /// 거리 상관없이 퀘스트 자동진행
    /// </summary>
    private void QuestFromDistance(UseActionType useActionType, Transform target)
    {
        MoveTowardTarget(useActionType, target);
    }

    /// <summary>
    /// 타겟(NPC or Enemy)과의 거리에 따른 행동패턴
    /// </summary>
    protected void ActionFromDistance(UseActionType useActionType, Transform target)
    {
        if (Math.Pow(_attackRadius, 2) < (transform.position -target.position).sqrMagnitude) //타겟이 공격사거리 밖에있을때
        {
            MoveTowardTarget(useActionType, target);
        }
        else //공격 사거리 안에 있을때
        {
            useActionType();
        }
    }

    private void MoveTowardTarget(UseActionType useActionType, Transform target)
    {
        if (_moveCo == null) //버튼이 여러번 눌렸을때 코루틴 중복 방지
        {
            _moveCo = MoveTowardTargetCo(useActionType, target);
            StartCoroutine(_moveCo);
        } 
    }

    private void StopMoveCo()
    {
        _isAttacking = false;
        _targets.Clear();
        if (_moveCo != null)
        {
            SetMoveAnim(0);
            _nav.ResetPath();
            _nav.enabled = false;
            StopCoroutine(_moveCo);
            _moveCo = null;
        }
    }

    /// <summary>
    /// target에 적정 사거리까지 이동하여 Action 수행
    /// </summary>
    protected IEnumerator MoveTowardTargetCo(UseActionType useActionType, Transform target)
    {
        float goalRadius;

        if (target.gameObject.layer == LayerMask.NameToLayer("NPC")) 
        {
            goalRadius = _npcTalkRadius;
        }
        else
        {
            goalRadius = _attackRadius;
        }
        
        _nav.enabled = true;
        SetMoveAnim(1);
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        WaitForFixedUpdate delay = new WaitForFixedUpdate();
        while (true)
        {
            if (!_nav.enabled)
                break;
            
            if (Math.Pow(goalRadius, 2) >= (transform.position - target.position).sqrMagnitude) //공격 사거리 안에 들어왔을때
            {
                useActionType();
                SetMoveAnim(0);
                _nav.enabled = false;
                _moveCo = null;
                break;
            }

            _nav.SetDestination(target.transform.position);
            yield return delay;
        }
    }

    protected abstract UniTask NormalAttack();

    private void CheckInitCombo()
    {
        if (_canNextNormalAttack)
        {
            _normalAttackCancelTime -= Time.deltaTime;
            if (_normalAttackCancelTime <= 0)
            {
                _comboCount = 1;
                _canNextNormalAttack = false;
            }
        }
    }

    private void CancelAutoQuest()
    {
        if (IsQuest)
        {
            StopMoveCo();
            IsQuest = false;
            QuestManager.Instance.SetAniQuest(IsQuest);
        }
    }

    public void ActiveFootPrinters(bool active)
    {
        for (int i = 0; i < _foots.Length; i++)
        {
             _foots[i].ActiveFoot(active);
        }
    }

    protected override void Die()
    {
        base.Die();
        SoundManager.Instance.PlayerPlay(PlayerSoundType.Die);
        _anim.Play(Global.Dead);
        _fade.FadeIn();
        _deadCanvas.SetActive(true);
        UIManager.Instance.CloseAll();
        Invoke(nameof(SetRespawnButton), 1.5f);
    }

    private void SetRespawnButton()
    {
        _respawnButton.SetActive(true);
    }

    /// <summary>
    /// 사망 이후 리스폰 처리
    /// </summary>
    public void Respawn()
    {
        Init();
        MapManager.Instance.DunArea.Clear();//던전에서 죽었을 경우
        _respawnButton.SetActive(false);
        _deadCanvas.SetActive(false);
        Teleport(MapManager.Instance.GetSpwan(1));
        _fade.FadeOut();
    }

    protected override void Init()
    {
        base.Init();
        _nav.enabled = false;
        _targets.Clear();
        this.gameObject.layer = LayerMask.NameToLayer("Player");
        _comboCount = 1;
    }

    public void DeleteTarget(Transform target)
    {
        if (_targets.Contains(target))
        {
            _targets.Remove(target);
        }
    }

    /// <summary>
    /// 피해 면역인 무적 상태로 변경
    /// </summary>
    public void SetInvincibility(bool isActive)
    {
        if (isActive)
        {
            this.gameObject.layer = LayerMask.NameToLayer("Invincibility");
        }
        else
        {
            this.gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }

    public float GetSearchRadius()
    {
        return _searchRadius;
    }

    public void UpdateHpBar()
    {
        _hpBar.SetHpBar(Stat.MaxHp, Stat.CurHp.Value);
    }
  
}