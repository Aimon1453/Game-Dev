using UnityEngine;

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
    private bool isInvincible = false;
    private float invincibleTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (UIManager.Instance != null)
        {
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
        isInvincible = true;
        invincibleTimer = invincibilityDuration;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        //Debug.Log("似了");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame();
        }
    }
}
