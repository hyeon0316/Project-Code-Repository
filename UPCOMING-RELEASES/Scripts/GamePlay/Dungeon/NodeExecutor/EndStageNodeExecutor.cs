public class EndStageNodeExecutor : StageNodeExecutorBase
{
    public override void Execute(StageNode node)
    {
        var contents = ContentsManager.Instance.Get<DungeonContents>();
        contents.DungeonProgress.StageType = EDungeonState.Map;
        base.Execute(node);
        contents.NotifyNodeCompleted();
    }
}
