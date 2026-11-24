using UnityEngine;
using UnityEngine.AI;


public class OopEnemyBaseline : MonoBehaviour
{
    public JobEnemyState State { get; private set; }

    private NavMeshAgent _nav;
    private JobSpawnArea _area;
    private JobEnemyConfig _config;
    private Transform _player;
    private float _timer;
    private int _hp;

    public void Init(JobSpawnArea area, JobEnemyConfig config, Transform player)
    {
        _nav = GetComponent<NavMeshAgent>();
        _area = area;
        _config = config;
        _player = player;
        _nav.speed = config.MoveSpeed;
        _nav.radius = config.SeparationRadius * 0.5f;
        Respawn();
        _timer = Random.Range(0f, 2f);
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        switch (State)
        {
            case JobEnemyState.Idle:
                if (_timer <= 0f)
                {
                    if (Random.value < 0.5f)
                    {
                        _timer = Random.Range(2, 6);
                    }
                    else
                    {
                        _nav.SetDestination(RandomAreaPos());
                        _timer = _config.StuckTimeOut;
                        State = JobEnemyState.Wander;
                    }
                }
                break;

            case JobEnemyState.Wander:
                if ((!_nav.pathPending && _nav.remainingDistance <= 0.5f) || _timer <= 0f)
                {
                    _nav.ResetPath();
                    State = JobEnemyState.Idle;
                    _timer = 1f;
                }
                break;

            case JobEnemyState.Chase:
                {
                    Vector3 pos = transform.position;
                    Vector3 center = _area.Center;
                    if (Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(center.x, center.z)) > _area.BackDistance)
                    {
                        _nav.SetDestination(RandomAreaPos());
                        State = JobEnemyState.Return;
                    }
                    else if (Vector3.Distance(pos, _player.position) <= _config.AttackRadius)
                    {
                        _nav.ResetPath();
                        _timer = _config.AttackCooldown;
                        State = JobEnemyState.Attack;
                    }
                    else
                    {
                        _nav.SetDestination(_player.position);
                    }
                    break;
                }

            case JobEnemyState.Attack:
                transform.LookAt(_player.position);
                if (_timer <= 0f)
                {
                    State = JobEnemyState.Chase;
                }
                break;

            case JobEnemyState.Return:
                if (_area.Contains(transform.position))
                {
                    _nav.ResetPath();
                    _hp = _config.MaxHp;
                    State = JobEnemyState.Idle;
                    _timer = 1f;
                }
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        _hp -= damage;
        if (_hp <= 0)
        {
            Respawn();
            return;
        }

        if (State == JobEnemyState.Idle || State == JobEnemyState.Wander)
        {
            State = JobEnemyState.Chase;
        }
    }

    private void Respawn()
    {
        _nav.Warp(RandomAreaPos());
        _nav.ResetPath();
        _hp = _config.MaxHp;
        State = JobEnemyState.Idle;
        _timer = 1f;
    }

    private Vector3 RandomAreaPos()
    {
        Vector3 pos = (Vector3)_area.Center + new Vector3(
            Random.Range(-_area.HalfExtents.x, _area.HalfExtents.x),
            0f,
            Random.Range(-_area.HalfExtents.z, _area.HalfExtents.z));

        return NavMesh.SamplePosition(pos, out NavMeshHit hit, 5f, NavMesh.AllAreas) ? hit.position : pos;
    }
}
