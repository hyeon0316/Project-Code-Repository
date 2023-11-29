using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssassinSound : MonoBehaviour
{
    public enum SoundName
    {
        AssassinAttack,
        AssassinWalf,
        AssassinWalfAttack,
        AssassinHit,
    }

    public void SelectSound(SoundName name)
    {
        switch (name)
        {
            case SoundName.AssassinAttack:
                SoundManager.Instance.Play("Enemy/Assassin/AssassinAttack",SoundType.Effect);
                break;
            case SoundName.AssassinWalf:
                SoundManager.Instance.Play("Enemy/Assassin/AssassinWalf",SoundType.Effect);
                break;
            case SoundName.AssassinHit:
                int rand = Random.Range(0, 2);
                switch (rand)
                {
                    case 0:
                        SoundManager.Instance.Play("Enemy/Assassin/AssassinHit1",SoundType.Effect);
                        break;
                    case 1:
                        SoundManager.Instance.Play("Enemy/Assassin/AssassinHit2",SoundType.Effect);
                        break;
                }
                break;
            case SoundName.AssassinWalfAttack:
                SoundManager.Instance.Play("Enemy/Assassin/AssassinWalfAttack",SoundType.Effect);
                break;

        }
    }
}
