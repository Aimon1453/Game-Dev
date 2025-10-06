using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [Header("旋转设置")]
    [SerializeField] private float rotationSpeed = 60f;
    private Vector2 targetDirection = Vector2.up; // 初始目标方向为上

    [Header("状态图片")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite miningSprite;
    private ShipState currentState = ShipState.Normal;  // 当前状态

    [Header("无敌视觉效果")]
    [SerializeField] private float blinkRate = 0.1f; // 闪烁频率
    [SerializeField] private Color invincibleTint = new Color(0.7f, 0.7f, 1.0f, 1.0f);

    public enum ShipState
    {
        Normal,
        Mining
    }

    private SpriteRenderer spriteRenderer;
    private AutoTargeting autoTargeting;
    private PlayerShip playerShip;
    private Color originalColor;
    private bool wasInvincible = false;

    private void Start()
    {
        playerShip = GetComponentInParent<PlayerShip>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        autoTargeting = GetComponentInParent<AutoTargeting>();

        if (playerShip == null)
        {
            Debug.LogError("无法找到父物体上的PlayerShip组件!");
        }
        if (spriteRenderer == null)
        {
            Debug.LogError("无法找到SpriteRenderer组件!");
        }

        if (autoTargeting == null)
        {
            Debug.LogError("无法找到父物体上的AutoTargeting组件!");
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 确保初始朝上
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void Update()
    {
        UpdateShipState();       // 更新飞船状态
        UpdateTargetDirection();
        RotateTowardsDirection();
        UpdateInvincibilityVisual(); // 更新无敌状态视觉效果
    }

    //用于根据当前目标更新飞船状态
    private void UpdateShipState()
    {
        // 根据是否有当前目标决定状态
        ShipState newState = (autoTargeting != null && autoTargeting.HasTarget())
                           ? ShipState.Mining
                           : ShipState.Normal;

        if (newState != currentState)
        {
            currentState = newState;
            UpdateSprite();
        }
    }

    private void UpdateSprite()
    {
        if (spriteRenderer != null)
        {
            switch (currentState)
            {
                case ShipState.Normal:
                    spriteRenderer.sprite = normalSprite;
                    break;
                case ShipState.Mining:
                    spriteRenderer.sprite = miningSprite;
                    break;
            }
        }
    }

    private void UpdateTargetDirection()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // 只有当有输入时才更新目标方向
        if (Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveY) > 0.01f)
        {
            targetDirection = new Vector2(moveX, moveY).normalized;
        }
    }

    private void RotateTowardsDirection()
    {
        if (targetDirection.sqrMagnitude < 0.01f)
            return; // 防止零向量

        // 计算目标角度（0度是右方向，逆时针增加）
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90f;

        float currentAngle = transform.eulerAngles.z;

        float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

        float rotationAmount = Mathf.Sign(angleDiff) * Mathf.Min(rotationSpeed * Time.deltaTime, Mathf.Abs(angleDiff));

        transform.Rotate(0, 0, -rotationAmount);
    }

    // 更新无敌状态的视觉效果
    private void UpdateInvincibilityVisual()
    {
        if (playerShip == null || spriteRenderer == null) return;

        // 直接使用公开的isInvincible变量
        if (playerShip.isInvincible)
        {
            // 闪烁效果
            bool visible = Mathf.FloorToInt(Time.time / blinkRate) % 2 == 0;
            spriteRenderer.enabled = visible;

            // 确保颜色变化
            if (visible && !wasInvincible) // 仅在首次开始无敌状态时更改颜色
            {
                spriteRenderer.color = invincibleTint;
            }
        }
        else if (wasInvincible) // 如果刚从无敌状态退出
        {
            // 恢复正常显示
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }

        // 更新前一帧的无敌状态
        wasInvincible = playerShip.isInvincible;
    }
}