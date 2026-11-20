using UnityEngine;

public class PlayerNode : DialogueNode
{
    [Input] public int Entry;
    [Output] public int Exit;
    [TextArea] public string Sentence;
}
