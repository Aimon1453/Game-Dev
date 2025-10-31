using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public TextMeshProUGUI mineralsCounterText;
    public TextMeshProUGUI healthCounterText;
    private GameUIConnector healthBarConnector;
    private PlayerShip playerShip;

    // 注册血条连接器
    public void RegisterHealthBar(GameUIConnector connector)
    {
        healthBarConnector = connector;

        // 如果已经有玩家血量数据，立即更新血条
        if (playerShip != null)
        {
            float healthPercent = playerShip.GetCurrentHealth() / playerShip.maxHealth;
            connector.UpdateHealthBar(healthPercent);
        }
    }

    // 注销血条连接器
    public void UnregisterHealthBar()
    {
        healthBarConnector = null;
    }

    // 获取玩家当前血量百分比
    public float GetPlayerHealthPercent()
    {
        if (playerShip != null)
        {
            return playerShip.GetCurrentHealth() / playerShip.maxHealth;
        }
        return 1f; // 默认返回满血
    }

    public void RegisterPlayerShip(PlayerShip ship)
    {
        playerShip = ship;
    }

    // 更新血量文本
    public void UpdateHealthText(float health)
    {
        if (healthCounterText != null)
        {
            healthCounterText.text = Mathf.Ceil(health).ToString();
        }

        // 同时更新血条
        if (healthBarConnector != null && playerShip != null)
        {
            float healthPercent = health / playerShip.maxHealth;
            healthBarConnector.UpdateHealthBar(healthPercent);
        }
    }

    #region 局内矿物数显示
    public void RegisterMineralsText(TextMeshProUGUI textComponent)
    {
        mineralsCounterText = textComponent;
    }

    public void UnregisterMineralsText()
    {
        mineralsCounterText = null;
    }

    //调用它来更新矿物数量的显示
    public void UpdateMineralsText(int amount)
    {
        if (mineralsCounterText != null)
        {
            mineralsCounterText.text = "Minerals: " + amount;
        }
    }
    #endregion

    #region 局内血量显示
    public void RegisterHealthText(TextMeshProUGUI textComponent)
    {
        healthCounterText = textComponent;
    }

    public void UnregisterHealthText()
    {
        healthCounterText = null;
    }
    #endregion
}
