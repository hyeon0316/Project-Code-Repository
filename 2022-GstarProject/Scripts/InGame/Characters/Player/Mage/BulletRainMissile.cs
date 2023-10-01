using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BulletRainMissile : SkillAttack
{
    private Vector3[] _bezierPoints = new Vector3[4];

    private Transform _target;
    
    private float _speed;
    private float _endTime = 0;
    private float _curTime = 0;

    /// <summary>
    /// 미사일의 초기값 설정
    /// </summary>
    public void Init(Transform startTr,Transform endTr, float speed, float distanceFromStart, float distanceFromEnd)
    {
        _curTime = 0;
        _speed = speed;
        _target = endTr;
        _endTime = Random.Range(0.6f, 0.8f); //도착 시간을 랜덤으로 설정

        _bezierPoints[0] = startTr.position; //시작 지점

        //시작 지점을 기준으로 랜덤 포인트 지정
        _bezierPoints[1] = startTr.position +
                           (distanceFromStart * Random.Range(-1.0f, 1.0f) * startTr.right) +  // X (좌, 우 전체)
                           (distanceFromStart * Random.Range(-0.15f, 1.0f) * startTr.up) + // Y (아래쪽 조금, 위쪽 전체)
                           (distanceFromStart * Random.Range(-1.0f, -0.8f) * startTr.forward); // Z (뒤 쪽만)
        
        // 도착 지점을 기준으로 랜덤 포인트 지정.
        _bezierPoints[2] = _target.position +
                           (distanceFromEnd * Random.Range(-1.0f, 1.0f) * _target.right) + // X (좌, 우 전체)
                           (distanceFromEnd * Random.Range(-1.0f, 1.0f) * _target.up) + // Y (위, 아래 전체)
                           (distanceFromEnd * Random.Range(0.8f, 1.0f) * _target.forward); // Z (앞 쪽만)
        
        //_bezierPoints[3] = _target.position + new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)); // 도착 지점

        transform.position = _bezierPoints[0];
    }

    private void Update()
    {
        if (_curTime > _endTime)
        {
            StartCoroutine(MoveToTarget());
            
            return;
        }

        _curTime += Time.deltaTime * _speed;

        transform.position = new Vector3(
            CubicBezierCurve(_bezierPoints[0].x, _bezierPoints[1].x, _bezierPoints[2].x),
            CubicBezierCurve(_bezierPoints[0].y, _bezierPoints[1].y, _bezierPoints[2].y),
            CubicBezierCurve(_bezierPoints[0].z, _bezierPoints[1].z, _bezierPoints[2].z));
    }

    private IEnumerator MoveToTarget()
    {
        while (true)
        {
            if ((transform.position - _target.position).sqrMagnitude < 1)
            {
                _target.transform.GetComponent<Creature>().TryGetDamage(DataManager.Instance.Player.Stat, this);
                CreateMissileEffect();
                DisableObject();
                break;
            }

            transform.position = Vector3.Lerp(transform.position, _target.transform.position, Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// 3차 베지어 곡선.
    /// </summary>
    /// <param name="a">시작 위치</param>
    /// <param name="b">시작 위치에서 얼마나 꺾일 지 정하는 위치</param>
    /// <param name="c">도착 위치까지 얼마나 꺾일 지 정하는 위치</param>
    /// <param name="d">도착 위치</param>
    /// <returns></returns>
    private float CubicBezierCurve(float a, float b, float c)
    {
        float time = _curTime / _endTime;

        //방정식 표현
        float ab = Mathf.Lerp(a, b, time);
        float bc = Mathf.Lerp(b, c, time);


        return Mathf.Lerp(ab, bc, time);
    }

    private void CreateMissileEffect()
    {
        GameObject effect = ObjectPoolManager.Instance.GetObject(PoolType.BulletRainEffect);
        effect.transform.position = transform.position;
        effect.GetComponent<BulletRainEffect>().DelayDisable();
    }
   
}
