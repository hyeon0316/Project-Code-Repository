using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class SkipControlClip : PlayableAsset, ITimelineClipAsset
{
    public SkipControlBehaviour Template = new SkipControlBehaviour();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<SkipControlBehaviour>.Create(graph, Template);
        return playable;
    }
}
