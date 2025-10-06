using UnityEngine;
using System.Collections;

public class Virus_A : Enemy
{
    [Header("三连发设置")]
    public float burstDelay = 0.2f; // 连发间隔
    public float bulletSpeed = 6f;
    public float bulletDamage = 1.5f;

    private bool isFiring = false;

    protected override void Attack()
    {
        if (!isFiring && playerTransform != null)
        {
            StartCoroutine(FireTripleShot());
        }
    }

    private IEnumerator FireTripleShot()
    {
        isFiring = true;

        for (int i = 0; i < 3; i++)
        {
            if (playerTransform != null)
            {
                // 计算射击方向
                Vector2 shootDirection = (playerTransform.position - transform.position).normalized;

                // 从对象池获取子弹
                Vector3 bulletPosition = new Vector3(transform.position.x, transform.position.y, 0);
                GameObject bulletObj = ObjectPool.Instance.GetObject(bulletPrefab, bulletPosition, Quaternion.identity);

                Bullet bullet = bulletObj.GetComponent<Bullet>();

                // 初始化子弹
                if (bullet != null)
                {
                    bullet.Initialize(transform.position, shootDirection, bulletSpeed, bulletDamage);
                }

                // 添加射击音效或视觉效果

                // 等待下一发子弹
                yield return new WaitForSeconds(burstDelay);
            }
            else
            {
                break;
            }
        }

        isFiring = false;
    }
}
