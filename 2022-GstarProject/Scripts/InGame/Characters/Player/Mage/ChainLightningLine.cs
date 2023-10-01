using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class ChainLightningLine : SkillAttack
{
    private AudioSource _audioSource;
    
    [SerializeField] private float _drawingSpeed;
    
    private LineRenderer _lineRenderer;

    private bool _isDone;
    private float _timer = 0;


    [Header("스킬 유지 시간")] [SerializeField] private float _keepTime;

    [Header("연결 범위")] [SerializeField] private float _chainRange;

    private List<Transform> _chainTargets = new List<Transform>();
    
    [Range(1, 64)]
    [SerializeField] private int _rows = 8;
    
    [Range(1, 64)]
    [SerializeField] private int _columns = 1;
    private Vector2 _textureSize;
    private Vector2[] _offsets;
    private int _animationOffsetIndex;
    private int _animationPingPongDirection = 1;

    private bool _isMany; //타겟이 2명이상일때

    /// <summary>
    /// 체인 라이트닝을 이미 사용중인지
    /// </summary>
    private bool _isUsed;
    
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (_isDone)
        {
            _timer += Time.deltaTime;
            if(_timer >= _keepTime)
                CloseLine();

            KeepDrawing();
            SelectOffset();
        }
    }

    /// <summary>
    /// 계속 그려줌
    /// </summary>
    private void KeepDrawing()
    {
        if (_isMany)
        {
            for (int i = 0; i < _chainTargets.Count; i++)
            {
                _lineRenderer.SetPosition(i, _chainTargets[i].transform.position + Vector3.up);
            }
        }
        else
        {
            if (_chainTargets.Count != 0)
            {
                _lineRenderer.SetPosition(0, transform.position + Vector3.up);
                _lineRenderer.SetPosition(1, _chainTargets[0].transform.position + Vector3.up);
            }
        }
    }

    private IEnumerator TakeLightningDamage()
    {
        if (!_isUsed)
        {
            _isUsed = true;
            WaitForSeconds delay = new WaitForSeconds(1f);
            while (_timer < _keepTime)
            {
                List<Transform> tempTargets = new List<Transform>();
                foreach (var t in _chainTargets) //깊은복사, foreach로 탐색 중 target이 사라졌을때 오류발생에 대한 방지
                {
                    tempTargets.Add(t);
                }

                foreach (var target in tempTargets)
                {
                    if (target.TryGetComponent(out Creature enemy))
                    {
                        enemy.TryGetDamage(DataManager.Instance.Player.Stat, this);
                        SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.ChainLightning);
                    }
                }

                yield return delay;
            }

            _isUsed = false;
        }
    }

    public void CreateLine()
    {
        _lineRenderer.enabled = true;
        _chainTargets.Add(DataManager.Instance.Player.Targets[0]);
        CheckRange(3);
        SetFromMaterialChange();

        StartCoroutine(_chainTargets.Count > 1 ? CreateLineCo() : CreateOneLineCo());
    }

    /// <summary>
    /// 라인 연결을 한명만 할때 
    /// </summary>
    /// <returns></returns>
    private IEnumerator CreateOneLineCo()
    {
        _isMany = false;
        _lineRenderer.SetPosition(0, transform.position + Vector3.up);
        
        int duration = 1;
        float time = 0;
        while (true)
        {
            if (time >= duration)
            {
                break;
            }
            
            time += Time.deltaTime * _drawingSpeed;
            _lineRenderer.SetPosition(1, Vector3.Lerp(
                transform.position + Vector3.up,
                _chainTargets[0].transform.position + Vector3.up, Mathf.Clamp01(time)));
            
            SelectOffset();
            yield return null;
        }
        _isDone = true;
        StartCoroutine(TakeLightningDamage());
    }

    /// <summary>
    /// 여러 적에게 라인을 서서히 연결
    /// </summary>
    private IEnumerator CreateLineCo()
    {
        _isMany = true;
        _lineRenderer.SetPosition(0, _chainTargets[0].transform.position + Vector3.up);
        
        int index = 1;
        int duration = 1;
        float time = 0;
        while (true)
        {
            if (time >= duration)
            {
                index++;
                time = 0;
                _lineRenderer.positionCount++;
                if (index == _chainTargets.Count)
                {
                    _lineRenderer.positionCount--;
                    break;
                }
            }
            time += Time.deltaTime * _drawingSpeed;
            _lineRenderer.SetPosition(index, Vector3.Lerp(
                _chainTargets[index - 1].transform.position + Vector3.up,
                _chainTargets[index].transform.position + Vector3.up, Mathf.Clamp01(time)));
            
            SelectOffset();
            yield return null;
        }
        _isDone = true;
        StartCoroutine(TakeLightningDamage());
    }

    private void CheckRange(int searchCount)
    {
        Collider[] colliders = Physics.OverlapSphere(_chainTargets[0].transform.position, _chainRange, LayerMask.GetMask("Enemy"));

        var searchList = colliders.OrderBy(col => Vector3.Distance(_chainTargets[0].transform.position, col.transform.position))
            .ToList();

        if (searchList.Count != 1) //자기자신만 검출될 경우를 제외하고
        {
            for (int i = 1; i < searchCount; i++)
            {
                if (i == searchList.Count) //찾고자 하는 타겟 수가 실제 존재하는 타겟 수 보다 적을 경우
                    break;

                _chainTargets.Add(searchList[i].transform);
            }
        }
    }
    
    private void CloseLine()
    {
        _isDone = false;
        _chainTargets.Clear();
        _lineRenderer.positionCount = 2;
        _lineRenderer.enabled = false;
        _timer = 0;
    }
    
    /// <summary>
    /// 전기 텍스쳐 초기 설정
    /// </summary>
    private void SetFromMaterialChange()
    {
        _textureSize = new Vector2(1.0f / (float)_columns, 1.0f / (float)_rows);
        _lineRenderer.material.mainTextureScale = _textureSize;
        _offsets = new Vector2[_rows * _columns];
        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                _offsets[x + (y * _columns)] = new Vector2((float)x / _columns, (float)y / _rows);
            }
        }
    }

    /// <summary>
    /// 텍스쳐를 수시로 바꿔서 전기가 흐르는 것을 표현
    /// </summary>
    private void SelectOffset()
    {
        int index;

        index = _animationOffsetIndex;
        _animationOffsetIndex += _animationPingPongDirection;
        if (_animationOffsetIndex >= _offsets.Length)
        {
            _animationOffsetIndex = _offsets.Length - 2;
            _animationPingPongDirection = -1;
        }
        else if (_animationOffsetIndex < 0)
        {
            _animationOffsetIndex = 1;
            _animationPingPongDirection = 1;
        }

        if (index >= 0 && index < _offsets.Length)
        {
            _lineRenderer.material.mainTextureOffset = _offsets[index];
        }
        else
        {
            _lineRenderer.material.mainTextureOffset = _offsets[0];
        }
    }
}
