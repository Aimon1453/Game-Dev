using UnityEngine;
using UnityEngine.UI;

public class NodeView : MonoBehaviour
{
    [Header("Refs")]
    public Image icon;  // 放节点图片（UI Image）

    [Header("Sprites (by type)")]
    public Sprite startSprite;
    public Sprite endSprite;
    public Sprite normalSprite;
    public Sprite bonusASprite;
    public Sprite bonusBSprite;
    public Sprite bonusCSprite;

    [Header("Visited Appearance")]
    public Sprite visitedSprite; // 统一的“被连接后”外观

    private NodeType _type;
    private bool _visited;

    /// <summary>
    /// 初始外观：根据类型设置
    /// </summary>
    public void Setup(NodeType type)
    {
        _type = type;
        _visited = false;
        ApplyUnvisitedSprite();
    }

    /// <summary>
    /// 进入/离开路径后的外观切换
    /// </summary>
    public void SetVisited(bool on)
    {
        _visited = on;
        if (icon == null) return;

        if (_visited)
        {
            if (visitedSprite) icon.sprite = visitedSprite;
        }
        else
        {
            ApplyUnvisitedSprite();
        }
    }

    private void ApplyUnvisitedSprite()
    {
        if (icon == null) return;

        switch (_type)
        {
            case NodeType.Start:   icon.sprite = startSprite;   break;
            case NodeType.End:     icon.sprite = endSprite;     break;
            case NodeType.BonusA:  icon.sprite = bonusASprite;  break;
            case NodeType.BonusB:  icon.sprite = bonusBSprite;  break;
            case NodeType.BonusC:  icon.sprite = bonusCSprite;  break;
            default:               icon.sprite = normalSprite;  break;
        }
    }
}