using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// BulletRain의 발사체 생성기
/// </summary>
public class BulletRain : MonoBehaviour
{
   [Header("미사일 기능 관련")]
   [SerializeField] private float _missileSpeed = 2; // 미사일 속도.
   
   [Space(10f)]
   [SerializeField] private float _distanceFromStart = 6.0f; // 시작 지점을 기준으로 얼마나 꺾일지.
   [SerializeField] private float _distanceFromEnd = 3.0f; // 도착 지점을 기준으로 얼마나 꺾일지.
   
   [Space(10f)]
   [SerializeField] private int _shotMaxCount = 10; // 총 몇 개 발사할건지.
   [Range(0, 1)] 
   [SerializeField] private float _interval = 0.15f;
   [SerializeField] private int _shotCountEveryInterval = 2; // 한번에 몇 개씩 발사할건지.

   private AudioSource _audioSource;

   private void Awake()
   {
      _audioSource = GetComponent<AudioSource>();
   }

   public void CreateMissile(Transform target)
   {
      StartCoroutine(CreateMissileCo(target));
      StartCoroutine(SetRandomSound(6));
   }

   private IEnumerator SetRandomSound(int maxCount)
   {
      WaitForSeconds delay = new WaitForSeconds(0.05f);
      for (int i = 0; i < maxCount; i++)
      {
         SoundManager.Instance.EffectPlay(_audioSource, EffectSoundType.BulletRain);
         yield return delay;
      }
   }
   
   private IEnumerator CreateMissileCo(Transform target)
   {
      int shotCount = _shotMaxCount;
      while (shotCount > 0)
      {
         if (target == null) //타겟이 죽을 경우 중단
            break;
         
         for (int i = 0; i < _shotCountEveryInterval; i++)
         {
            if (target == null) //타겟이 죽을 경우 중단
               break;
            
            GameObject missile = ObjectPoolManager.Instance.GetObject(PoolType.BulletRainMissile);
            BulletRainMissile bulletRainMissile = missile.GetComponent<BulletRainMissile>();
            bulletRainMissile.Init(this.transform, target, _missileSpeed, _distanceFromStart, _distanceFromEnd);
            shotCount--;
         }
      }
      yield return new WaitForSeconds(_interval);
   }
}
