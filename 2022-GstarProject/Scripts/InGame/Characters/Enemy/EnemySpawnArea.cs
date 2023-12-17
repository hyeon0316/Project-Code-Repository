using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct SpawnEnemy
{
    public PoolType Type;
    public int Count;
}

public class EnemySpawnArea : MonoBehaviour
{
    [SerializeField]private string[] _enemyNames; 
    [SerializeField] private string _mapName;
    [Header("스폰 될 적 종류와 수")]
    [SerializeField] private SpawnEnemy[] _spawnEnemys;
    [SerializeField] private EnemyStatObj[] enemyStat;
    [Header("해당 스폰지점이 던전인지 아닌지")]
    [SerializeField] private bool _isDungeon;
    [SerializeField] private BoxCollider _boxCollider;

    private List<Enemy> _enemyList = new List<Enemy>();
    private int _difficulty; 

    private void OnEnable()
    {
        SpawnEnemy();
    }

    private void OnDisable()
    {
        Clear();
    }

    private void SpawnEnemy()
    {
        for (int i = 0; i < _spawnEnemys.Length; i++)
        {
            for (int j = 0; j < _spawnEnemys[i].Count; j++)
            {
                Enemy enemy = ObjectPoolManager.Instance.GetObject(_spawnEnemys[i].Type, false).GetComponent<Enemy>();
                AddEnemy(enemy);
                if (_isDungeon)
                {
                    enemy.ChangeStat(enemyStat[_difficulty]);
                    enemy.GetComponent<IGold>().RewardGold *= _difficulty + 1;
                }
                enemy.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 자신을 반환하고 딕셔너리에 있는 적 종류 중 랜덤한 적을 스폰
    /// </summary>
    public void ReturnEnemy(Enemy returnEnemy)
    {
        _enemyList.Remove(returnEnemy);

        int index = UnityEngine.Random.Range(0, _spawnEnemys.Length);
        Enemy enemy = ObjectPoolManager.Instance.GetObject(_spawnEnemys[index].Type).GetComponent<Enemy>();
        AddEnemy(enemy);
        if (_isDungeon)
        {
            enemy.ChangeStat(enemyStat[_difficulty]);
        }
        enemy.Show();
    }

    public void Clear()
    {
        for (int i = 0; i < _enemyList.Count; i++)
        {
            ObjectPoolManager.Instance.ReturnObject(_enemyList[i].GetEnemyType(), _enemyList[i].gameObject);
        }
        _enemyList.Clear();
    }

    public Vector3 GetRandomSpawnPos()
    {
        Vector3 originPos = transform.position;

        float width = _boxCollider.bounds.size.x;
        float height = _boxCollider.bounds.size.z;

        float randomX = UnityEngine.Random.Range((width / 2) * -1, width / 2);
        float randomZ = UnityEngine.Random.Range((height / 2) * -1, height / 2);
        Vector3 randomPos = new Vector3(randomX, 0, randomZ);
        Vector3 spawnPos = originPos + randomPos;
        return spawnPos;
    }

    private void AddEnemy(Enemy enemy)
    {
        _enemyList.Add(enemy);
        enemy.SetArea(this);
        enemy.transform.position = GetRandomSpawnPos();
    }

    public string GetMapName()
    {
        return _mapName;
    }

    public void SetDifficulty(int difficulty)
    {
        _difficulty = difficulty;
    }

    public IEnumerable GetEnemyNames()
    {
        return _enemyNames;
    }
}
