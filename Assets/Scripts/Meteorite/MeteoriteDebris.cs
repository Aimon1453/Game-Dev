using UnityEngine;

public class MeteoriteDebris : MonoBehaviour
{
    [Header("移动")]
    private float moveSpeed = 2f;
    private Vector2 moveDirection;

    [Header("缩放")]
    private float shrinkSpeed = 1f;     // 每秒缩小的比例
    private float minScaleBeforeDestroy = 0.3f; // 小于此缩放比例时销毁

    private Vector3 originalScale;
    private float currentScale = 1f;
    private bool isInitialized = false;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (!isInitialized)
            return;
        // 移动
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // 缩小
        currentScale -= shrinkSpeed * Time.deltaTime;
        transform.localScale = originalScale * currentScale;

        // 检查是否需要销毁
        if (currentScale <= minScaleBeforeDestroy)
        {
            isInitialized = false;
            ObjectPool.Instance.PushObject(gameObject);
        }
    }

    /// <summary>
    /// 初始化碎片
    /// </summary>
    /// <param name="position">初始位置</param>
    /// <param name="direction">飞行方向</param>
    /// <param name="speed">飞行速度</param>
    /// <param name="shrinkRate">缩小速率</param>
    public void Initialize(Vector3 position, Vector2 direction, float speed, float shrinkRate)
    {
        position = new Vector3(position.x, position.y, 0);
        transform.position = position;

        gameObject.SetActive(true);

        Debug.Log("2");
        transform.position = position;
        moveDirection = direction.normalized;
        moveSpeed = speed;
        shrinkSpeed = shrinkRate;

        currentScale = 1f;
        transform.localScale = originalScale;

        // 随机旋转一下增加随机性
        float randomRotation = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0, 0, randomRotation);

        isInitialized = true;
    }
}