using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("Data")]
    public DialogueSequence sequence;

    [Header("Split Trigger (choose what you want)")]
    public SplitScreenLerper lerper;          // 拖 UIFlow 上的脚本
    public bool splitOnFirstChoiceClick = true;
    public bool splitOnNextClick = false;
    public bool splitOnNodeEnter = false;

    [Header("UI")]
    public Image   portraitImage;
    public TMP_Text nameLabel;
    public TMP_Text dialogueText;

    // 整个对话气泡区域（名牌 + 气泡）
    public GameObject speechGroup;

    // 气泡右下角的 Next（Button 或任意带 Button 的物体）
    public Button nextButton;

    // 右侧选项容器（Top-Right 对齐）
    public Transform choicesContainer;

    [Header("Prefabs")]
    public Button choiceButtonPrefab;

    private int currentIndex = 0;
    private readonly List<Button> spawnedChoices = new();
    private bool didSplit = false;

    public RectTransform speechGroupRect;   // 指向 SpeechGroup 的 RectTransform
    public RectTransform dialoguePanelRect; // 指向左侧 DialoguePanel 的 RectTransform（备用）

    void Start()
    {
        // 开场全屏
        if (lerper != null) lerper.SetupFullScreen();

        if (sequence == null || sequence.nodes == null || sequence.nodes.Count == 0)
        {
            Debug.LogWarning("Dialogue sequence is empty.");
            SetUIActive(false);
            return;
        }

        SetUIActive(true);
        ShowNode(0);
    }

    void SetUIActive(bool active)
    {
        if (portraitImage)   portraitImage.gameObject.SetActive(active);
        if (nameLabel)       nameLabel.gameObject.SetActive(active);
        if (dialogueText)    dialogueText.gameObject.SetActive(active);
        if (speechGroup)     speechGroup.SetActive(active);
        if (nextButton)      nextButton.gameObject.SetActive(active);

        if (choicesContainer) choicesContainer.gameObject.SetActive(false); // 初始不显示选项
    }

    void ClearChoices()
    {
        foreach (var b in spawnedChoices)
            if (b) Destroy(b.gameObject);
        spawnedChoices.Clear();
    }

    public void TriggerSplitOnce()
    {
        if (didSplit || lerper == null) return;
        didSplit = true;
        lerper.ToSplitScreen();

        // 分屏动画结束后切到 Bottom Stretch（让宽度随左侧面板）
        StartCoroutine(ApplyLeftLayoutAfterSplit());
    }

    private System.Collections.IEnumerator ApplyLeftLayoutAfterSplit()
    {
        yield return new WaitForSecondsRealtime(lerper.duration);
        // 关键：把 SpeechGroup 从“底部居中固定宽”改成“底部拉伸贴边”
        var rt = speechGroupRect;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot     = new Vector2(0f, 0f);

        // 左/右/下边距 + 高度（和之前一致即可）
        rt.offsetMin = new Vector2(32f, 24f);   // Left, Bottom
        rt.offsetMax = new Vector2(-32f, 240f); // -Right, Height（保持高度 240）

        // 刷新一次布局，确保生效
        Canvas.ForceUpdateCanvases();
    }

    void ShowNode(int index)
    {
        if (index < 0 || index >= sequence.nodes.Count) { EndDialogue(); return; }

        currentIndex = index;
        var node = sequence.nodes[currentIndex];

        // 更新左侧头像/名牌/文本 —— 不管有没有选项都要更新
        if (portraitImage) portraitImage.sprite = node.portrait;
        if (nameLabel)     nameLabel.text      = node.speakerName;
        if (dialogueText)  dialogueText.text   = node.text;

        // 清理旧选项
        ClearChoices();

        bool hasChoices = node.choices != null && node.choices.Count > 0;

        // —— 显隐规则（与是否分屏无关）——
        if (!hasChoices)
        {
            // 无选项：左侧显示气泡+Next；右侧隐藏choices
            if (speechGroup)      speechGroup.SetActive(true);
            if (nextButton)       nextButton.gameObject.SetActive(true);
            if (choicesContainer) choicesContainer.gameObject.SetActive(false);
            return;
        }
        else
        {
            // 有选项：左侧仍显示气泡（正常显示人物与对话内容），仅隐藏 Next；
            // 右侧显示 choices（贴在对话框右侧/上方，按你的布局）
            if (speechGroup)      speechGroup.SetActive(true);
            if (nextButton)       nextButton.gameObject.SetActive(false);
            if (choicesContainer)
            {
                choicesContainer.gameObject.SetActive(true);
                choicesContainer.SetAsLastSibling(); // 避免被气泡挡住
            }
        }

        // 生成 choice 按钮（点击任意 choice -> 分屏一次，然后跳转）
        for (int i = 0; i < node.choices.Count; i++)
        {
            var c = node.choices[i];
            var btn = Instantiate(choiceButtonPrefab, choicesContainer);
            spawnedChoices.Add(btn);

            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label) label.text = c.choiceText;

            int next = c.nextIndex; // 防闭包
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                // “点击 choice 后分屏”（只会触发一次）
                TriggerSplitOnce();

                if (next < 0) EndDialogue();
                else ShowNode(next);
            });
        }
    }


    // 绑定在 Next 按钮上
    public void OnClickNext()
    {
        if (splitOnNextClick) TriggerSplitOnce();

        var node = sequence.nodes[currentIndex];
        if (node.nextIndex < 0) EndDialogue();
        else ShowNode(node.nextIndex);
    }

    void EndDialogue()
    {
        Debug.Log("Dialogue ended.");
        ClearChoices();

        if (speechGroup)      speechGroup.SetActive(false);
        if (nextButton)       nextButton.gameObject.SetActive(false);
        if (choicesContainer) choicesContainer.gameObject.SetActive(false);
        if (dialogueText)     dialogueText.text = "(对话结束)";
    }
}
