using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NodeView : MonoBehaviour
{
    [Header("Refs")]
    public Image dot;          // 一个圆点图（Image）
    public TMP_Text label;     // 显示编号（可选）

    [Header("States")]
    public bool isOrdered;
    public int orderIndex = -1;

    public void Setup(bool ordered, int index, Color baseColor)
    {
        isOrdered = ordered; orderIndex = index;
        if (label) label.text = ordered ? (index + 1).ToString() : "";
        if (dot)
        {
            baseColor.a = 1f;         // 强制不透明
            dot.color = baseColor;    // 节点基础色
        }
    }

    public void SetVisited(bool visited)
    {
        if (!dot) return;
        var c = dot.color;
        dot.color = visited ? Color.white : c;   // 访问后变白（你可改成发光）
    }
}
