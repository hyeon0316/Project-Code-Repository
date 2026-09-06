using UnityEngine;

public class ActorNode : DialogueNode
{
    [Input] public int Entry;
    [Output] public int Exit;
    [TextArea] public string Sentence;
}
