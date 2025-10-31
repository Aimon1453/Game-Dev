using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("基础属性")]
    public float maxHealth = 15f;
    protected float currentHealth;

    [Header("移动")]
    public float moveSpeed = 1.0f;
    public Vector2 moveDirection;
    public Vector3 targetPosition;
    protected bool reachedTarget = false;

    [Header("伤害")]
    public float damage = 2f;

    [Header("攻击")]
    public GameObject bulletPrefab; // 子弹预制体
    public float attackRange = 10f; // 攻击范围
    public float attackCooldown = 2f; // 攻击冷却时间
    protected float attackTimer;
    protected Transform playerTransform; // 玩家的位置

    [Header("死亡特效")]
    public GameObject[] debrisPrefabs;  // 碎片预制体数组

    protected EnemyVisual visual;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        attackTimer = attackCooldown;

        visual = GetComponentInChildren<EnemyVisual>();
        if (visual == null)
        {
            Debug.LogWarning("敌人没有找到视觉子物体!");
        }
    }

    protected virtual void Start()
    {
        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("敌人无法找到玩家对象!");
        }
    }

    protected virtual void Update()
    {
        if (!reachedTarget)
        {
            // 计算到目标的距离
            float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

            // 如果还没到达目标位置，继续移动
            if (distanceToTarget > 0.1f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );
            }
            else
            {
                reachedTarget = true;
                OnReachedTarget();
            }
        }
        else
        {
            // 已到达目标位置，处理攻击逻辑
            HandleAttack();
        }

        // 检查是否超出屏幕范围
        if (IsOutOfBigRect())
        {
            ObjectPool.Instance.PushObject(gameObject);
        }
    }

    // 抵达目标位置时调用
    protected virtual void OnReachedTarget()
    {
        Debug.Log("敌人到达目标位置");
    }

    // 抽象方法：子类必须实现的攻击逻辑
    protected abstract void Attack();

    // 处理攻击冷却和攻击判断
    protected virtual void HandleAttack()
    {
        // 攻击冷却计时
        if (attackTimer < attackCooldown)
        {
            attackTimer += Time.deltaTime;
            return;
        }

        // 检查玩家是否在攻击范围内
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                Attack();
                attackTimer = 0f; // 重置攻击计时器
            }
        }
    }

    public virtual void Initialize(Vector3 position, Vector3 targetPos, float speed)
    {

        position = new Vector3(position.x, position.y, 0);
        targetPos = new Vector3(targetPos.x, targetPos.y, 0);

        transform.position = position;
        targetPosition = targetPos;
        moveSpeed = speed;
        reachedTarget = false;
        currentHealth = maxHealth;
        attackTimer = attackCooldown;

        // 计算移动方向
        moveDirection = (targetPosition - position).normalized;

        // 随机设置旋转参数
        if (visual != null)
        {
            visual.SetRotationParameters(Random.Range(20f, 60f), Random.value > 0.5f);
        }
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null)
        {
            Debug.Log("敌人碰撞，造成伤害: " + damage);
            target.TakeDamage(damage);
        }
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        SpawnDebris();
        ObjectPool.Instance.PushObject(gameObject);
    }

    protected virtual void SpawnDebris()
    {
        if (debrisPrefabs == null || debrisPrefabs.Length == 0)
            return;

        Vector3 centerPos = transform.position;
        float randomAngle = Random.Range(0f, 90f);

        for (int i = 0; i < 4; i++)
        {
            // 计算方向
            float angle = i * 90f + randomAngle;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // 选择一个碎片预制体
            GameObject debrisPrefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Length)];
            GameObject debris = ObjectPool.Instance.GetObject(debrisPrefab, centerPos, Quaternion.identity);

            // 初始化
            MeteoriteDebris debrisComponent = debris.GetComponent<MeteoriteDebris>();
            if (debrisComponent != null)
            {
                float speed = Random.Range(3f, 4.5f);
                float shrinkRate = Random.Range(0.7f, 1.3f);
                debrisComponent.Initialize(centerPos, direction, speed, shrinkRate);
            }
        }
    }

    protected bool IsOutOfBigRect()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        float margin = Object.FindFirstObjectByType<MeteoriteManager>()?.spawnDistance + 1f ?? 5f;

        Vector3 camPos = cam.transform.position;
        Vector3 pos = transform.position;

        float left = camPos.x - camWidth / 2 - margin;
        float right = camPos.x + camWidth / 2 + margin;
        float bottom = camPos.y - camHeight / 2 - margin;
        float top = camPos.y + camHeight / 2 + margin;

        return pos.x < left || pos.x > right || pos.y < bottom || pos.y > top;
    }
}