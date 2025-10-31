using System.Collections.Generic;
using UnityEngine;

public class MeteoriteManager : Singleton<MeteoriteManager>
{
    [Header("prefab")]
    public GameObject meteoritePrefab;
    public GameObject enemyPrefab;

    [Header("生成设置")]
    public float spawnInterval = 2f;
    public float spawnDistance = 3f; // 屏幕外生成的距离
    public float meteoriteSpeed = 0.5f; // 陨石移动速度
    public float enemySpeed = 1.0f;
    public int enemySpawnRate = 0; // 敌人生成速率，0表示不生成


    private float timer = 0f;
    private float enemyTimer = 0f;
    private Camera mainCamera;
    private bool isSpawning = false;

    void Update()
    {
        if (!isSpawning) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnMeteorite();
        }

        if (enemySpawnRate > 0)
        {
            enemyTimer += Time.deltaTime;
            if (enemyTimer >= enemySpawnRate)
            {
                enemyTimer = 0f;
                SpawnEnemy();
            }
        }
    }

    public void StartSpawning()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            isSpawning = true;
            timer = spawnInterval; // 确保一开始就生成一个
            Debug.Log("Meteorite Spawning has STARTED.");
        }
        else
        {
            Debug.LogError("MeteoriteManager could not find a Main Camera!");
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("Meteorite Spawning has STOPPED.");
    }

    void SpawnMeteorite()
    {
        if (mainCamera == null) return;

        int edge = Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;
        Vector3 screenCenter = mainCamera.transform.position;
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;

        float angle = 0f;

        switch (edge)
        {
            case 0: // 上
                spawnPos = screenCenter + new Vector3(Random.Range(-camWidth / 2, camWidth / 2), camHeight / 2 + spawnDistance, 0);
                // 方向：向下的120度扇形（210°~330°）
                angle = Random.Range(210f, 330f);
                break;
            case 1: // 下
                spawnPos = screenCenter + new Vector3(Random.Range(-camWidth / 2, camWidth / 2), -camHeight / 2 - spawnDistance, 0);
                // 方向：向上的120度扇形（30°~150°）
                angle = Random.Range(30f, 150f);
                break;
            case 2: // 左
                spawnPos = screenCenter + new Vector3(-camWidth / 2 - spawnDistance, Random.Range(-camHeight / 2, camHeight / 2), 0);
                // 方向：向右的120度扇形（-60°~60°，即300°~60°）
                angle = Random.Range(-60f, 60f);
                break;
            case 3: // 右
                spawnPos = screenCenter + new Vector3(camWidth / 2 + spawnDistance, Random.Range(-camHeight / 2, camHeight / 2), 0);
                // 方向：向左的120度扇形（120°~240°）
                angle = Random.Range(120f, 240f);
                break;
        }

        // 角度转为弧度
        float rad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        GameObject meteoriteObj = ObjectPool.Instance.GetObject(meteoritePrefab, spawnPos, Quaternion.identity);
        Meteorite meteorite = meteoriteObj.GetComponent<Meteorite>();
        meteorite.Initialize(spawnPos, direction, meteoriteSpeed);
    }

    void SpawnEnemy()
    {
        if (mainCamera == null || enemyPrefab == null) return;

        Vector3 screenCenter = mainCamera.transform.position;
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;

        // 敌人从屏幕外的随机一边生成
        int edge = Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;

        switch (edge)
        {
            case 0: // 上
                spawnPos = screenCenter + new Vector3(Random.Range(-camWidth / 2, camWidth / 2), camHeight / 2 + spawnDistance, 0);
                break;
            case 1: // 下
                spawnPos = screenCenter + new Vector3(Random.Range(-camWidth / 2, camWidth / 2), -camHeight / 2 - spawnDistance, 0);
                break;
            case 2: // 左
                spawnPos = screenCenter + new Vector3(-camWidth / 2 - spawnDistance, Random.Range(-camHeight / 2, camHeight / 2), 0);
                break;
            case 3: // 右
                spawnPos = screenCenter + new Vector3(camWidth / 2 + spawnDistance, Random.Range(-camHeight / 2, camHeight / 2), 0);
                break;
        }

        // 敌人的目标位置是屏幕内的随机位置
        Vector3 targetPos = screenCenter + new Vector3(
            Random.Range(-camWidth * 0.4f, camWidth * 0.4f),
            Random.Range(-camHeight * 0.4f, camHeight * 0.4f),
            0
        );

        // 从对象池获取敌人
        GameObject enemyObj = ObjectPool.Instance.GetObject(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Initialize(spawnPos, targetPos, enemySpeed);
            // Debug.Log("已生成敌人，目标位置: " + targetPos);
        }
    }

}
