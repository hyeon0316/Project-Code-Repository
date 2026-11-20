using System;
using System.Collections.Generic;
using XNode;

public class DialogueNodeExecutorRegistry
{
    private readonly Dictionary<Type, IDialogueNodeExecutor> m_Executors = new()
    {
        { typeof(ActorNode),  new ActorNodeExecutor()  },
        { typeof(PlayerNode), new PlayerNodeExecutor() },
        { typeof(ShopNode),   new ShopNodeExecutor()   },
        { typeof(OptionNode), new OptionNodeExecutor() },
        { typeof(QuitNode),   new QuitNodeExecutor()   },
    };

    public void Execute(DialogueNode node)
    {
        if (m_Executors.TryGetValue(node.GetType(), out var executor))
            executor.Execute(node);
    }
}
