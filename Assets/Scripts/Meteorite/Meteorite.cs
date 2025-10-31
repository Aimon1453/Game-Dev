using UnityEngine;

public class Meteorite : MonoBehaviour, IDamageable
{
    [Header("移动")]
    public float moveSpeed = 0.2f;
    public Vector2 moveDirection;

    [Header("生命值")]
    public float maxHealth = 20f;
    private float currentHealth;

    [Header("伤害")]
    public float damage = 3f;

    [Header("掉落矿物")]
    public GameObject mineralPrefab;
    public int mineralCount = 3;
    public float mineralSpawnRadius = 0.5f;

    [Header("死亡特效")]
    public GameObject[] debrisPrefabs;  // 碎片预制体数组

    private MeteoriteVisual visual;

    private void Awake()
    {
        currentHealth = maxHealth;

        visual = GetComponentInChildren<MeteoriteVisual>();
        if (visual == null)
        {
            Debug.LogWarning("陨石没有找到视觉子物体!");
        }
    }

    void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        if (IsOutOfBigRect())
        {
            ObjectPool.Instance.PushObject(gameObject);
        }
    }

    public void Initialize(Vector3 position, Vector2 direction, float speed)
    {
        transform.position = new Vector3(position.x, position.y, 0);
        moveDirection = direction.normalized;
        moveSpeed = speed;

        // 重置生命值
        currentHealth = maxHealth;

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


    void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null)
        {
            Debug.Log("陨石碰撞，造成伤害: " + damage);
            target.TakeDamage(damage);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 生成矿物
        for (int i = 0; i < mineralCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * mineralSpawnRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            GameObject mineral = ObjectPool.Instance.GetObject(mineralPrefab, spawnPosition, Quaternion.identity);
        }

        SpawnDebris();

        ObjectPool.Instance.PushObject(gameObject);
    }

    private void SpawnDebris()
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

    private bool IsOutOfBigRect()
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

    void OnDrawGizmos()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        float margin = Object.FindFirstObjectByType<MeteoriteManager>()?.spawnDistance + 1f ?? 5f; // 取manager的spawnDistance

        Vector3 camPos = cam.transform.position;

        // 画大长方形（红色）
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            camPos,
            new Vector3(camWidth + margin * 2, camHeight + margin * 2, 0)
        );
    }
}
