using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;
    private float damage = 1f;
    public float lifetime = 5f;
    private float timer = 0f;
    private Vector2 direction;

    public void Initialize(Vector3 position, Vector2 direction, float speed, float damage)
    {
        position = new Vector3(position.x, position.y, 0);
        transform.position = position;
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        timer = 0f;

        // 计算旋转角度以匹配移动方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        // 移动
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);

        // 生命周期
        timer += Time.deltaTime;
        if (timer >= lifetime || IsOutOfScreen())
        {
            ObjectPool.Instance.PushObject(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
            //Debug.Log("陨石碰撞，造成伤害: " + damage);
            ObjectPool.Instance.PushObject(gameObject);
        }
    }

    private bool IsOutOfScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 viewportPosition = cam.WorldToViewportPoint(transform.position);
        return viewportPosition.x < -0.1f || viewportPosition.x > 1.1f ||
               viewportPosition.y < -0.1f || viewportPosition.y > 1.1f;
    }
}