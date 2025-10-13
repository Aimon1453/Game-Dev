using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : Singleton<DialogueManager>
{
    [Header("UI 引用")]
    [SerializeField] private GameObject rootPanel; // 整个对话UI的根节点
    [SerializeField] private Image profileImage; // 头像图像
    [SerializeField] private GameObject namePanel; // 对话框面板
    [SerializeField] private TextMeshProUGUI nameText; // 名字文本
    [SerializeField] private GameObject dialoguePanel; // 对话框面板
    [SerializeField] private TextMeshProUGUI dialogueText; // 对话内容文本
    [SerializeField] private Button continueButton; // 继续按钮
    [SerializeField] private GameObject choicesPanel; // 选择面板
    [SerializeField] private GameObject choiceButtonPrefab; // 选择按钮预制体

    [SerializeField] private GameObject dayGamePanel; // 小游戏panel
    [SerializeField] private Button normalButton; // 结局1
    [SerializeField] private Button excellentButton; // 结局2

    [Header("打字机效果")]
    [SerializeField] private float typingSpeed = 0.05f;

    private DialogueData currentDialogue;//当前对话数据
    private int currentLineIndex = 0;//当前行索引
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private Sprite lastProfile = null; // 记录上一个头像

    protected override void Awake()
    {
        base.Awake();
        // 确保初始状态下UI是隐藏的
        choicesPanel.SetActive(false);
        rootPanel.SetActive(false);


        // 仅测试
        normalButton.onClick.AddListener(() => OnMinigameResult(0));
        excellentButton.onClick.AddListener(() => OnMinigameResult(1));
    }

    // 开始对话
    public void StartDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null)
        {
            Debug.LogError("传入的对话数据为空!");
            return;
        }

        currentDialogue = dialogueData;

        rootPanel.SetActive(true);
        choicesPanel.SetActive(false);

        nameText.text = currentDialogue.speakerName;// 设置角色信息

        if (dialogueData.namePanelBg != null)// 设置名字底框
            namePanel.GetComponent<Image>().sprite = dialogueData.namePanelBg;

        if (dialogueData.dialoguePanelBg != null)// 设置对话框底框
            dialoguePanel.GetComponent<Image>().sprite = dialogueData.dialoguePanelBg;

        // 从第一行开始显示
        currentLineIndex = 0;

        if (currentDialogue.profile != lastProfile)
        {
            dialoguePanel.SetActive(false);
            namePanel.SetActive(false);
            StartCoroutine(SwitchProfileSmooth(currentDialogue.profile));
            lastProfile = currentDialogue.profile;
            return;
        }
        else
        {
            if (currentDialogue.profile != null)
            {
                profileImage.sprite = currentDialogue.profile;
                profileImage.SetNativeSize();
                profileImage.rectTransform.sizeDelta *= 0.3f;
                profileImage.gameObject.SetActive(true);
                profileImage.color = Color.white; // 保证不透明
            }
            else
            {
                profileImage.gameObject.SetActive(false);
            }
        }

        DisplayCurrentLine();
    }

    // 显示当前行
    private void DisplayCurrentLine()
    {
        // 越界检测
        if (currentLineIndex >= currentDialogue.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        string line = currentDialogue.dialogueLines[currentLineIndex];//取出当前行

        // 检查是否是结束标记
        if (line == "end")
        {
            EndDialogue();
            return;
        }

        // 开始下一次打字前，清除上一次的协程
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeDialogue(line));
    }

    private IEnumerator TypeDialogue(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // 检查是否要显示选项
        if (currentDialogue.nodeType == DialogueData.NodeType.Choice &&
            currentLineIndex == currentDialogue.specificLineIndex)
        {
            DisplayChoices();
        }

        // 检查是否要显示小游戏面板
        if (currentDialogue.minigameData != null &&
            currentLineIndex == currentDialogue.specificLineIndex)
        {
            continueButton.gameObject.SetActive(false); // 隐藏继续按钮
            StartCoroutine(SlideDayGamePanelIn());
        }
    }

    // 显示选项
    private void DisplayChoices()
    {
        // 清除现有选项
        foreach (Transform child in choicesPanel.transform)
        {
            Destroy(child.gameObject);
        }

        continueButton.gameObject.SetActive(false); // 隐藏继续按钮

        // 生成新选项
        if (currentDialogue.choices != null && currentDialogue.choices.Length > 0)
        {
            for (int i = 0; i < currentDialogue.choices.Length; i++)
            {
                GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesPanel.transform);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

                buttonText.text = currentDialogue.choices[i];

                int index = i;
                button.onClick.AddListener(() => OnChoiceSelected(index));
            }

            choicesPanel.SetActive(true);
        }
    }

    // 选择选项
    public void OnChoiceSelected(int index)
    {
        choicesPanel.SetActive(false);
        continueButton.gameObject.SetActive(true); // 显示继续按钮

        // 寻找对应分支
        string branchTag = $"choice_{index}";
        for (int i = currentLineIndex + 1; i < currentDialogue.dialogueLines.Length; i++)
        {
            if (currentDialogue.dialogueLines[i] == branchTag)
            {
                currentLineIndex = i + 1; // 跳到分支标记后的第一行
                DisplayCurrentLine();
                return;
            }
        }

        // 没找到分支则结束对话
        EndDialogue();
    }

    // dayGamePanel滑入动画
    private IEnumerator SlideDayGamePanelIn()
    {
        RectTransform rt = dayGamePanel.GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = new Vector2(70, startPos.y);
        float duration = 0.5f;
        float t = 0f;

        // 先把panel放到屏幕外
        rt.anchoredPosition = new Vector2(250, startPos.y);

        while (t < duration)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(new Vector2(250, startPos.y), endPos, t / duration);
            yield return null;
        }
        rt.anchoredPosition = endPos;
    }

    private void OnMinigameResult(int resultIndex)
    {
        StartCoroutine(SlideDayGamePanelOut(resultIndex));
    }

    private IEnumerator SlideDayGamePanelOut(int resultIndex)
    {
        RectTransform rt = dayGamePanel.GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = new Vector2(250, startPos.y); // 滑出到屏幕外
        float duration = 0.5f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        rt.anchoredPosition = endPos;

        // 跳转分支
        JumpToMinigameBranch(resultIndex);
    }

    // 跳转到对应分支
    private void JumpToMinigameBranch(int index)
    {
        continueButton.gameObject.SetActive(true); // 显示继续按钮

        string branchTag = $"choice_{index}";
        for (int i = currentLineIndex + 1; i < currentDialogue.dialogueLines.Length; i++)
        {
            if (currentDialogue.dialogueLines[i] == branchTag)
            {
                currentLineIndex = i + 1;
                DisplayCurrentLine();
                return;
            }
        }
        EndDialogue();
    }

    // 下一行 - 由继续按钮触发
    public void NextLine()
    {
        // 如果正在打字，直接显示完整文本，否则开始下一行
        if (isTyping)
        {

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = currentDialogue.dialogueLines[currentLineIndex];
            isTyping = false;

            // 检测是否需要显示选项
            if (currentDialogue.nodeType == DialogueData.NodeType.Choice &&
                currentLineIndex == currentDialogue.specificLineIndex)
            {
                DisplayChoices();
            }

            return;
        }
        else
        {
            currentLineIndex++;
            DisplayCurrentLine();

        }
    }

    // 结束对话
    private void EndDialogue()
    {
        StoryManager.Instance.HandleDialogueOver(currentDialogue.isOver);
    }

    public void EndToday()
    {
        choicesPanel.SetActive(false);
        rootPanel.SetActive(false);
    }



    /// <summary>
    /// 以下属于优化范畴
    /// </summary>
    /// <param name="newProfile"></param>
    /// <returns></returns>

    private IEnumerator SwitchProfileSmooth(Sprite newProfile)
    {
        // 淡出
        float duration = 1.5f;
        float t = 0f;
        Color c = profileImage.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            profileImage.color = c;
            yield return null;
        }
        c.a = 0f;
        profileImage.color = c;

        // 切换图片
        if (newProfile != null)
        {
            profileImage.sprite = newProfile;
            profileImage.SetNativeSize();
            profileImage.rectTransform.sizeDelta *= 0.3f;
            profileImage.gameObject.SetActive(true);
        }
        else
        {
            profileImage.gameObject.SetActive(false);
            yield break;
        }

        // 淡入
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            profileImage.color = c;
            yield return null;
        }
        c.a = 1f;
        profileImage.color = c;

        SetPanelAlpha(dialoguePanel, 0f);
        SetPanelAlpha(namePanel, 0f);
        dialogueText.text = "";
        dialoguePanel.SetActive(true);
        namePanel.SetActive(true);
        yield return StartCoroutine(FadeInPanelsTogether(dialoguePanel, namePanel, 1.5f, 0.9f));
        DisplayCurrentLine();
    }

    // 设置panel透明度
    private void SetPanelAlpha(GameObject panel, float alpha)
    {
        var img = panel.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }

    // 协程淡入panel
    private IEnumerator FadeInPanelsTogether(GameObject panelA, GameObject panelB, float duration, float targetAlpha = 0.85f)
    {
        Image imgA = panelA.GetComponent<Image>();
        Image imgB = panelB.GetComponent<Image>();
        if (imgA == null || imgB == null) yield break;

        float t = 0f;
        Color colorA = imgA.color;
        Color colorB = imgB.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, targetAlpha, t / duration);
            colorA.a = alpha;
            colorB.a = alpha;
            imgA.color = colorA;
            imgB.color = colorB;
            yield return null;
        }
        colorA.a = targetAlpha;
        colorB.a = targetAlpha;
        imgA.color = colorA;
        imgB.color = colorB;
    }
}