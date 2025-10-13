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
    private int currentDay = 0;
    private int currentDialogueIndex = 0;

    [Header("开始hacker游戏的按钮")]
    [SerializeField] private Button StartButton; // 开始按钮

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartDay(1);
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
            DialogueManager.Instance.EndToday();
            StartButton.gameObject.SetActive(true);
        }
        else
        {
            DialogueManager.Instance.StartDialogue(GetNextDialogue());
        }
    }
}