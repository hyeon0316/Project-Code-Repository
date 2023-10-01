using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class DataManager : MonoBehaviour
{
    private readonly string _settingPath = "./setting.Json";
    public SetData Data;

    private static DataManager _instance = null;

    public static DataManager Instance()
    {
        return _instance;
    }
    private void Awake()
    {
        Data = new SetData();
   
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else 
        {
            if (this != _instance)
            {
                Destroy(this.gameObject);
            }
        }
    }

    public void Save()
    {
        try
        {
            using (var writer = new StreamWriter(new FileStream(_settingPath, FileMode.CreateNew)))
            {
                string json = JsonUtility.ToJson(Data, true);
                writer.Write(json);
            }
        }
        catch (IOException)
        {
            Debug.LogError("파일이 이미 생성되어 있습니다.");
            return;
        }
    }

    public void Load()
    {
        try
        {
            using (var reader = new StreamReader(new FileStream(_settingPath, FileMode.Open)))
            {
                Data = JsonUtility.FromJson<SetData>(reader.ReadToEnd());
                Debug.Log($"변경된 플레이어 Hp: {Data.PlayerHp}");
            }
        }
        catch (Exception)//파일이 없을 경우
        {
            Debug.LogError("파일이 존재 하지 않습니다.");
            return;
        }
    }

}