using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class BubbleDialogueBehaviour : PlayableBehaviour
{
    public string LocalizeKey = "";
    public BubblePanel.EDirectionType Direction;
    public float TypingSpeed = 0.08f;

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

        var targetRenderer = playerData as Renderer;

        m_IsPlayed = true;
        m_Director.Pause();

        Vector3 worldPos = targetRenderer != null
            ? CharacterPositionUtil.GetOverHeadPos(targetRenderer)
            : Vector3.zero;

        DialogueManager.Instance.PlayBubbleLine(LocalizeKey, TypingSpeed, worldPos, Direction, () =>
        {
            m_Director.Play();
        });
    }
}
