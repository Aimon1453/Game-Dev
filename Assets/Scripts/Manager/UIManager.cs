using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public TextMeshProUGUI mineralsText;
    public TextMeshProUGUI healthText;

    #region 局内矿物数显示
    public void RegisterMineralsText(TextMeshProUGUI textComponent)
    {
        mineralsText = textComponent;
    }

    public void UnregisterMineralsText()
    {
        mineralsText = null;
    }

    //调用它来更新矿物数量的显示
    public void UpdateMineralsText(int amount)
    {
        if (mineralsText != null)
        {
            mineralsText.text = "Minerals: " + amount;
        }
    }
    #endregion

    #region 局内血量显示
    public void RegisterHealthText(TextMeshProUGUI textComponent)
    {
        healthText = textComponent;
    }

    public void UnregisterHealthText()
    {
        healthText = null;
    }

    public void UpdateHealthText(float amount)
    {
        if (healthText != null)
        {
            //来向下取整
            int healthAsInt = Mathf.FloorToInt(amount);
            healthText.text = "Health: " + healthAsInt.ToString();
        }
    }
    #endregion
}
