using UnityEngine;
using System.Collections;

public class SplitScreenLerper : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform dialoguePanel;
    public RectTransform minigamePanel;
    public CanvasGroup   minigameCg; // 可选：淡入

    [Header("Layout")]
    [Range(0f, 1f)] public float dialogueWidthPercent = 0.45f;
    public float duration = 0.5f;

    public void SetupFullScreen()
    {
        if (!dialoguePanel || !minigamePanel) return;

        // 左侧：全屏
        SetAnchors(dialoguePanel, new Vector2(0f, 0f), new Vector2(1f, 1f));

        // 右侧：激活，但先收缩为 0 宽（靠右边缘）
        minigamePanel.gameObject.SetActive(true);
        SetAnchors(minigamePanel, new Vector2(1f, 0f), new Vector2(1f, 1f));

        if (minigameCg)
        {
            minigameCg.alpha = 1f;            // 不做淡入
            minigameCg.blocksRaycasts = false; // 动画完成前先不接收点击
        }
    }

    public void ToSplitScreen()
    {
        if (!isActiveAndEnabled) return;
        StopAllCoroutines();
        StartCoroutine(LerpSplitOnly());
    }

    private IEnumerator LerpSplitOnly()
    {
        if (!dialoguePanel || !minigamePanel) yield break;

        float t = 0f;
        float start = 1f;
        float target = Mathf.Clamp01(dialogueWidthPercent); // 例如 0.45

        // 避免飞入：确保右侧一开始就在右边，0 宽
        SetAnchors(minigamePanel, new Vector2(1f, 0f), new Vector2(1f, 1f));

        while (t < duration)
        {
            t += Time.deltaTime;
            float split = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / duration));

            // 左面板：0 ~ split
            SetAnchors(dialoguePanel, new Vector2(0f, 0f), new Vector2(split, 1f));
            // 右面板：split ~ 1
            SetAnchors(minigamePanel, new Vector2(split, 0f), new Vector2(1f, 1f));
            yield return null;
        }

        // 终值再“落锚”一次，防数值漂移
        SetAnchors(dialoguePanel,  new Vector2(0f, 0f), new Vector2(target, 1f));
        SetAnchors(minigamePanel,  new Vector2(target, 0f), new Vector2(1f, 1f));

        if (minigameCg) minigameCg.blocksRaycasts = true;
    }

    void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; // 关键：清零偏移
    }
}
