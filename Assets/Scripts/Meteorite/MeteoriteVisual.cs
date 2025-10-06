using UnityEngine;
using System.Collections;

public class MeteoriteVisual : MonoBehaviour
{
    [Header("状态图片")]
    [SerializeField] private Sprite normalSprite;      // 正常状态 (血量 > 2/3)
    [SerializeField] private Sprite damagedSprite;     // 损伤状态 (血量 1/3 ~ 2/3)
    [SerializeField] private Sprite criticalSprite;    // 濒死状态 (血量 < 1/3)

    [Header("旋转设置")]
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private bool clockwise = true;

    [Header("状态切换特效")]
    [SerializeField] private float pulseScale = 1.2f;      // 放大倍数
    [SerializeField] private float pulseDuration = 0.2f;   // 完整放大缩小周期时长

    public enum MeteoriteState
    {
        Normal,
        Damaged,
        Critical
    }

    private MeteoriteState currentState = MeteoriteState.Normal;
    private SpriteRenderer spriteRenderer;
    private Meteorite meteorite;  // 父物体引用

    private Vector3 originalScale;
    private bool isPulsing = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        meteorite = GetComponentInParent<Meteorite>();

        originalScale = transform.localScale;

        if (spriteRenderer == null)
        {
            Debug.LogError("无法找到SpriteRenderer组件!");
        }

        if (meteorite == null)
        {
            Debug.LogError("无法找到父物体上的Meteorite组件!");
        }

        // 初始状态设为正常
        UpdateSprite(MeteoriteState.Normal);

        // 随机旋转方向
        clockwise = Random.value > 0.5f;
        rotationSpeed = Random.Range(20f, 60f);
    }

    private void OnEnable()
    {
        // 确保任何旧协程被停止
        StopAllCoroutines();

        // 重置脉冲状态
        isPulsing = false;

        // 重置缩放到初始值
        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
        }
    }

    private void Update()
    {
        // 旋转
        float direction = clockwise ? -1f : 1f;
        transform.Rotate(0, 0, direction * rotationSpeed * Time.deltaTime);

        // 更新状态
        UpdateState();
    }

    // 根据血量更新陨石状态
    private void UpdateState()
    {
        // 计算当前血量百分比
        float healthPercent = meteorite.GetHealthPercent();

        // 根据血量确定状态
        MeteoriteState newState;

        if (healthPercent > 0.66f)
        {
            newState = MeteoriteState.Normal;
        }
        else if (healthPercent > 0.33f)
        {
            newState = MeteoriteState.Damaged;
        }
        else
        {
            newState = MeteoriteState.Critical;
        }

        // 如果状态变化，更新精灵图片
        if (newState != currentState)
        {
            currentState = newState;
            UpdateSprite(currentState);

            // 状态变化时播放放大缩小动画
            if (!isPulsing)
            {
                StartCoroutine(PulseEffect());
            }
        }
    }

    // 根据状态更新精灵图片
    private void UpdateSprite(MeteoriteState state)
    {
        if (spriteRenderer != null)
        {
            switch (state)
            {
                case MeteoriteState.Normal:
                    spriteRenderer.sprite = normalSprite;
                    break;
                case MeteoriteState.Damaged:
                    spriteRenderer.sprite = damagedSprite;
                    break;
                case MeteoriteState.Critical:
                    spriteRenderer.sprite = criticalSprite;
                    break;
            }
        }
    }

    // 脉冲放大缩小特效
    private IEnumerator PulseEffect()
    {
        isPulsing = true;

        // 保存当前缩放
        Vector3 startScale = transform.localScale;
        Vector3 maxScale = startScale * pulseScale;

        // 第一阶段：放大
        float halfDuration = pulseDuration / 2;
        float elapsedTime = 0;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / halfDuration);
            transform.localScale = Vector3.Lerp(startScale, maxScale, t);
            yield return null;
        }

        // 第二阶段：缩小回原尺寸
        elapsedTime = 0;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / halfDuration);
            transform.localScale = Vector3.Lerp(maxScale, startScale, t);
            yield return null;
        }

        // 确保回到原始尺寸
        transform.localScale = startScale;
        isPulsing = false;
    }

    // 重置脉冲状态，防止协程重叠
    // private void OnDisable()
    // {
    //     StopAllCoroutines();
    //     isPulsing = false;
    //     transform.localScale = originalScale;
    // }

    // 提供一个方法让父物体可以设置旋转参数
    public void SetRotationParameters(float speed, bool isClockwise)
    {
        rotationSpeed = speed;
        clockwise = isClockwise;
    }
}