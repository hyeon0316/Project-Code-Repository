public class StageNodeExecutorBase
{
    public virtual void Execute(StageNode node)
    {
        var contents = ContentsManager.Instance.Get<DungeonContents>();
        contents.DungeonProgress.LastNodeID = node.GUID;
        contents.SaveProgress();
    }
}
