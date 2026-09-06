using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

[CreateAssetMenu(menuName = "SO/Dialogue/Graph")]
public class DialogueGraph : NodeGraph
{
    public bool AllowSkip => m_AllowSkip;

    [SerializeField] private bool m_AllowSkip;

    public OptionNode GetOptionNode()
    {
        return nodes.OfType<OptionNode>().FirstOrDefault();
    }
}
