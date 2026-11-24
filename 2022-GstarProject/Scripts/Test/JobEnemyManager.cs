using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using UnityEngine.Jobs;
using Random = Unity.Mathematics.Random;

/// <summary>
/// 대량 Enemy 벤치마크. 데이터는 필드별 NativeArray(SoA)로 보관하고 Burst Job으로 갱신
/// </summary>
public class JobEnemyManager : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab; //MeshRenderer만 있는 가벼운 프리팹
    [SerializeField] private int _enemyCount;
    [SerializeField] private JobEnemyConfig _enemyConfig = JobEnemyConfig.Default;
    [SerializeField] private BoxCollider[] _spawnAreas;
    [SerializeField] private Transform _player;

    [Header("Flow Field 격자 (Origin은 좌하단 모서리)")]
    [SerializeField] private FlowGridInfo _grid = new FlowGridInfo { CellSize = 1f, Size = new int2(200, 200) };
    [SerializeField] private float _heightExtent = 5f;

    [Header("옵션")]
    [SerializeField] private bool _useNavMesh = true;
    [SerializeField] private bool _useBaseline;
    [SerializeField] private bool _useInstancing; //GameObject 없이 GPU 인스턴싱으로 그리기
    [SerializeField] private Material _instancedMaterial; //Test/InstancedEnemy(정적) 또는 Test/InstancedEnemyVat 셰이더 머티리얼
    [SerializeField] private VatAnimData _vatData; //VAT 셰이더를 쓸 때만 지정 (Tools/VAT Baker로 생성)
    [SerializeField] private bool _useCulling; //인스턴싱 모드에서 카메라 절두체 밖 개체를 그리지 않음
    [SerializeField] private Camera _cullingCamera; //비어 있으면 Camera.main
    [SerializeField] private float _cullPadding = 0.5f; //애니메이션으로 기본 포즈 밖으로 나가는 만큼의 여유 반지름
    [SerializeField] private bool _usePriorityQueue; //Integration Field를 이진 힙 다익스트라로 계산 (끄면 FIFO)
    [SerializeField] private float _hashCellSize = 1f;
    [SerializeField] private float _gizmoRadius = 20f;

    [Header("테스트 공격 (Space)")]
    [SerializeField] private float _hitRadius = 6f;
    [SerializeField] private int _hitDamage = 30;

    private bool _initialized;
    private JobHandle _handle;

    private NativeArray<float3> _positions;
    private NativeArray<Vector3> _desiredPositions;
    private NativeArray<float3> _targets;
    private NativeArray<quaternion> _rotations;
    private NativeArray<JobEnemyState> _states;
    private NativeArray<float> _timers;
    private NativeArray<int> _hp;
    private NativeArray<int> _areaIndex;
    private NativeArray<Random> _randoms;
    private NativeArray<NavMeshLocation> _navLocations;
    private NativeArray<int> _areaMasks;
    private TransformAccessArray _transforms;
    private NativeArray<EnemyInstanceData> _instances;
    private NativeArray<float4> _animParams;
    private NativeArray<JobEnemyState> _prevStates;
    private NativeArray<float> _animTime;
    private InstancedEnemyRenderer _instancedRenderer;
    private NativeList<EnemyInstanceData> _visibleInstances;
    private NativeArray<float4> _frustumPlanes;
    private readonly Plane[] _planeCache = new Plane[6];
    private bool _culledThisFrame;

    private NativeArray<JobSpawnArea> _areaData;
    private NativeArray<byte> _cost;
    private FlowField _chaseField;
    private FlowField _returnFields;
    private NativeMultiHashMap<int, int> _hash;
    private NativeQueue<int> _deadQueue;
    private NavMeshQuery _query;
    private int _agentTypeId;
    private int2 _lastPlayerCell = new int2(int.MinValue);

    private readonly List<OopEnemyBaseline> _baselines = new List<OopEnemyBaseline>();

    // 표시용
    private float _fieldBuildMs;
    private float _smoothDeltaTime;
    private readonly int[] _stateCounts = new int[6];

    private void Start()
    {
        JobSpawnArea[] areas = BuildAreas();

        if (_useBaseline)
        {
            SpawnBaseline(areas);
            return;
        }

        int n = _enemyCount;
        _positions = new NativeArray<float3>(n, Allocator.Persistent);
        _desiredPositions = new NativeArray<Vector3>(n, Allocator.Persistent);
        _targets = new NativeArray<float3>(n, Allocator.Persistent);
        _rotations = new NativeArray<quaternion>(n, Allocator.Persistent);
        _states = new NativeArray<JobEnemyState>(n, Allocator.Persistent);
        _timers = new NativeArray<float>(n, Allocator.Persistent);
        _hp = new NativeArray<int>(n, Allocator.Persistent);
        _areaIndex = new NativeArray<int>(n, Allocator.Persistent);
        _randoms = new NativeArray<Random>(n, Allocator.Persistent);
        _navLocations = new NativeArray<NavMeshLocation>(n, Allocator.Persistent);
        _areaMasks = new NativeArray<int>(n, Allocator.Persistent);

        if (_useInstancing && (!SystemInfo.supportsComputeShaders || !SystemInfo.supportsInstancing))
        {
            UnityEngine.Debug.LogWarning("[JobEnemyManager] 이 기기는 인스턴싱을 지원하지 않아 Transform 방식으로 그립니다.");
            _useInstancing = false;
        }
        if (_useInstancing)
        {
            _instances = new NativeArray<EnemyInstanceData>(n, Allocator.Persistent);
            _animParams = new NativeArray<float4>(n, Allocator.Persistent);
            _prevStates = new NativeArray<JobEnemyState>(n, Allocator.Persistent);
            _animTime = new NativeArray<float>(n, Allocator.Persistent);
            _instancedRenderer = new InstancedEnemyRenderer(_enemyPrefab, _instancedMaterial, n, GridBounds(), _vatData);
            _visibleInstances = new NativeList<EnemyInstanceData>(n, Allocator.Persistent);
            _frustumPlanes = new NativeArray<float4>(6, Allocator.Persistent);
        }
        else
        {
            _transforms = new TransformAccessArray(n);
        }

        _areaData = new NativeArray<JobSpawnArea>(areas, Allocator.Persistent);
        _cost = new NativeArray<byte>(_grid.CellCount, Allocator.Persistent);
        _chaseField = new FlowField(_grid, 1);
        _returnFields = new FlowField(_grid, areas.Length);
        _hash = new NativeMultiHashMap<int, int>(n, Allocator.Persistent);
        _deadQueue = new NativeQueue<int>(Allocator.Persistent);
        _query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent);
        _agentTypeId = NavMesh.GetSettingsByIndex(0).agentTypeID;
        _initialized = true;

        BuildStaticFields();

        for (int i = 0; i < n; i++)
        {
            _areaIndex[i] = i % areas.Length;
            _rotations[i] = quaternion.identity;
            _randoms[i] = new Random((uint)i * 7919u + 1u);
            _areaMasks[i] = NavMesh.AllAreas;
            Respawn(i);
            _timers[i] = UnityEngine.Random.Range(0f, 2f); //처음 패턴이 한 프레임에 몰리지 않도록 분산

            if (_useInstancing)
            {
                _animTime[i] = UnityEngine.Random.Range(0f, 2f); //처음 Idle 애니메이션이 모두 같은 동작으로 맞물리지 않도록 분산
                continue;
            }
            GameObject go = Instantiate(_enemyPrefab, _positions[i], Quaternion.identity);
            _transforms.Add(go.transform);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DealDamage(_player.position, _hitRadius, _hitDamage);
        }

        if (!_initialized)
        {
            return;
        }

        int n = _enemyCount;
        float3 playerPos = _player.position;

        //플레이어가 다른 셀로 옮겨갔을 때만 추적 필드 재계산
        JobHandle fieldHandle = default;
        int2 playerCell = _grid.CellOf(playerPos);
        if (!playerCell.Equals(_lastPlayerCell))
        {
            fieldHandle = _chaseField.Schedule(0, playerPos, _cost, default, _usePriorityQueue);
            _lastPlayerCell = playerCell;
        }

        _hash.Clear();
        JobHandle hashHandle = new BuildSpatialHashJob
        {
            Positions = _positions,
            Hash = _hash.AsParallelWriter(),
            CellSize = _hashCellSize,
        }.Schedule(n, 64);

        JobHandle aiHandle = new EnemyAIJob
        {
            Positions = _positions,
            Hash = _hash,
            Areas = _areaData,
            AreaIndex = _areaIndex,
            Cost = _cost,
            ChaseDirections = _chaseField.Directions,
            ChaseIntegration = _chaseField.Integration,
            ReturnDirections = _returnFields.Directions,
            States = _states,
            Timers = _timers,
            Targets = _targets,
            Hp = _hp,
            Randoms = _randoms,
            Rotations = _rotations,
            DesiredPositions = _desiredPositions,
            DeadQueue = _deadQueue.AsParallelWriter(),
            Grid = _grid,
            Config = _enemyConfig,
            HashCellSize = _hashCellSize,
            PlayerPos = playerPos,
            DeltaTime = Time.deltaTime,
        }.Schedule(n, 64, JobHandle.CombineDependencies(fieldHandle, hashHandle));

        JobHandle moveHandle;
        if (_useNavMesh)
        {
            moveHandle = new NavConstrainJob
            {
                Query = _query,
                Locations = _navLocations,
                Targets = _desiredPositions,
                AreaMasks = _areaMasks,
                Positions = _positions,
            }.Schedule(aiHandle);
            NavMeshWorld.GetDefaultWorld().AddDependency(moveHandle);
        }
        else
        {
            moveHandle = new CopyPositionsJob
            {
                Source = _desiredPositions,
                Positions = _positions,
            }.Schedule(n, 64, aiHandle);
        }

        if (_useInstancing)
        {
            //애니메이션은 위치 보정과 무관하므로 NavConstrain과 병렬로 실행
            JobHandle animHandle = aiHandle;
            if (_vatData != null)
            {
                animHandle = new AnimStateJob
                {
                    States = _states,
                    PrevStates = _prevStates,
                    AnimTime = _animTime,
                    AnimParams = _animParams,
                    Idle = _vatData.Clips[VatAnimData.IdleClip],
                    Walk = _vatData.Clips[VatAnimData.WalkClip],
                    Attack = _vatData.Clips[VatAnimData.AttackClip],
                    DeltaTime = Time.deltaTime,
                }.Schedule(n, 64, aiHandle);
            }

            _handle = new BuildMatricesJob
            {
                Positions = _positions,
                Rotations = _rotations,
                AnimParams = _animParams,
                Instances = _instances,
                MeshOffset = _instancedRenderer.MeshOffset,
            }.Schedule(n, 64, JobHandle.CombineDependencies(moveHandle, animHandle));

            Camera cam = _cullingCamera != null ? _cullingCamera : Camera.main;
            _culledThisFrame = _useCulling && cam != null;
            if (_culledThisFrame)
            {
                GeometryUtility.CalculateFrustumPlanes(cam, _planeCache);
                for (int p = 0; p < 6; p++)
                {
                    _frustumPlanes[p] = new float4(_planeCache[p].normal, _planeCache[p].distance);
                }
                _visibleInstances.Clear();
                _handle = new CullInstancesJob
                {
                    Instances = _instances,
                    FrustumPlanes = _frustumPlanes,
                    Visible = _visibleInstances.AsParallelWriter(),
                    SphereCenter = _instancedRenderer.CullSphereCenter,
                    SphereRadius = _instancedRenderer.CullSphereRadius + _cullPadding,
                }.Schedule(n, 64, _handle);
            }
        }
        else
        {
            _handle = new ApplyTransformJob
            {
                Positions = _positions,
                Rotations = _rotations,
            }.Schedule(_transforms, moveHandle);
        }

        JobHandle.ScheduleBatchedJobs();
    }

    private void LateUpdate()
    {
        if (!_initialized)
        {
            return;
        }

        _handle.Complete();
        while (_deadQueue.TryDequeue(out int index))
        {
            Respawn(index);
        }

        if (_useInstancing)
        {
            if (_culledThisFrame)
            {
                _instancedRenderer.Draw(_visibleInstances.AsArray(), _visibleInstances.Length);
            }
            else
            {
                _instancedRenderer.Draw(_instances, _enemyCount);
            }
        }
    }

    /// <summary>
    /// 걸을 수 있는 셀 판정 + 영역별 복귀 필드 계산 (영역은 움직이지 않으므로 1회)
    /// </summary>
    private void BuildStaticFields()
    {
        Stopwatch sw = Stopwatch.StartNew();

        JobHandle handle = default;
        if (_useNavMesh)
        {
            handle = new BuildCostFieldJob
            {
                Query = _query,
                Grid = _grid,
                HeightExtent = _heightExtent,
                AgentTypeId = _agentTypeId,
                Cost = _cost,
            }.Schedule();
            NavMeshWorld.GetDefaultWorld().AddDependency(handle);
        }
        else
        {
            for (int i = 0; i < _cost.Length; i++)
            {
                _cost[i] = FlowField.Walkable;
            }
        }

        for (int a = 0; a < _areaData.Length; a++)
        {
            handle = _returnFields.Schedule(a, _areaData[a].Center, _cost, handle, _usePriorityQueue);
        }
        handle.Complete();

        _fieldBuildMs = (float)sw.Elapsed.TotalMilliseconds;
    }

    /// <summary>
    /// 테스트용 범위 공격. 대기/배회 중이던 개체는 추적으로 전환 (기존 Enemy.TakeDamage와 동일)
    /// </summary>
    public void DealDamage(float3 center, float radius, int damage)
    {
        if (_useBaseline)
        {
            for (int i = 0; i < _baselines.Count; i++)
            {
                if (math.distance(((float3)_baselines[i].transform.position).xz, center.xz) <= radius)
                {
                    _baselines[i].TakeDamage(damage);
                }
            }
            return;
        }

        if (!_initialized)
        {
            return;
        }

        _handle.Complete();
        for (int i = 0; i < _enemyCount; i++)
        {
            JobEnemyState state = _states[i];
            if (state == JobEnemyState.Dead || math.distance(_positions[i].xz, center.xz) > radius)
            {
                continue;
            }

            _hp[i] -= damage;
            if (state == JobEnemyState.Idle || state == JobEnemyState.Wander)
            {
                _states[i] = JobEnemyState.Chase;
            }
        }
    }

    private void Respawn(int i)
    {
        JobSpawnArea area = _areaData[_areaIndex[i]];
        float3 pos = RandomWalkablePos(area);

        if (_useNavMesh)
        {
            Vector3 extents = new Vector3(_grid.CellSize, _heightExtent, _grid.CellSize);
            NavMeshLocation location = _query.MapLocation(pos, extents, _agentTypeId);
            if (!_query.IsValid(location))
            {
                location = _query.MapLocation(area.Center, area.HalfExtents + (float3)extents, _agentTypeId);
            }
            if (!_query.IsValid(location))
            {
                UnityEngine.Debug.LogWarning($"[JobEnemyManager] 영역 {_areaIndex[i]} 근처에 NavMesh가 없습니다.");
            }
            _navLocations[i] = location;
            pos = location.position;
        }

        _positions[i] = pos;
        _desiredPositions[i] = pos;
        _targets[i] = pos;
        _hp[i] = _enemyConfig.MaxHp;
        _states[i] = JobEnemyState.Idle;
        _timers[i] = 1f;
    }

    private float3 RandomWalkablePos(JobSpawnArea area)
    {
        float3 pos = area.Center;
        for (int t = 0; t < 8; t++)
        {
            pos = area.Center + new float3(
                UnityEngine.Random.Range(-area.HalfExtents.x, area.HalfExtents.x),
                0f,
                UnityEngine.Random.Range(-area.HalfExtents.z, area.HalfExtents.z));

            int2 cell = _grid.CellOf(pos);
            if (_grid.InBounds(cell) && _cost[_grid.Index(cell)] != FlowField.Blocked)
            {
                break;
            }
        }
        return pos;
    }

    /// <summary>
    /// 인스턴싱 컬링용 범위. 개체가 격자 안에 있다고 보고 격자 전체를 덮음
    /// </summary>
    private Bounds GridBounds()
    {
        Vector3 size = new Vector3(_grid.Size.x * _grid.CellSize, _heightExtent * 2f + 10f, _grid.Size.y * _grid.CellSize);
        return new Bounds((Vector3)_grid.Origin + new Vector3(size.x * 0.5f, 0f, size.z * 0.5f), size);
    }

    private JobSpawnArea[] BuildAreas()
    {
        var areas = new JobSpawnArea[_spawnAreas.Length];
        for (int a = 0; a < _spawnAreas.Length; a++)
        {
            Bounds bounds = _spawnAreas[a].bounds;
            areas[a] = new JobSpawnArea
            {
                Center = bounds.center,
                HalfExtents = bounds.extents,
                BackDistance = _enemyConfig.BackDistance,
                FieldIndex = a,
            };
        }
        return areas;
    }

    private void SpawnBaseline(JobSpawnArea[] areas)
    {
        for (int i = 0; i < _enemyCount; i++)
        {
            GameObject go = Instantiate(_enemyPrefab, areas[i % areas.Length].Center, Quaternion.identity);
            go.AddComponent<NavMeshAgent>();
            OopEnemyBaseline enemy = go.AddComponent<OopEnemyBaseline>();
            enemy.Init(areas[i % areas.Length], _enemyConfig, _player);
            _baselines.Add(enemy);
        }
    }

    private void OnDestroy()
    {
        if (!_initialized)
        {
            return;
        }

        _handle.Complete();
        _positions.Dispose();
        _desiredPositions.Dispose();
        _targets.Dispose();
        _rotations.Dispose();
        _states.Dispose();
        _timers.Dispose();
        _hp.Dispose();
        _areaIndex.Dispose();
        _randoms.Dispose();
        _navLocations.Dispose();
        _areaMasks.Dispose();
        if (_transforms.isCreated) _transforms.Dispose();
        if (_instances.IsCreated) _instances.Dispose();
        if (_animParams.IsCreated) _animParams.Dispose();
        if (_prevStates.IsCreated) _prevStates.Dispose();
        if (_animTime.IsCreated) _animTime.Dispose();
        _instancedRenderer?.Dispose();
        if (_visibleInstances.IsCreated) _visibleInstances.Dispose();
        if (_frustumPlanes.IsCreated) _frustumPlanes.Dispose();
        _areaData.Dispose();
        _cost.Dispose();
        _chaseField.Dispose();
        _returnFields.Dispose();
        _hash.Dispose();
        _deadQueue.Dispose();
        _query.Dispose();
        _initialized = false;
    }

    private void OnGUI()
    {
        _smoothDeltaTime = Mathf.Lerp(_smoothDeltaTime, Time.unscaledDeltaTime, 0.05f);

        System.Array.Clear(_stateCounts, 0, _stateCounts.Length);
        if (_useBaseline)
        {
            for (int i = 0; i < _baselines.Count; i++)
            {
                _stateCounts[(int)_baselines[i].State]++;
            }
        }
        else if (_initialized)
        {
            _handle.Complete();
            for (int i = 0; i < _enemyCount; i++)
            {
                _stateCounts[(int)_states[i]]++;
            }
        }

        GUILayout.BeginArea(new Rect(10, 10, 360, 230), GUI.skin.box);
        GUILayout.Label($"{(_useBaseline ? "OOP Baseline" : _useInstancing ? "Job + Burst + Instancing" : "Job + Burst")}  /  {_enemyCount}마리");
        GUILayout.Label($"FPS {1f / _smoothDeltaTime:F1}  ({_smoothDeltaTime * 1000f:F2} ms)");
        if (!_useBaseline)
        {
            GUILayout.Label($"정적 필드 생성 {_fieldBuildMs:F1} ms  ({(_usePriorityQueue ? "Heap" : "FIFO")})");
        }
        if (!_useBaseline && _initialized)
        {
            GUILayout.Label($"셀 확장 횟수  복귀 {_returnFields.TotalExpandCount()}  추적 {_chaseField.ExpandCounts[0]}  (격자 {_grid.CellCount})");
        }
        if (_initialized && _useInstancing)
        {
            GUILayout.Label($"그리는 개체 {(_culledThisFrame ? _visibleInstances.Length : _enemyCount)} / {_enemyCount}  (컬링 {(_culledThisFrame ? "켬" : "끔")})");
        }
        GUILayout.Label($"Idle {_stateCounts[0]}  Wander {_stateCounts[1]}  Chase {_stateCounts[2]}");
        GUILayout.Label($"Attack {_stateCounts[3]}  Return {_stateCounts[4]}  Dead {_stateCounts[5]}");
        GUILayout.Label("Space: 플레이어 주변 공격");
        GUILayout.EndArea();
    }

    /// <summary>
    /// 플레이어 주변의 막힌 셀(빨강)과 추적 방향(초록) 표시
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!_initialized || _player == null)
            return;

        _handle.Complete();
        int2 center = _grid.CellOf(_player.position);
        int range = Mathf.CeilToInt(_gizmoRadius / _grid.CellSize);
        Vector3 cubeSize = new Vector3(_grid.CellSize, 0.1f, _grid.CellSize) * 0.9f;

        for (int y = center.y - range; y <= center.y + range; y++)
        {
            for (int x = center.x - range; x <= center.x + range; x++)
            {
                int2 cell = new int2(x, y);
                if (!_grid.InBounds(cell))
                {
                    continue;
                }

                int index = _grid.Index(cell);
                Vector3 c = _grid.CellCenter(cell);
                if (_cost[index] == FlowField.Blocked)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(c, cubeSize);
                    continue;
                }

                float2 dir = _chaseField.Directions[index];
                Gizmos.color = Color.green;
                Gizmos.DrawLine(c, c + new Vector3(dir.x, 0f, dir.y) * _grid.CellSize * 0.45f);
            }
        }
    }
}
