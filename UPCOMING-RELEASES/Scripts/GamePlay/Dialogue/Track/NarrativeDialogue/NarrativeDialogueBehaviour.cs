using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class NarrativeDialogueBehaviour : PlayableBehaviour
{
    public string LocalizeKey;
    public float TypingSpeed = 0.08f;
    public bool KeepVisibleAfter = true;

    private PlayableDirector m_Director;
    private bool m_IsPlayed;

    public override void OnPlayableCreate(Playable playable)
    {
        m_Director = (playable.GetGraph().GetResolver() as PlayableDirector);
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (m_IsPlayed || !Application.isPlaying)
            return;

        m_IsPlayed = true;
        m_Director.Pause();

        DialogueManager.Instance.PlayNarrativeLine(LocalizeKey, TypingSpeed, KeepVisibleAfter, () =>
        {
            m_Director.Play();
        });
    }
}
