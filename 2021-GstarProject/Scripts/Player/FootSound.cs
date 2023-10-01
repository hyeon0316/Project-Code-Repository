using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class FootSound : MonoBehaviour
{
    // Start is called before the first frame update
    string sceneName;
    public AudioClip[] footSound;
    AudioSource audioSource;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        sceneName = arg0.name;
        if (sceneName == "Town")
        {
            audioSource.clip = footSound[0];
        }
        if (sceneName == "Minor" || sceneName == "Minor 1")
        {
            audioSource.clip = footSound[1];
        }
        if (sceneName == "Room" || sceneName == "Boss" || sceneName == "Dungeon" || sceneName == "Treasure"
            || sceneName == "Battle Room 1" || sceneName == "Battle Room 2")
        {
            audioSource.clip = footSound[2];
        }
        audioSource.volume = 0.3f;
    }
    // Update is called once per frame
    void Update()
    {
        sceneName = SceneManager.GetActiveScene().name;
        if (Player.inst.isMove)
        {
            if(!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }
}
