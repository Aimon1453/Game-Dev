using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : Singleton<DialogueManager>
{
    [Header("UI 引用")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite nightSprite;
    [SerializeField] public Sprite daySprite;
    [SerializeField] public GameObject rootPanel;
    [SerializeField] private Image profileImage;
    [SerializeField] private GameObject namePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private GameObject choiceButtonPrefab;

    [SerializeField] private GameObject dayGamePanel;   // 小游戏panel（只显示UI，不连逻辑）
    [SerializeField] private MinigameManager minigame;  // 场景里的 MinigameManager
    // [SerializeField] private Button normalButton;       // 结果：普通
    // [SerializeField] private Button excellentButton;    // 结果：优秀

    [Header("打字机效果")]
    [SerializeField] private float typingSpeed = 0.05f;

    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private Sprite lastProfile = null;

    protected override void Awake()
    {
        base.Awake();

        choicesPanel.SetActive(false);
        rootPanel.SetActive(false);

        // // 仅测试：直接用两个按钮决定分支
        // if (normalButton)    normalButton.onClick.AddListener(() => OnMinigameResult(0));
        // if (excellentButton) excellentButton.onClick.AddListener(() => OnMinigameResult(1));
    }

    void Update()
    {
        // // 快速测试：键盘直接给结果
        // if (Input.GetKeyDown(KeyCode.N)) OnMinigameResult(0); // normal
        // if (Input.GetKeyDown(KeyCode.E)) OnMinigameResult(1); // excellent
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

        nameText.text = currentDialogue.speakerName;

        if (dialogueData.namePanelBg != null)
            namePanel.GetComponent<Image>().sprite = dialogueData.namePanelBg;

        if (dialogueData.dialoguePanelBg != null)
            dialoguePanel.GetComponent<Image>().sprite = dialogueData.dialoguePanelBg;

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
                profileImage.rectTransform.sizeDelta *= 0.25f;
                profileImage.gameObject.SetActive(true);
                profileImage.color = Color.white;
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
        if (currentLineIndex >= currentDialogue.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        string line = currentDialogue.dialogueLines[currentLineIndex];

        if (line == "end")
        {
            EndDialogue();
            return;
        }

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

        // 是否显示选项
        if (currentDialogue.nodeType == DialogueData.NodeType.Choice &&
            currentLineIndex == currentDialogue.specificLineIndex)
        {
            DisplayChoices();
        }

        // —— 触发小游戏 —— //
        if (currentDialogue.minigameData != null &&
            currentLineIndex == currentDialogue.specificLineIndex)
        {
            Debug.Log($"[Dialogue] Hit minigame trigger. line={currentLineIndex}, specific={currentDialogue.specificLineIndex}, hasData={(currentDialogue.minigameData != null)}");

            continueButton.gameObject.SetActive(false);

            if (dayGamePanel != null)
            {
                dayGamePanel.SetActive(true);
                dayGamePanel.transform.SetAsLastSibling(); // 确保在最上层
                StartCoroutine(SlideDayGamePanelIn());    // 见下面“步骤2”的改法
            }
            else
            {
                Debug.LogWarning("[Dialogue] dayGamePanel is null");
            }

            // 初始化小游戏（与滑入无关，但保留）
            if (MinigameManager.Instance != null)
            {
                MinigameManager.Instance.OnFinished -= HandleMinigameFinished;
                MinigameManager.Instance.OnFinished += HandleMinigameFinished;

                MinigameManager.Instance.gameObject.SetActive(true);

                // 用对话中的 minigameData 初始化小游戏（动态加载关卡）
                MinigameManager.Instance.InitLevel(currentDialogue.minigameData);

                Debug.Log("[Dialogue] Minigame inited with dynamic level.");
            }
            else
            {
                Debug.LogWarning("[Dialogue] MinigameManager.Instance is null");
            }
        }
        else
        {
            Debug.Log($"[Dialogue] Not trigger: line={currentLineIndex}, specific={currentDialogue.specificLineIndex}, hasData={(currentDialogue.minigameData != null)}");
        }
    }

    // 收到小游戏三元组（A,B,C）后直接退场并继续
    private void HandleMinigameFinished(int countA, int countB, int countC)
    {
        Debug.Log($"[Dialogue] Minigame finished: A={countA}, B={countB}, C={countC}");

        // 防重复订阅
        if (MinigameManager.Instance != null)
            MinigameManager.Instance.OnFinished -= HandleMinigameFinished;

        // 退场后继续
        // MinigameManager.Instance?.HideAndClear();
        StartCoroutine(SlideDayGamePanelOut_ThenNextSkippingChoiceTags());
    }

    private System.Collections.IEnumerator SlideDayGamePanelOut_ThenNextSkippingChoiceTags()
    {
        if (dayGamePanel != null)
        {
            RectTransform rt = dayGamePanel.GetComponent<RectTransform>();
            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = new Vector2(250, startPos.y);
            float duration = 0.5f, t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t / duration);
                yield return null;
            }
            rt.anchoredPosition = endPos;
            dayGamePanel.SetActive(false);
        }

        // 跳过连续的分支标签（如 "choice_0", "choice_1"...）
        int next = currentLineIndex + 1;
        while (next < currentDialogue.dialogueLines.Length &&
            currentDialogue.dialogueLines[next] != null &&
            currentDialogue.dialogueLines[next].StartsWith("choice_"))
        {
            next++;
        }

        // 继续对白
        continueButton.gameObject.SetActive(true);
        currentLineIndex = next;
        DisplayCurrentLine();
    }

    // 右侧选项
    private void DisplayChoices()
    {
        foreach (Transform child in choicesPanel.transform)
            Destroy(child.gameObject);

        continueButton.gameObject.SetActive(false);

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

    public void OnChoiceSelected(int index)
    {
        choicesPanel.SetActive(false);
        continueButton.gameObject.SetActive(true);

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

    // dayGamePanel 滑入
    private IEnumerator SlideDayGamePanelIn()
    {
        RectTransform rt = dayGamePanel.GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = new Vector2(70, startPos.y);
        float duration = 0.5f, t = 0f;

        rt.anchoredPosition = new Vector2(250, startPos.y);

        while (t < duration)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(new Vector2(250, startPos.y), endPos, t / duration);
            yield return null;
        }
        rt.anchoredPosition = endPos;
    }

    // “测试按钮/快捷键”调用它来给出结果：0=普通，1=优秀
    private void OnMinigameResult(int resultIndex)
    {
        StartCoroutine(SlideDayGamePanelOut(resultIndex));
    }

    private IEnumerator SlideDayGamePanelOut(int resultIndex)
    {
        if (dayGamePanel == null) { JumpToMinigameBranch(resultIndex); yield break; }

        RectTransform rt = dayGamePanel.GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = new Vector2(250, startPos.y);
        float duration = 0.5f, t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        rt.anchoredPosition = endPos;
        dayGamePanel.SetActive(false); // 退场后隐藏

        JumpToMinigameBranch(resultIndex);
    }

    private void JumpToMinigameBranch(int index)
    {
        continueButton.gameObject.SetActive(true);

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

    public void NextLine()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = currentDialogue.dialogueLines[currentLineIndex];
            isTyping = false;

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

    private void EndDialogue()
    {
        StoryManager.Instance.HandleDialogueOver(currentDialogue.isOver);
    }

    public void EndToday()
    {
        // 只隐藏这些UI
        profileImage.gameObject.SetActive(false);
        namePanel.SetActive(false);
        dialoguePanel.SetActive(false);
        choicesPanel.SetActive(false);

        // 开始转场特效
        StartCoroutine(BackgroundTransitionEffect(nightSprite, 1f));
    }

    public IEnumerator BackgroundTransitionEffect(Sprite targetSprite, float duration = 1f)
    {
        // 1. 渐隐
        float t = 0f;
        Color c = backgroundImage.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            backgroundImage.color = c;
            yield return null;
        }
        c.a = 0f;
        backgroundImage.color = c;

        // 2. 更换背景图
        if (targetSprite != null)
            backgroundImage.sprite = targetSprite;

        // 3. 渐显
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            backgroundImage.color = c;
            yield return null;
        }
        c.a = 1f;
        backgroundImage.color = c;
    }

    // 头像切换动效（原样保留）
    private IEnumerator SwitchProfileSmooth(Sprite newProfile)
    {
        float duration = 1.5f, t = 0f;
        Color c = profileImage.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            profileImage.color = c;
            yield return null;
        }
        c.a = 0f; profileImage.color = c;

        if (newProfile != null)
        {
            profileImage.sprite = newProfile;
            profileImage.SetNativeSize();
            profileImage.rectTransform.sizeDelta *= 0.25f;
            profileImage.gameObject.SetActive(true);
        }
        else
        {
            profileImage.gameObject.SetActive(false);
            yield break;
        }

        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            profileImage.color = c;
            yield return null;
        }
        c.a = 1f; profileImage.color = c;

        SetPanelAlpha(dialoguePanel, 0f);
        SetPanelAlpha(namePanel, 0f);
        dialogueText.text = "";
        dialoguePanel.SetActive(true);
        namePanel.SetActive(true);
        yield return StartCoroutine(FadeInPanelsTogether(dialoguePanel, namePanel, 1.5f, 0.9f));
        DisplayCurrentLine();
    }

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

    private IEnumerator FadeInPanelsTogether(GameObject panelA, GameObject panelB, float duration, float targetAlpha = 0.85f)
    {
        Image imgA = panelA.GetComponent<Image>();
        Image imgB = panelB.GetComponent<Image>();
        if (imgA == null || imgB == null) yield break;

        float t = 0f;
        Color colorA = imgA.color, colorB = imgB.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, targetAlpha, t / duration);
            colorA.a = alpha; colorB.a = alpha;
            imgA.color = colorA; imgB.color = colorB;
            yield return null;
        }
        colorA.a = targetAlpha; colorB.a = targetAlpha;
        imgA.color = colorA; imgB.color = colorB;
    }
}