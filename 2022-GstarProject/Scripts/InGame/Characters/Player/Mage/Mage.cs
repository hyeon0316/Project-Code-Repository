using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mage : Player
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
    [SerializeField] private Transform _normalAttackPos;

    [Header("불렛레인")]
    [SerializeField] private BulletRain _bulletRain;
    
    [Header("체인라이트닝 라인")] [SerializeField] private ChainLightningLine _chainLightningLine;

    protected override void Start()
    {
        base.Start();
        _useSkills.Add(UseWideAreaBarrage);
        _useSkills.Add(UseChainLightning);
        _useSkills.Add(UseBulletRain);
        _useSkills.Add(UseWindAttack);
        _useSkills.Add(UseSpikeAttack);
    }
    
    /// <summary>
    /// 기본공격할때 지정 위치에 발사체 생성
    /// </summary>
    public void CreateNormalAttackMissile()
    {
        if (_targets.Count != 0)
        {
            SoundManager.Instance.PlayerPlay(PlayerSoundType.NormalAttack);
            var obj = ObjectPoolManager.Instance.GetObject(PoolType.NormalAttackMissile);
            obj.transform.position = _normalAttackPos.position;
            obj.transform.rotation = _normalAttackPos.rotation;
            obj.GetComponent<NormalAttackMissile>().Init(_targets[0]);
        }
    }

    public void UseSpikeAttack()
    {
        if (!_skiilCoolDown[(int) SkillCoolType.SpikeAttack].IsCoolDown && !IsAttack)
        {
            if (_targets.Count != 0)
            {
                ActionFromDistance(SpikeAttack, _targets[0]);
            }
            else
            {
                CheckAttackRange(1, SpikeAttack);
            }
        }
    }

    private void SpikeAttack()
    {
        if (_targets.Count != 0)
        {
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Attak2);
            IsAttack = true;
            Debug.Log("스파이크 어택");
            _skiilCoolDown[(int) SkillCoolType.SpikeAttack].SetCoolDown();
            transform.LookAt(new Vector3(_targets[0].position.x, transform.position.y, _targets[0].position.z));
            _animator.SetTrigger(Global.SpikeAttackTrigger);
        }
    }

    public void CreateSpikeAttack()
    {
        if (_targets.Count != 0)
        {
            GameObject spike = ObjectPoolManager.Instance.GetObject(PoolType.VolcanicSpike);
            spike.transform.position =
                new Vector3(_targets[0].position.x, _targets[0].transform.position.y, _targets[0].position.z);
            spike.GetComponent<SpikeAttack>().DelayDisable();
        }
    }
    
    /// <summary>
    /// 불렛라인 사용
    /// </summary>
    public void UseBulletRain()
    {
        if (!_skiilCoolDown[(int)SkillCoolType.BulletRain].IsCoolDown && !IsAttack)
        {
            if (_targets.Count != 0)
            {
                ActionFromDistance(BulletRain, _targets[0]);
            }
            else
            {
                CheckAttackRange(1, BulletRain);
            }
        }
    }

    private void BulletRain()
    {
        if (_targets.Count != 0)
        {
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Attak3);
            IsAttack = true;
            Debug.Log("불렛 레인");
            _skiilCoolDown[(int) SkillCoolType.BulletRain].SetCoolDown();
            transform.LookAt(new Vector3(_targets[0].position.x, transform.position.y, _targets[0].position.z));
            _animator.SetTrigger(Global.BulletRainTrigger);
        }
    }

    private void CreateBulletRainMissile()
    {
        if(_targets.Count != 0)
            _bulletRain.CreateMissile(_targets[0]);
    }

    /// <summary>
    /// 체인 라이트닝 사용
    /// </summary>
    public void UseChainLightning()
    {
        if (!_skiilCoolDown[(int)SkillCoolType.ChainLightning].IsCoolDown && !IsAttack)
        {
            if (_targets.Count != 0)
            {
                ActionFromDistance(ChainLightning, _targets[0]);
            }
            else
            {
                CheckAttackRange(1, ChainLightning);
            }
        }
    }

    private void ChainLightning()
    {
        if (_targets.Count != 0)
        {
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Jump1);
            IsAttack = true;
            Debug.Log("체인 라이트닝");
            _skiilCoolDown[(int) SkillCoolType.ChainLightning].SetCoolDown();
            transform.LookAt(new Vector3(_targets[0].position.x, transform.position.y, _targets[0].position.z));
            _animator.SetTrigger(Global.ChainLightningTrigger);
        }
    }
    
    public void CreateChainLightningLine()
    {
        if(_targets.Count != 0)
            _chainLightningLine.CreateLine();
    }

    /// <summary>
    /// 광역기 스킬 사용
    /// </summary>
    public void UseWideAreaBarrage()
    {
        if (!_skiilCoolDown[(int)SkillCoolType.WideAreaBarrage].IsCoolDown && !IsAttack) 
        {
            if (_targets.Count != 0)
            {
                ActionFromDistance(WideAreaBarrage, _targets[0]);
            }
            else
            {
                CheckAttackRange(1, WideAreaBarrage);
            }
        }
    }

    public void UseWindAttack()
    {
        if (!_skiilCoolDown[(int)SkillCoolType.WindAttack].IsCoolDown && !IsAttack) 
        {
            if (_targets.Count != 0)
            {
                ActionFromDistance(WindAttack, _targets[0]);
            }
            else
            {
                CheckAttackRange(1, WindAttack);
            }
        }
    }

    public void CreateWindAttackEffect()
    {
        if (_targets.Count != 0)
        {
            var windAttack = ObjectPoolManager.Instance.GetObject(PoolType.WindAttackEffect);
            windAttack.transform.position =
                new Vector3(_targets[0].position.x, _targets[0].transform.position.y, _targets[0].position.z);
            windAttack.GetComponent<WindAttack>().DelayDisable();
        }
    }

    private void WindAttack()
    {
        if (_targets.Count != 0)
        {
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Jump2);
            IsAttack = true;
            Debug.Log("바람범위공격");
            _skiilCoolDown[(int) SkillCoolType.WindAttack].SetCoolDown();
            transform.LookAt(new Vector3(_targets[0].position.x, transform.position.y, _targets[0].position.z));
        }
        _animator.SetTrigger(Global.WindAttackTrigger);
    }

    private void WideAreaBarrage()
    {
        if (_targets.Count != 0)
        {
            SoundManager.Instance.PlayerPlay(PlayerSoundType.Jump3);
            IsAttack = true;
            Debug.Log("범위공격");
            _skiilCoolDown[(int) SkillCoolType.WideAreaBarrage].SetCoolDown();
            transform.LookAt(new Vector3(_targets[0].position.x, transform.position.y, _targets[0].position.z));
            _animator.SetTrigger(Global.WideAreaBarrageTrigger);
        }
    }


    public void CreateWideAreaBarrageEffect()
    {
        if (_targets.Count != 0)
        {
            var effect = ObjectPoolManager.Instance.GetObject(PoolType.WideAreaBarrage);
            effect.transform.position = new Vector3(_targets[0].position.x, _targets[0].transform.position.y,
                    _targets[0].position.z);
            effect.GetComponent<WideAreaBarrageEffect>().DelayDisable();
        }
    }


}
