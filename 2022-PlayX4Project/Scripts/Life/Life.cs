using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LifeType
{
    Player,
    Cultist,
    Assassin,
    BigCultist,
    Twisted,
    Bringer,
    Necromancer,
    Demon,
    Dummy,
    Max
}

public abstract class Life : MonoBehaviour
{
    public int LifeId { get; private set; }
    public float MaxHp { get; set; }
    public int Power { get; set; }
    public float Speed { get; set; }
    public float Hp
    {
        get { return _hp; }
        set
        {
            if (MaxHp < value)
            { _hp = MaxHp; }
            else
            { _hp = value; }
        }
    }
    public float HpRatio
    {
        get
        {
            if (_hp / MaxHp < 0)
            { return 0f; }
            else
            { return _hp / MaxHp; }
        }
    }

    [SerializeField] protected Animator _anim;
    [SerializeField] protected Rigidbody _rigid;
    [SerializeField] private LifeType _lifeType;

    protected float _hp;

    protected virtual void Awake()
    {
        Initdata(JsonToDataManager.Instance.Data.Stats[(int)_lifeType]);
    }

    /// <summary>
    ///  dmg * addDmg값 만큼의 데미지를 받음
    /// </summary>
    /// <param name="dmg">상대 캐릭터의 기본 Power</param>
    /// <param name="addDmg">특정 스킬의 Damage</param>
    public abstract bool GetDamage(float dmg, float addDmg);
    protected abstract IEnumerator DeadEventCo();

    protected bool CheckLiving()
    {
        if(_hp <=0)
        {
            StartCoroutine(DeadEventCo());
            return false;
        }
        return true;
    }

    private void Initdata(Stat stat)
    {
        LifeId = stat.Id;
        Power = stat.Power;
        _hp = stat.Hp;
        MaxHp = _hp;
        Speed = stat.Speed;
    }
}

