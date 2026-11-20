public class SelectCardStageNodeExecutor : StageNodeExecutorBase
{
    public override void Execute(StageNode node)
    {
        var selectCardNode = (SelectCardStageNode)node;
        var contents = ContentsManager.Instance.Get<DungeonContents>();
        contents.DungeonProgress.StageType = EDungeonState.SelectCard;
        contents.CurCardCategory = selectCardNode.CardCategory;
        base.Execute(node);
        contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.SelectCard));
    }
}
