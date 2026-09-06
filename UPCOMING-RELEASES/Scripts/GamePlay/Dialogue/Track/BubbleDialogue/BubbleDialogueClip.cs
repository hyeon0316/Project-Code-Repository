using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class BubbleDialogueClip : PlayableAsset, ITimelineClipAsset
{
    public BubbleDialogueBehaviour Template = new BubbleDialogueBehaviour();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<BubbleDialogueBehaviour>.Create(graph, Template);
        return playable;
    }
}
