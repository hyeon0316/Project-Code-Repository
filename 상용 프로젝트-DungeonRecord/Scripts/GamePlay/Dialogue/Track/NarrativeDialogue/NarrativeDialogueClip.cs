using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class NarrativeDialogueClip : PlayableAsset, ITimelineClipAsset
{
    public NarrativeDialogueBehaviour Template = new NarrativeDialogueBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<NarrativeDialogueBehaviour>.Create (graph, Template);
        return playable;
    }
}
