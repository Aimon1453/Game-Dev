using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "Game/Level")]

public class LevelSO : ScriptableObject
{
    [Header("Grid Size")]
    public int rows = 6;
    public int cols = 6;

    [Header("Typed Nodes (sparse)")]
    public List<TypedNode> typedNodes = new List<TypedNode>(); // 只填非 Normal 的点

    // —— API：供 MinigameManager 调用 —— //
    public NodeType GetNodeType(NodeCoord n)
    {
        // 边界保护（可选）
        if (n.r < 0 || n.r >= rows || n.c < 0 || n.c >= cols)
            return NodeType.Normal;

        // 稀疏表查找
        for (int i = 0; i < typedNodes.Count; i++)
        {
            var t = typedNodes[i];
            if (t.r == n.r && t.c == n.c)
                return t.type;
        }
        return NodeType.Normal;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 限制范围 & 统计 Start/End
        int startCount = 0, endCount = 0;
        for (int i = 0; i < typedNodes.Count; i++)
        {
            var t = typedNodes[i];
            t.r = Mathf.Clamp(t.r, 0, Mathf.Max(0, rows - 1));
            t.c = Mathf.Clamp(t.c, 0, Mathf.Max(0, cols - 1));
            typedNodes[i] = t;

            if (t.type == NodeType.Start) startCount++;
            if (t.type == NodeType.End)   endCount++;
        }

        if (startCount != 1)
            Debug.LogWarning($"[LevelSO] 建议恰好配置 1 个 Start（当前 {startCount}）。", this);
        if (endCount != 1)
            Debug.LogWarning($"[LevelSO] 建议恰好配置 1 个 End（当前 {endCount}）。", this);
    }
#endif
}

[Serializable]
public struct TypedNode
{
    public int r, c;
    public NodeType type;

    public TypedNode(int r, int c, NodeType type)
    {
        this.r = r; this.c = c; this.type = type;
    }
}