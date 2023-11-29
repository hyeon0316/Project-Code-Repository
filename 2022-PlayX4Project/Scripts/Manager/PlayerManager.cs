using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : Singleton<PlayerManager>
{
    public Inventory Inventory => _inventory;
    public Player Player => _player;

    [SerializeField] private GameObject _playerUIObj;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Player _player;

    public void SetActivePlayerUI(bool isActive)
    {
        if (SceneManager.GetActiveScene().name.Equals("Dungeon"))
        {
            _playerUIObj.SetActive(isActive);
        }
    }
}
