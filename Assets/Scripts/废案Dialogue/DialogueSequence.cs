// using System;
// using System.Collections.Generic;
// using UnityEngine;

// [CreateAssetMenu(fileName = "NewDialogueSequence", menuName = "Game/Dialogue Sequence")]
// public class DialogueSequence : ScriptableObject
// {
//     public List<DialogueNode> nodes = new List<DialogueNode>();
// }

// [Serializable]
// public class DialogueNode
// {
//     public string speakerName;
//     public Sprite portrait;
//     [TextArea(2, 5)] public string text;

//     public List<DialogueChoice> choices = new List<DialogueChoice>(); // 为空=线性
//     public int nextIndex = -1;  // 没有选项时从本节点跳到下一个节点索引，-1表示结束
// }

// [Serializable]
// public class DialogueChoice
// {
//     public string choiceText; // 按钮上的文字
//     public int nextIndex = -1; // 点击跳到的节点，-1=结束
// }
