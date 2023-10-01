using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{

    // Start is called before the first frame update
    public static SoundManager inst = null;
    public AudioClip[] bgList;
    public AudioClip[] uiList;
    public AudioClip[] skList;
    public AudioClip[] etcList;
    public AudioClip[] bossList;
    public AudioSource bgSound;
    public int a=0;
    private void Awake()
    {
        if (inst == null)
            inst = this;
        else
        {
            Destroy(this.gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(this);
    }
    void Start()
    {
        
    }
    private void OnSceneLoaded(Scene arg0,LoadSceneMode arg1)
    {
        for(int i=0;i<bgList.Length;i++)
        {
            if (arg0.name == bgList[i].name)
            {
                BgSoundPlay(bgList[i]);
                Debug.Log("dd"+bgList[i].name);
            }
        }
    }
    public void BgSoundPlay(AudioClip clip)
    {
        bgSound.clip = clip;
        bgSound.loop = true;
        bgSound.volume = 2f;
        bgSound.Play();
    }
    public void SFXPlay(string sfxName,AudioClip clip,float vol=1f)
    {
       
        if(sfxName == "EnemyBeAttack")
        {
            a++;
            if(a>2)
            {
                return;
            }
        }
        //Debug.Log(clip.name);
        GameObject go = new GameObject(sfxName + "Sound");
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = vol;
        audioSource.Play();
       
        Destroy(go, clip.length+1f);
        
        if (sfxName == "EnemyBeAttack")
        {
            a--;
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
