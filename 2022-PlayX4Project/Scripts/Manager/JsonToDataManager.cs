using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;

public class JsonToDataManager : SingletonAwake<JsonToDataManager>
{
    private const string SETTING_PATH = "./setting.Json";

    public StatWrapper Data { get; set; }
    [SerializeField] private StatObj[] _basicStats;
    
    public async UniTask Save()
    {
        try
        {
            using (var writer = new StreamWriter(new FileStream(SETTING_PATH, FileMode.CreateNew)))
            {
                string json = JsonUtility.ToJson(SetBasicData(), true);
                await writer.WriteAsync(json);
            }
        }
        catch (IOException)
        {
            //Debug.LogError("파일이 이미 생성되어 있습니다.");
            return;
        }
    }

    public async UniTask Load()
    {
        try
        {
            using (var reader = new StreamReader(new FileStream(SETTING_PATH, FileMode.Open)))
            {
                string json = await reader.ReadToEndAsync();
                Data = JsonUtility.FromJson<StatWrapper>(json);
                Debug.Log($"변경된 플레이어 Hp: {Data.Stats[0].Hp}");
            }
        }
        catch (Exception)//파일이 없을 경우
        {
            Data = SetBasicData();
            Debug.LogError("파일이 존재 하지 않습니다.");
            return;
        }
    }

    private StatWrapper SetBasicData()
    {
        StatWrapper data = new StatWrapper();
        data.Stats = new Stat[(int)LifeType.Max];
        for(int i = 0; i < data.Stats.Length; i++)
        {
            data.Stats[i].Id = _basicStats[i].Id;
            data.Stats[i].Hp = _basicStats[i].Hp;
            data.Stats[i].Power = _basicStats[i].Power;
            data.Stats[i].Speed = _basicStats[i].Speed;
        }
        return data;
    }

}