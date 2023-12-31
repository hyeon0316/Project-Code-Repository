﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecromancerSound : MonoBehaviour
{
    public enum SoundName
    {
        NomalSummon,
        SpecialSummon,
        NecromancerHill,
        NecromancerHit,
    }

    public void SelectSound(SoundName name)
    {
        switch (name)
        {
            case SoundName.NomalSummon:
                SoundManager.Instance.Play("Enemy/Necromancer/NomalSummon",SoundType.Effect);
                break;
            case SoundName.SpecialSummon:
                SoundManager.Instance.Play("Enemy/Necromancer/SpecialSummon",SoundType.Effect);
                break;
            case SoundName.NecromancerHill:
                SoundManager.Instance.Play("Enemy/Necromancer/NecromancerHill",SoundType.Effect);
                break;
            case SoundName.NecromancerHit:
                int rand = Random.Range(0, 2);
                switch (rand)
                {
                    case 0:
                        SoundManager.Instance.Play("Enemy/Necromancer/NecromancerHit1",SoundType.Effect);
                        break;
                    case 1:
                        SoundManager.Instance.Play("Enemy/Necromancer/NecromancerHit2",SoundType.Effect);
                        break;
                }
                break;
        }
    }
}
