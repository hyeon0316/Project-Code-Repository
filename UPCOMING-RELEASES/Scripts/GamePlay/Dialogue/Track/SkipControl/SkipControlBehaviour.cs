using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class SkipControlBehaviour : PlayableBehaviour
{
    private const float SKIP_LOADING_DURATION = 1f;

    private PlayableDirector m_Director;
    private bool m_IsRegistered;
    private bool m_IsChecked;
    private bool m_HasAnySkipMarker;

    public override void OnPlayableCreate(Playable playable)
    {
        m_Director = (playable.GetGraph().GetResolver() as PlayableDirector);
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying)
            return;

        if (!m_IsChecked)
        {
            m_IsChecked = true;
            m_HasAnySkipMarker = HasSkipMarker();
        }

        if (!m_HasAnySkipMarker)
            return;

        bool hasRemainingMarker = FindNextSkipMarkerTime() >= 0;
        if (hasRemainingMarker && !m_IsRegistered)
        {
            m_IsRegistered = true;
            DialogueManager.Instance.RegisterTimelineSkip(SkipToNextMarker);
        }
        else if (!hasRemainingMarker && m_IsRegistered)
        {
            m_IsRegistered = false;
            DialogueManager.Instance.UnregisterTimelineSkip();
        }
    }

    private bool HasSkipMarker()
    {
        if (!(m_Director.playableAsset is TimelineAsset timelineAsset))
            return false;

        foreach (var track in timelineAsset.GetOutputTracks())
        {
            foreach (var marker in track.GetMarkers())
            {
                if (marker is SkipMarker)
                    return true;
            }
        }

        return false;
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        if (!m_IsRegistered || !Application.isPlaying)
            return;

        m_IsRegistered = false;
        DialogueManager.Instance.UnregisterTimelineSkip();
    }

    private double FindNextSkipMarkerTime()
    {
        if (!(m_Director.playableAsset is TimelineAsset timelineAsset))
            return -1;

        double curTime = m_Director.time;
        double targetTime = -1;

        foreach (var track in timelineAsset.GetOutputTracks())
        {
            foreach (var marker in track.GetMarkers())
            {
                if (marker is SkipMarker skipMarker && skipMarker.time > curTime)
                {
                    if (targetTime < 0 || skipMarker.time < targetTime)
                        targetTime = skipMarker.time;
                }
            }
        }

        return targetTime;
    }

    private void SkipToNextMarker()
    {
        double targetTime = FindNextSkipMarkerTime();
        if (targetTime < 0)
            return;

        SkipToTimeAsync(targetTime).Forget();
    }

    private async UniTask SkipToTimeAsync(double targetTime)
    {
        using (new LoadingScope(ELoadingType.Background))
        {
            m_Director.Stop();
            m_Director.time = targetTime;
            m_Director.Evaluate();
            m_Director.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(SKIP_LOADING_DURATION));
        }
    }
}
