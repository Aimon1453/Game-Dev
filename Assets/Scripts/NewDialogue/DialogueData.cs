using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New DialogueData", menuName = "Game/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public enum NodeType { Normal, Choice, Minigame }

    public string speakerName;
    public Sprite profile;
    public Sprite namePanelBg;      // 名字底框
    public Sprite dialoguePanelBg;  // 对话框底框
    /// <summary>
    /// 对话行，每行对应一次对话进度
    /// 行中写入“choice_0”，“choice_1”等用于表示选项位置
    /// 行中写入“end”表示对话结束
    /// </summary>
    [Header("对话内容，行中写入“end”表示对话结束")]
    [TextArea(2, 5)] public string[] dialogueLines; // 多行对话内容


    [Header("节点类型，Normal为普通对话，Choice为选项节点，Minigame为小游戏节点")]
    public NodeType nodeType = NodeType.Normal;

    [Header("选项节点或小游戏节点触发的行索引，值为-1表示无选项或小游戏")]
    public int specificLineIndex = -1;

    [Header("选项内容，值为null或空表示无选项")]
    public string[] choices; // 选项内容

    // 小游戏相关数据（可扩展）
    public ScriptableObject minigameData;

    [Header("在这一天的最后一个对话的data中将其设为true")]
    public bool isOver; // 是否是这一天剧情的结束
}