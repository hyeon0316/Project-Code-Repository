using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigCultistSound : MonoBehaviour
{
    public enum SoundName
    {
        BigCultistAttack1,
        BigCultistAttack2,
        BigCultistHit,
    }

    public void SelectSound(SoundName name)
    {
        switch (name)
        {
            case SoundName.BigCultistAttack1:
                FindObjectOfType<SoundManager>().Play("Enemy/BigCultist/BigCultistAttack1",SoundType.Effect);
                break;
            case SoundName.BigCultistAttack2:
                FindObjectOfType<SoundManager>().Play("Enemy/BigCultist/BigCultistAttack2",SoundType.Effect);
                break;
            case SoundName.BigCultistHit:
                int rand = Random.Range(0, 2);
                switch (rand)
                {
                    case 0:
                        FindObjectOfType<SoundManager>().Play("Enemy/BigCultist/BigCultistHit1",SoundType.Effect);
                        break;
                    case 1:
                        FindObjectOfType<SoundManager>().Play("Enemy/BigCultist/BigCultistHit2",SoundType.Effect);
                        break;
                }
                break;

        }
    }
}
