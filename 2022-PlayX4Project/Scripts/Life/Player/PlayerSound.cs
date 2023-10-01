using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSound : MonoBehaviour
{
    public enum SoundName
    {
        TownFoot,
        XAttack,
        PlayerHit,
        PlayerJump,
        PlayerRoll,
        FlyAttack,
        ZAttack1,
        ZAttack2,
        DashAttack,
        GunSound,
    }
    
    public void SelectSound(SoundName name)
    {
        switch (name)
        {
            case SoundName.TownFoot:
                if(SceneManager.GetActiveScene().name.Equals("Town"))
                    FindObjectOfType<SoundManager>().Play("Player/TownFoot",SoundType.Effect);
                else if(SceneManager.GetActiveScene().name.Equals("Dungeon"))
                    FindObjectOfType<SoundManager>().Play("Player/DungeonFoot",SoundType.Effect);
                break;
            case SoundName.XAttack:
                FindObjectOfType<SoundManager>().Play("Player/XAttack",SoundType.Effect);
                break;
            case SoundName.PlayerHit:
                int rand = Random.Range(0, 2);
                switch (rand)
                {
                    case 0:
                        FindObjectOfType<SoundManager>().Play("Player/PlayerHit1",SoundType.Effect);
                        break;
                    case 1:
                        FindObjectOfType<SoundManager>().Play("Player/PlayerHit2",SoundType.Effect);
                        break;
                }
                break;
            case SoundName.PlayerJump:
                FindObjectOfType<SoundManager>().Play("Player/PlayerJump",SoundType.Effect);
                break;
            case SoundName.PlayerRoll:
                FindObjectOfType<SoundManager>().Play("Player/PlayerRoll",SoundType.Effect);
                break;
            case SoundName.FlyAttack:
                FindObjectOfType<SoundManager>().Play("Player/FlyAttack",SoundType.Effect);
                break;
            case SoundName.ZAttack1:
                FindObjectOfType<SoundManager>().Play("Player/ZAttack1",SoundType.Effect);
                break;
            case SoundName.ZAttack2:
                FindObjectOfType<SoundManager>().Play("Player/ZAttack2",SoundType.Effect);
                break;
            case SoundName.DashAttack:
                FindObjectOfType<SoundManager>().Play("Player/DashAttack",SoundType.Effect);
                break;
            case SoundName.GunSound:
                FindObjectOfType<SoundManager>().Play("Player/GunSound",SoundType.Effect);
                break;
        }
    }
}
