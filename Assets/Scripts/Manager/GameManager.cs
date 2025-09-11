using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private int mineralsThisSession = 0;
    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")//场景检测
        {
            mineralsThisSession = 0;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateMineralsText(mineralsThisSession);
            }

            if (MeteoriteManager.Instance != null)
            {
                MeteoriteManager.Instance.StartSpawning();
            }
        }
    }

    //捡起矿物
    public void AddMineral(int amount)
    {
        mineralsThisSession += amount;
        //Debug.Log("矿物 + " + amount + " | 本局总数: " + mineralsThisSession);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMineralsText(mineralsThisSession);
        }
    }

    //游戏结束时，结算全部的资源
    public void EndGame()
    {
        MeteoriteManager.Instance.StopSpawning();//停止生成陨石

        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ClearPool();//清空对象池
        }

        int totalMinerals = PlayerPrefs.GetInt("TotalMinerals", 0);
        totalMinerals += mineralsThisSession;
        PlayerPrefs.SetInt("TotalMinerals", totalMinerals);
        PlayerPrefs.Save();
        //Debug.Log("游戏结束！本局获得: " + mineralsThisSession + " | 新的总矿物数: " + totalMinerals);

        SceneManager.LoadScene("MainMenu");
    }
}
