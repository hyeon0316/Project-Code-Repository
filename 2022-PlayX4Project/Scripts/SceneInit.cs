using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInit : MonoBehaviour
{
    [SerializeField] private Transform _spawnPos;
    void Start()
    {
        if (_spawnPos != null)
        {
           PlayerManager.Instance.Player.SetPos(_spawnPos.position);
        }

        if (SceneManager.GetActiveScene().name == "Dungeon")
        {
            PlayerManager.Instance.SetActivePlayerUI(true);
            PlayerManager.Instance.Inventory.ClearUsed();
            PlayerManager.Instance.Player.Hp = JsonToDataManager.Instance.Data.Stats[(int)LifeType.Player].Hp;
        }
        else
        {
            PlayerManager.Instance.SetActivePlayerUI(false);
        }

        CameraManager.Instance.Init();
        FadeManager.Instance.FadeOut();
    }
}
