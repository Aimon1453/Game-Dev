using UnityEngine;
using System.Collections;

public class Mineral : MonoBehaviour, IPickupable
{
    [SerializeField] private int value = 1;

    [Header("拾取特效")]
    [SerializeField] private float expandScale = 1.5f;
    [SerializeField] private float expandDuration = 0.1f;
    [SerializeField] private float shrinkDuration = 1f;
    [SerializeField] private float minScaleBeforeDestroy = 0.3f; // 回池阈值
    [SerializeField] private float riseSpeed = 1.0f;

    private bool isBeingPickedUp = false;  // 防止重复拾取
    private Vector3 originalScale;         // 原始缩放
    private SpriteRenderer spriteRenderer; // Sprite渲染器

    private void Awake()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnEnable()
    {
        ResetState();
    }

    public void OnPickup()
    {
        if (isBeingPickedUp) return;  // 防止重复拾取

        isBeingPickedUp = true;

        // 通知GameManager增加矿物数量
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMineral(value);
        }
        else
        {
            Debug.LogWarning("GameManager instance is null!");
        }

        // 播放拾取特效
        StartCoroutine(PlayPickupEffect());
    }

    private IEnumerator PlayPickupEffect()
    {
        // 第一阶段：快速放大
        float elapsedTime = 0;
        Vector3 targetScale = originalScale * expandScale;

        while (elapsedTime < expandDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / expandDuration);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            transform.position += new Vector3(0, riseSpeed * Time.deltaTime, 0);
            yield return null;
        }

        // 确保达到最大缩放
        transform.localScale = targetScale;

        // 第二阶段：缓慢缩小
        elapsedTime = 0;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = originalScale * minScaleBeforeDestroy;

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / shrinkDuration);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            transform.position += new Vector3(0, riseSpeed * Time.deltaTime, 0);
            yield return null;
        }

        // 动画结束，回收到对象池
        ObjectPool.Instance.PushObject(gameObject);
    }

    // 用于重置状态，从对象池取出时确保状态正确
    public void ResetState()
    {
        StopAllCoroutines();
        transform.localScale = originalScale;
        isBeingPickedUp = false;
    }
}
