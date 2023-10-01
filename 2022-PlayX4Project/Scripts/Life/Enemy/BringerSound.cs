using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BringerSound : MonoBehaviour
{
    public enum SoundName
    {
        BringerAttack,
        BringerSkill,
        BringerHit,
        BringerDead,
    }

    public void SelectSound(SoundName name)
    {
        switch (name)
        {
            case SoundName.BringerAttack:
                FindObjectOfType<SoundManager>().Play("Enemy/Bringer/BringerAttack",SoundType.Effect);
                break;
            case SoundName.BringerSkill:
                FindObjectOfType<SoundManager>().Play("Enemy/Bringer/BringerSkill",SoundType.Effect);
                break;
            case SoundName.BringerHit:
                int rand = Random.Range(0, 2);
                switch (rand)
                {
                    case 0:
                        FindObjectOfType<SoundManager>().Play("Enemy/Bringer/BringerHit1",SoundType.Effect);
                        break;
                    case 1:
                        FindObjectOfType<SoundManager>().Play("Enemy/Bringer/BringerHit2",SoundType.Effect);
                        break;
                }
                break;
            case SoundName.BringerDead:
                FindObjectOfType<SoundManager>().Play("Enemy/Bringer/BringerDead",SoundType.Effect);
                break;

        }
    }
}
