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
                FindObjectOfType<SoundManager>().Play("Enemy/Demon/DemonBomb",SoundType.Effect);
                break;
            case SoundName.DemonFireBall:
                FindObjectOfType<SoundManager>().Play("Enemy/Demon/DemonFireBall",SoundType.Effect);
                break;
           case SoundName.DemonAttack:
               FindObjectOfType<SoundManager>().Play("Enemy/Demon/DemonAttack",SoundType.Effect);
               break;
           case SoundName.DemonArea:
               FindObjectOfType<SoundManager>().Play("Enemy/Demon/DemonArea",SoundType.Effect);
               break;
            case SoundName.DemonDead:
                FindObjectOfType<SoundManager>().Play("Enemy/Demon/DemonDead",SoundType.Effect);
                break;

        }
    }
}
