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
                    SoundManager.Instance.Play("Player/TownFoot",SoundType.Effect);
                else if(SceneManager.GetActiveScene().name.Equals("Dungeon"))
                    SoundManager.Instance.Play("Player/DungeonFoot",SoundType.Effect);
                break;
            case SoundName.XAttack:
                SoundManager.Instance.Play("Player/XAttack",SoundType.Effect);
                break;
            case SoundName.PlayerHit:
                int rand = Random.Range(0, 2);
                switch (rand)
                {
                    case 0:
                        SoundManager.Instance.Play("Player/PlayerHit1",SoundType.Effect);
                        break;
                    case 1:
                        SoundManager.Instance.Play("Player/PlayerHit2",SoundType.Effect);
                        break;
                }
                break;
            case SoundName.PlayerJump:
                SoundManager.Instance.Play("Player/PlayerJump",SoundType.Effect);
                break;
            case SoundName.PlayerRoll:
                SoundManager.Instance.Play("Player/PlayerRoll",SoundType.Effect);
                break;
            case SoundName.FlyAttack:
                SoundManager.Instance.Play("Player/FlyAttack",SoundType.Effect);
                break;
            case SoundName.ZAttack1:
                SoundManager.Instance.Play("Player/ZAttack1",SoundType.Effect);
                break;
            case SoundName.ZAttack2:
                SoundManager.Instance.Play("Player/ZAttack2",SoundType.Effect);
                break;
            case SoundName.DashAttack:
                SoundManager.Instance.Play("Player/DashAttack",SoundType.Effect);
                break;
            case SoundName.GunSound:
                SoundManager.Instance.Play("Player/GunSound",SoundType.Effect);
                break;
        }
    }
}
