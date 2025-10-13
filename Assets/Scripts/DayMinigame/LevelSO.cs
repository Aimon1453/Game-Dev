using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ZIP/Level")]
public class LevelSO : ScriptableObject
{
    [Header("Grid")]
    public int rows = 5;    // 节点行数
    public int cols = 5;    // 节点列数

    [Header("Ordered Nodes (in order)")]
    public List<NodeCoord> orderedNodes = new(); // 必须按顺序经过的节点列表（坐标）

    [Header("Walls (blocked edges)")]
    public List<EdgeCoord> walls = new();        // 禁用的边（无向）
}
