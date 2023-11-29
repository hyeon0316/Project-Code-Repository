using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonSound : MonoBehaviour
{
    public enum SoundName
    {
        DemonAttack,
        DemonArea,
        DemonDead,
        DemonFireBall,
        DemonBomb,
    }

    public void SelectSound(SoundName name)
    {
        switch (name)
        {
            case SoundName.DemonBomb:
                SoundManager.Instance.Play("Enemy/Demon/DemonBomb",SoundType.Effect);
                break;
            case SoundName.DemonFireBall:
                SoundManager.Instance.Play("Enemy/Demon/DemonFireBall",SoundType.Effect);
                break;
           case SoundName.DemonAttack:
               SoundManager.Instance.Play("Enemy/Demon/DemonAttack",SoundType.Effect);
               break;
           case SoundName.DemonArea:
               SoundManager.Instance.Play("Enemy/Demon/DemonArea",SoundType.Effect);
               break;
            case SoundName.DemonDead:
                SoundManager.Instance.Play("Enemy/Demon/DemonDead",SoundType.Effect);
                break;

        }
    }
}
