using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Mage : Player
{
    private enum SkillCoolType
    {
        WideAreaBarrage,
        ChainLightning,
        BulletRain,
        WindAttack,
        SpikeAttack
    }
    
    [Header("기본공격 발사체 위치")]
    [SerializeField] private LongAttackCreator _normalAttackPos;

    [Header("불렛레인")]
    [SerializeField] private BulletRain _bulletRain;
    
    [Header("체인라이트닝 라인")] [SerializeField] private ChainLightningLine _chainLightningLine;


    protected override void Awake()
    {
        base.Awake();
        _useSkills = new UseActionType[] 
        { UseWideAreaBarrage, UseChainLightning, UseBulletRain, UseWindAttack, UseSpikeAttack };
    }


    private void CreateNormalAttackMissile()
    {
        if (_targets.Count != 0)
        {
            SoundManager.Instance.PlayerPlay(PlayerSoundType.NormalAttack);
            var obj = _normalAttackPos.CreateLongAttack<NormalAttackMissile>(PoolType.NormalAttackMissile);
            obj.SetStat(Stat);
            obj.Init(_targets[0]);
        }
    }

    public void UseSpikeAttack()
    {
        if (!_skillCoolDown[(int) SkillCoolType.SpikeAttack].IsCoolDown && !_isAttacking)
        {
            if (_targets.Count != 0)
            {
                ActionFromDistance(() => SpikeAttack().Forget(), _targets[0]);
            }
            else
            {
                AddTarget(1, () => SpikeAttack().Forget());
            }
        }
    }

    protected override async UniTask NormalAttack()
    {
        if (_targets.Count != 0)
        {
            _isAttacking = true;
            this.transform.LookAt(new Vector3(_targets[0].position.x, this.transform.position.y, _targets[0].position.z));
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Attak1);
            SelectNormalAttackAnim();
            await UniTask.NextFrame();
            var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);
            await UniTask.Delay(animDelay * 0.6f);
            CreateNormalAttackMissile();
            _anim.CrossFade(Global.Idle, 0.3f);
            await UniTask.Delay(1000);
            _canNextNormalAttack = true;
            _normalAttackCancelTime = _setNormalAttackCancelTime;
            _isAttacking = false;
        }
    }

    private void SelectNormalAttackAnim()
    {
        switch(_comboCount)
        {
            case 1:
                _anim.Play(Global.Attack1);
                break;
            case 2:
                _anim.Play(Global.Attack2);
                break;
            case 3:
                _anim.Play(Global.Attack3);
                break;
            case 4:
                _anim.Play(Global.Attack4);
                break;
        }
        _comboCount = (_comboCount % 4) + 1;
    }

    private async UniTask SpikeAttack()
    {
        if (_targets.Count != 0)
        {
            _isAttacking = true;
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Attak2);
            this.transform.LookAt(new Vector3(_targets[0].position.x, this.transform.position.y, _targets[0].position.z));
            _skillCoolDown[(int) SkillCoolType.SpikeAttack].SetCoolDown();
            _anim.Play(Global.SpikeAttack);
            var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);
            await UniTask.Delay(animDelay * 0.5f);
            _anim.CrossFade(Global.Idle, 0.3f);
            CreateSpikeAttack();
            await UniTask.Delay(1000);
            _isAttacking = false;
        }
    }

    private void CreateSpikeAttack()
    {
        if (_targets.Count != 0)
        {
            SpikeAttack spike = ObjectPoolManager.Instance.GetObject(PoolType.VolcanicSpike, false).GetComponent<SpikeAttack>();
            spike.SetTransform(_targets[0]);
            spike.gameObject.SetActive(true);
            spike.CallEvent();
        }
    }
    
    /// <summary>
    /// 불렛라인 사용
    /// </summary>
    public void UseBulletRain()
    {
        if (!_skillCoolDown[(int)SkillCoolType.BulletRain].IsCoolDown && !_isAttacking)
        {
            if (_targets.Count != 0)
            {
                ActionFromDistance(() =>BulletRain().Forget(), _targets[0]);
            }
            else
            {
                AddTarget(1, () => BulletRain().Forget());
            }
        }
    }

    private async UniTask BulletRain()
    {
        if (_targets.Count != 0)
        {
            _isAttacking = true;
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Attak3);
            _skillCoolDown[(int) SkillCoolType.BulletRain].SetCoolDown();
            this.transform.LookAt(new Vector3(_targets[0].position.x, this.transform.position.y, _targets[0].position.z));
            _anim.Play(Global.BulletRain);
            CreateBulletRainMissile();
            await UniTask.NextFrame();
            var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);
            await UniTask.Delay(animDelay);
            _anim.CrossFade(Global.Idle, 0.3f);
            await UniTask.Delay(1000);
            _isAttacking = false;
        }
    }

    private void CreateBulletRainMissile()
    {
        if (_targets.Count != 0)
        {
            _bulletRain.CreateMissile(_targets[0]);
        }
    }

    /// <summary>
    /// 체인 라이트닝 사용
    /// </summary>
    public void UseChainLightning()
    {
        if (!_skillCoolDown[(int)SkillCoolType.ChainLightning].IsCoolDown && !_isAttacking)
        {
            if (_targets.Count != 0)
            {
                ActionFromDistance(() => ChainLightning().Forget(), _targets[0]);
            }
            else
            {
                AddTarget(1, () => ChainLightning().Forget());
            }
        }
    }

    private async UniTask ChainLightning()
    {
        if (_targets.Count != 0)
        {
            _isAttacking = true;
            this.transform.LookAt(new Vector3(_targets[0].position.x, this.transform.position.y, _targets[0].position.z));
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Jump1);
            _skillCoolDown[(int) SkillCoolType.ChainLightning].SetCoolDown();
            _anim.Play(Global.ChainLightning);
            CreateChainLightningLine();
            await UniTask.NextFrame();
            var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);
            await UniTask.Delay(animDelay);
            _anim.CrossFade(Global.Idle, 0.3f);
            _isAttacking = false;
            await UniTask.Delay(1000);
        }
    }
    
    public void CreateChainLightningLine()
    {
        if (_targets.Count != 0)
        {
            _chainLightningLine.CreateLine(_targets[0]);
        }
    }

    /// <summary>
    /// 광역기 스킬 사용
    /// </summary>
    public void UseWideAreaBarrage()
    {
        if (!_skillCoolDown[(int)SkillCoolType.WideAreaBarrage].IsCoolDown && !_isAttacking) 
        {
            if (_targets.Count != 0)
            {
                ActionFromDistance(()=> WideAreaBarrage().Forget(), _targets[0]);
            }
            else
            {
                AddTarget(1, () => WideAreaBarrage().Forget());
            }
        }
    }

    public void UseWindAttack()
    {
        if (!_skillCoolDown[(int)SkillCoolType.WindAttack].IsCoolDown && !_isAttacking) 
        {
            if (_targets.Count != 0)
            {
                ActionFromDistance(() => WindAttack().Forget(), _targets[0]);
            }
            else
            {
                AddTarget(1, () => WindAttack().Forget());
            }
        }
    }

    private void CreateWindAttack()
    {
        if (_targets.Count != 0)
        {
            WindAttack windAttack = ObjectPoolManager.Instance.GetObject(PoolType.WindAttack, false).GetComponent<WindAttack>();
            windAttack.SetTransform(_targets[0]);
            windAttack.gameObject.SetActive(true);
            windAttack.CallEvent();
        }
    }

    private async UniTask WindAttack()
    {
        if (_targets.Count != 0)
        {
            _isAttacking = true;
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Jump2);
            _skillCoolDown[(int) SkillCoolType.WindAttack].SetCoolDown();
            this.transform.LookAt(new Vector3(_targets[0].position.x, this.transform.position.y, _targets[0].position.z));
            _anim.Play(Global.WindAttack);
            await UniTask.NextFrame();
            var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);
            await UniTask.Delay(animDelay * 0.5f);
            _anim.CrossFade(Global.Idle, 0.3f);
            CreateWindAttack();
            await UniTask.Delay(1000);
            _isAttacking = false;
        }
    }

    private async UniTask WideAreaBarrage()
    {
        if (_targets.Count != 0)
        {
            _isAttacking = true;
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Jump3);
            _skillCoolDown[(int) SkillCoolType.WideAreaBarrage].SetCoolDown();
            this.transform.LookAt(new Vector3(_targets[0].position.x, this.transform.position.y, _targets[0].position.z));
            _anim.Play(Global.WideAreaBarrage);
            await UniTask.NextFrame();
            var animDelay = TimeSpan.FromSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);
            await UniTask.Delay(animDelay * 0.5f);
            _anim.CrossFade(Global.Idle, 0.3f);
            CreateWideAreaBarrage();
            await UniTask.Delay(1000);
            _isAttacking = false;
        }
    }


    private void CreateWideAreaBarrage()
    {
        if (_targets.Count != 0)
        {
            var wideArea = ObjectPoolManager.Instance.GetObject(PoolType.WideAreaBarrage).GetComponent<WideAreaBarrageEffect>();
            wideArea.SetTransform(_targets[0]);
            wideArea.CallEvent();
        }
    }
}
