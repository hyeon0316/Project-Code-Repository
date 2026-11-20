using System;
using System.Collections.Generic;

public static class StageNodeExecutorRegistry
{
    private static readonly StageNodeExecutorBase s_Default = new();
    private static readonly Dictionary<Type, StageNodeExecutorBase> s_Executors = new()
    {
        { typeof(EmptyStageNode),      new EmptyStageNodeExecutor()      },
        { typeof(EnemyStageNode),      new EnemyStageNodeExecutor()      },
        { typeof(EndStageNode),        new EndStageNodeExecutor()        },
        { typeof(SelectCardStageNode), new SelectCardStageNodeExecutor() },
        { typeof(StoryStageNode),      new StoryStageNodeExecutor()      },
        { typeof(StartStageNode),      new StartStageNodeExecutor()      },
    };

    public static void Execute(StageNode node)
    {
        if (!s_Executors.TryGetValue(node.GetType(), out var executor))
            executor = s_Default;
        executor.Execute(node);
    }
}
