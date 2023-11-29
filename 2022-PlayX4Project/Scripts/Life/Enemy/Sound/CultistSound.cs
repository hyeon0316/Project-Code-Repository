using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistSound : MonoBehaviour
{
    public enum SoundName
    {
        CultistAttack,
        CultistHit,
        TwistedAttack,
    }

    public void SelectSound(SoundName name)
    {
        switch (name)
        {
           case SoundName.CultistAttack:
               SoundManager.Instance.Play("Enemy/Cultist/CultistAttack",SoundType.Effect);
               break;
           case SoundName.TwistedAttack:
               SoundManager.Instance.Play("Enemy/Cultist/TwistedAttack",SoundType.Effect);
               break;
            case SoundName.CultistHit:
                int rand = Random.Range(0, 2);
                switch (rand)
                {
                    case 0:
                        SoundManager.Instance.Play("Enemy/Cultist/CultistHit1",SoundType.Effect);
                        break;
                    case 1:
                        SoundManager.Instance.Play("Enemy/Cultist/CultistHit2",SoundType.Effect);
                        break;
                }
                break;

        }
    }
}
