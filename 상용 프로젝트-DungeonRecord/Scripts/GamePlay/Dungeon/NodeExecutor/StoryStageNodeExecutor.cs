public class StoryStageNodeExecutor : StageNodeExecutorBase
{
    public override void Execute(StageNode node)
    {
        var contents = ContentsManager.Instance.Get<DungeonContents>();
        contents.DungeonProgress.StageType = EDungeonState.Story;
        base.Execute(node);
        contents.SetStory(((StoryStageNode)node).StorySceneGUID);
    }
}
