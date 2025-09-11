using UnityEngine;

public class MeteoriteManager : Singleton<MeteoriteManager>
{
    public GameObject meteoritePrefab;
    public float spawnInterval = 2f;
    public float spawnDistance = 3f; // 屏幕外生成的距离
    public float meteoriteSpeed = 0.5f;

    private float timer = 0f;
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

}
