using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(NarrativeDialogueClip))]
public class NarrativeDialogueClipEditor : ClipEditor
{
    private const double FIXED_DURATION = 1.0;

    public override void OnClipChanged(TimelineClip clip)
    {
        if (clip.duration != FIXED_DURATION)
            clip.duration = FIXED_DURATION;
    }
}
