using System.Collections;
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
            DialogueManager.Instance.rootPanel.SetActive(false);

            // 隐藏开始按钮
            if (StoryManager.Instance != null && StoryManager.Instance.StartButton != null)
                StoryManager.Instance.StartButton.gameObject.SetActive(false);

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

        if (scene.name == "Clinic_Day")
        {
            DialogueManager.Instance.rootPanel.SetActive(true);
            StartCoroutine(DelayStartDay());
            //Debug.Log("进入Clinic_Day场景，开始当天剧情");
        }
    }


    private IEnumerator DelayStartDay()
    {
        yield return null; // 等待一帧
        StoryManager.Instance.StartDay(StoryManager.Instance.currentDay);
        //var dm = DialogueManager.Instance;
        //dm.StartCoroutine(dm.BackgroundTransitionEffect(dm.daySprite, 1f));
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
        //Debug.Log("GameManager回调结束游戏");
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

        StoryManager.Instance.currentDay = StoryManager.Instance.currentDay + 1;
        //Debug.Log("推进到第 " + StoryManager.Instance.currentDay + " 天");
        SceneManager.LoadScene("Clinic_Day");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
