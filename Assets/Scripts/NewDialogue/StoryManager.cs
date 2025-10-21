using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryManager : Singleton<StoryManager>
{
    [SerializeField] private List<DialogueData> day0Dialogues = new List<DialogueData>();
    [SerializeField] private List<DialogueData> day1Dialogues = new List<DialogueData>();
    [SerializeField] private List<DialogueData> day2Dialogues = new List<DialogueData>();
    [SerializeField] private List<DialogueData> day3Dialogues = new List<DialogueData>();
    [SerializeField] private List<DialogueData> day4Dialogues = new List<DialogueData>();
    [SerializeField] private List<DialogueData> day5Dialogues = new List<DialogueData>();

    private List<DialogueData> currentDayDialogue = new List<DialogueData>();
    public int currentDay = 0;
    private int currentDialogueIndex = 0;

    [Header("开始hacker游戏的按钮")]
    [SerializeField] public Button StartButton; // 开始按钮

    [Header("Minigame")]
    [SerializeField] private MinigameManager minigame;   // 拖场景中的 MinigameManager
    [SerializeField] private bool buildMinigameOnDayStart = true; // 进当天剧情就生成小游戏

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            StartDay(0);
            StartButton.gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartDay(1);
            StartButton.gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartDay(2);
            StartButton.gameObject.SetActive(false);
        }
    }

    public void StartDay(int day)
    {
        currentDay = Mathf.Clamp(day, 0, 5);
        currentDialogueIndex = 0;

        switch (currentDay)
        {
            case 0: currentDayDialogue = day0Dialogues; break;
            case 1: currentDayDialogue = day1Dialogues; break;
            case 2: currentDayDialogue = day2Dialogues; break;
            case 3: currentDayDialogue = day3Dialogues; break;
            case 4: currentDayDialogue = day4Dialogues; break;
            case 5: currentDayDialogue = day5Dialogues; break;
            default: currentDayDialogue = day0Dialogues; break;
        }

        // 进入剧情时，生成接线小游戏
        if (buildMinigameOnDayStart && minigame != null)
        {
            minigame.InitIfNeeded();
            minigame.gameObject.SetActive(true);
        }

        if (currentDayDialogue.Count > 0)
        {
            DialogueManager.Instance.StartDialogue(GetNextDialogue());
        }
    }

    public DialogueData GetNextDialogue()
    {
        if (currentDialogueIndex < currentDayDialogue.Count)
        {
            return currentDayDialogue[currentDialogueIndex++];
        }
        return null;
    }

    public void HandleDialogueOver(bool wasOver)
    {
        if (wasOver)
        {
            if (currentDay == 0)
            {
                currentDay = currentDay + 1;
                StartDay(currentDay);
            }
            else
            {
                DialogueManager.Instance.EndToday();
                StartCoroutine(FadeInButton(StartButton, 1f));
            }
        }
        else
        {
            DialogueManager.Instance.StartDialogue(GetNextDialogue());
        }
    }

    private IEnumerator FadeInButton(Button btn, float duration = 1f)
    {
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = btn.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        btn.gameObject.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }


}