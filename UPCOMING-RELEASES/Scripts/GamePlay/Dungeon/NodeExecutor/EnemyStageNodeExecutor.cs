public class EnemyStageNodeExecutor : StageNodeExecutorBase
{
    public override void Execute(StageNode node)
    {
        var enemyNode = (EnemyStageNode)node;
        var contents = ContentsManager.Instance.Get<DungeonContents>();
        contents.DungeonProgress.StageType = EDungeonState.Battle;
        contents.DungeonProgress.TerrainIndex = UnityEngine.Random.Range(0, contents.CurDungeonData.TerrainPrefabs.Count);
        base.Execute(node);
        contents.SetEnemies(enemyNode.Enemies);
    }
}
