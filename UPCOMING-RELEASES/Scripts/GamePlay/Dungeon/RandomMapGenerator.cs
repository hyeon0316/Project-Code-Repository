using System;
using System.Collections.Generic;
using UnityEngine;
using HNode;

public static class RandomMapGenerator
{
    public static RuntimeMapTree Generate(RandomMapConfig config, int seed)
    {
        var rng = new System.Random(seed);
        var tree = new RuntimeMapTree();
        float spacing = GlobalVariable.DUNGEON_MAP_NODE_SPACING;

        // 1. Layer별 노드 생성
        var layers = new List<List<Node>>(config.Layers.Count);
        for (int c = 0; c < config.Layers.Count; c++)
        {
            var layerCfg = config.Layers[c];
            int nodeCount = rng.Next(layerCfg.MinNodeCount, layerCfg.MaxNodeCount + 1);
            var layerNodes = new List<Node>(nodeCount);

            for (int r = 0; r < nodeCount; r++)
            {
                var picked = PickNodeType(layerCfg, rng);
                var nodeType = RandomNodeTypeToStageNodeType(picked);
                var node = tree.CreateNode(nodeType, r, c, MakeGuid(seed, r, c));

                // 중앙 기준 row 좌표: r번째 노드 → (r - (N-1)/2) * spacing
                float rowOffset = (r - (nodeCount - 1) / 2f) * spacing;
                node.Position = new Vector2(c * spacing, rowOffset);

                if (node is EnemyStageNode enemyNode)
                {
                    enemyNode.Grade = picked switch
                    {
                        ERandomNodeType.Elite => EEnemyGrade.Elite,
                        ERandomNodeType.Boss  => EEnemyGrade.Boss,
                        _                     => EEnemyGrade.Normal,
                    };
                    enemyNode.Enemies = PickEnemyGroup(config, picked, rng);
                }

                if (node is SelectCardStageNode cardNode)
                    cardNode.CardCategory = PickCardCategory(rng);

                layerNodes.Add(node);
            }
            layers.Add(layerNodes);
        }

        // 2. Layer 간 연결
        for (int c = 0; c < layers.Count - 1; c++)
            ConnectLayers(tree, layers[c], layers[c + 1]);

        return tree;
    }

    private static void ConnectLayers(RuntimeMapTree tree, List<Node> parents, List<Node> children)
    {
        int n = parents.Count;
        int m = children.Count;
        var assigned = new bool[m];

        //부모 i → 균등 매핑된 자식 1개
        for (int i = 0; i < n; i++)
        {
            int baseChild = (n == 1) ? (m / 2) : (int)Math.Round(i * (m - 1) / (double)(n - 1));
            tree.AddChild(parents[i], children[baseChild]);
            assigned[baseChild] = true;
        }

        //미할당 자식을 인접 부모에 추가 연결 
        for (int j = 0; j < m; j++)
        {
            if (assigned[j])
                continue;

            int bestParent = 0;
            int bestDist = int.MaxValue;
            for (int i = 0; i < n; i++)
            {
                int baseChild = (n == 1) ? (m / 2) : (int)Math.Round(i * (m - 1) / (double)(n - 1));
                int dist = Math.Abs(baseChild - j);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestParent = i;
                }
            }
            tree.AddChild(parents[bestParent], children[j]);
            assigned[j] = true;
        }

        //부모별 Children을 children 리스트 순서대로 정렬(시각적 정렬)
        foreach (var p in parents)
            p.Children.Sort((a, b) => children.IndexOf(a) - children.IndexOf(b));
    }

    private static ERandomNodeType PickNodeType(LayerConfig cfg, System.Random rng)
    {
        float total = 0f;
        foreach (var w in cfg.NodeWeights)
            total += w.Weight;

        float roll = (float)rng.NextDouble() * total;
        float acc = 0f;
        foreach (var w in cfg.NodeWeights)
        {
            acc += w.Weight;
            if (roll <= acc)
                return w.NodeType;
        }
        return cfg.NodeWeights[0].NodeType;
    }

    private static ECardCategory PickCardCategory(System.Random rng)
    {
        //HACK: Curse 비율 50%
        return rng.NextDouble() < 0.5 ? ECardCategory.Buff : ECardCategory.Curse;
    }

    private static List<EEnemyType> PickEnemyGroup(RandomMapConfig config, ERandomNodeType type, System.Random rng)
    {
        var pool = type switch
        {
            ERandomNodeType.Elite => config.EliteEnemyPool,
            ERandomNodeType.Boss  => config.BossEnemyPool,
            _                     => config.NormalEnemyPool,
        };
        if (pool == null || pool.Count == 0)
            return new List<EEnemyType>();
        var group = pool[rng.Next(pool.Count)];
        return new List<EEnemyType>(group.Enemies);
    }

    private static System.Type RandomNodeTypeToStageNodeType(ERandomNodeType type)
    {
        return type switch
        {
            ERandomNodeType.Enemy => typeof(EnemyStageNode),
            ERandomNodeType.Empty => typeof(EmptyStageNode),
            ERandomNodeType.SelectCard => typeof(SelectCardStageNode),
            ERandomNodeType.Elite => typeof(EnemyStageNode),
            ERandomNodeType.Boss  => typeof(EnemyStageNode),
            ERandomNodeType.Passage => typeof(PassageNode),
            ERandomNodeType.Start => typeof(StartStageNode),
            ERandomNodeType.End => typeof(EndStageNode),
            _ => typeof(EnemyStageNode),
        };
    }

    private static string MakeGuid(int seed, int row, int col)
    {
        return $"{seed}_{row}_{col}";
    }
}
