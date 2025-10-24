using UnityEngine;
using System.Collections;

public class PlayerShip : MonoBehaviour, IDamageable
{
    [Header("生命值")]
    [SerializeField] public float maxHealth = 20f;
    [SerializeField] public float degenerationPerSecond = 1f; // 每秒自动消耗
    [SerializeField] private float currentHealth;
    private bool isDead = false;

    [Header("移动")]
    [SerializeField] public float moveSpeed = 5f;

    [Header("无敌帧")]
    public float invincibilityDuration = 1f; // 无敌时间
    public bool isInvincible = false;
    private float invincibleTimer = 0f;

    [Header("子弹时间")]
    [SerializeField] private float timeSlowFactor = 0.25f; // 时间减慢到正常的一半
    [SerializeField] private float timeSlowDuration = 0.5f; // 时间减慢持续时间
    [SerializeField] private float timeRestoreDuration = 0.3f; // 时间恢复所需时间
    private bool isTimeSlowed = false;

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RegisterPlayerShip(this);
            UIManager.Instance.UpdateHealthText(currentHealth);
        }
    }

    void Update()
    {
        if (isDead) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(moveX, moveY, 0).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        currentHealth -= degenerationPerSecond * Time.deltaTime;

        if (UIManager.Instance != null)//更新血量UI
        {
            UIManager.Instance.UpdateHealthText(currentHealth);
        }
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        //无敌帧计时
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            if (!isTimeSlowed)
            {
                StartCoroutine(SlowTimeEffect());
            }
        }

        isInvincible = true;
        invincibleTimer = invincibilityDuration;
    }

    private IEnumerator SlowTimeEffect()
    {
        isTimeSlowed = true;

        // 快速减慢时间
        float startTime = Time.unscaledTime;
        float t = 0;
        float initialTimeScale = Time.timeScale;

        while (t < 0.1f) // 快速减速阶段
        {
            t = (Time.unscaledTime - startTime) / 0.1f;
            Time.timeScale = Mathf.Lerp(initialTimeScale, timeSlowFactor, t);
            yield return null;
        }

        // 保持减慢状态
        Time.timeScale = timeSlowFactor;

        // 等待指定时间
        yield return new WaitForSecondsRealtime(timeSlowDuration);

        // 逐渐恢复正常时间流速
        startTime = Time.unscaledTime;
        t = 0;

        while (t < timeRestoreDuration)
        {
            t = (Time.unscaledTime - startTime) / timeRestoreDuration;
            Time.timeScale = Mathf.Lerp(timeSlowFactor, 1f, t);
            yield return null;
        }

        // 确保时间恢复正常
        Time.timeScale = 1f;
        isTimeSlowed = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        //Debug.Log("似了");

        // 确保死亡时时间恢复正常
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            //Debug.Log("玩家死亡");
            GameManager.Instance.EndGame();
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    // 在游戏暂停或场景切换时确保时间缩放被重置
    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
