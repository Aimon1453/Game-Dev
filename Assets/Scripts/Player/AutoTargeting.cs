using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Experimental.GlobalIllumination;

public class AutoTargeting : MonoBehaviour
{
    [Header("检测设置")]
    public float detectRadius = 3f;
    public LayerMask meteoriteLayer;

    [Header("目标")]
    [SerializeField] private List<Transform> mineableMeteorites;
    private Transform currentTarget;

    [Header("Laser")]
    public LineRenderer laserLine;

    [Header("伤害设置")]
    public float damagePerHit = 5f;
    public float fireRate = 0.2f;
    private float fireTimer;

    void Start()
    {
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
        fireTimer = fireRate;
    }

    void Update()
    {
        UpdateTargets();
        HandleMining();
    }

    private void UpdateTargets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, meteoriteLayer);

        mineableMeteorites = hits
            .Select(hit => hit.transform)
            .OrderBy(t => Vector2.Distance(transform.position, t.position))
            .ToList();

        if (mineableMeteorites.Count > 0)
        {
            Transform closestTarget = mineableMeteorites[0];

            if (currentTarget != closestTarget)
            {
                currentTarget = closestTarget;
                //Debug.Log("新目标锁定: " + currentTarget.name);
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget = null;
                //Debug.Log("范围内无目标");
            }
        }
    }

    private void HandleMining()
    {
        if (currentTarget != null)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, transform.position);
            laserLine.SetPosition(1, currentTarget.position);

            //伤害逻辑部分
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                fireTimer = fireRate;

                IDamageable target = currentTarget.GetComponent<IDamageable>();

                if (target != null)
                {
                    target.TakeDamage(damagePerHit);
                }
            }
        }
        else
        {
            // 没有目标时，关闭激光
            laserLine.enabled = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
