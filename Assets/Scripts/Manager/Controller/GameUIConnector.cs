using UnityEngine;
using TMPro;
using UnityEngine.UI;

//用于连接UIManager
public class GameUIConnector : MonoBehaviour
{
    public TextMeshProUGUI mineralsCounterText;
    public TextMeshProUGUI healthCounterText;

    [Header("血条设置")]
    public RectTransform healthBarRect;  // 血条的RectTransform
    public Image healthBarImage;
    private float healthBarMaxWidth;     // 血条最大宽度

    [SerializeField] private float smoothSpeed = 5f;  // 血条平滑变化速度
    private float targetWidth;           // 目标宽度

    void Awake()
    {
        //注册矿物数显示文本
        if (UIManager.Instance != null && mineralsCounterText != null)
        {
            UIManager.Instance.RegisterMineralsText(mineralsCounterText);
        }
        //注册血量显示文本
        if (UIManager.Instance != null && healthCounterText != null)
        {
            UIManager.Instance.RegisterHealthText(healthCounterText);
        }

        // 初始化血条
        if (healthBarRect != null)
        {
            healthBarMaxWidth = healthBarRect.sizeDelta.x;

            // 注册血条到UI管理器
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RegisterHealthBar(this);
            }
        }
    }

    void Update()
    {
        // 更新血条宽度
        if (healthBarRect != null && UIManager.Instance != null)
        {
            UpdateHealthBar(UIManager.Instance.GetPlayerHealthPercent());
        }
    }

    // 更新血条宽度方法
    public void UpdateHealthBar(float healthPercent)
    {
        if (healthBarRect == null) return;

        // 计算目标宽度
        targetWidth = healthBarMaxWidth * healthPercent;

        // 平滑过渡到目标宽度
        Vector2 sizeDelta = healthBarRect.sizeDelta;
        sizeDelta.x = Mathf.Lerp(sizeDelta.x, targetWidth, smoothSpeed * Time.unscaledDeltaTime);
        healthBarRect.sizeDelta = sizeDelta;

        // 可选：根据血量改变颜色
        if (healthBarImage != null)
        {
            // 血量低于30%时变为橙色，低于15%时变为红色
            if (healthPercent < 0.15f)
            {
                healthBarImage.color = Color.red;
            }
            else if (healthPercent < 0.3f)
            {
                healthBarImage.color = new Color(1f, 0.5f, 0f); // 橙色
            }
            else
            {
                healthBarImage.color = Color.green;
            }
        }
    }

    void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UnregisterMineralsText();
            UIManager.Instance.UnregisterHealthText();
            UIManager.Instance.UnregisterHealthBar();
        }
    }
}
