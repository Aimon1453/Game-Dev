using UnityEngine;
using TMPro;

//用于连接UIManager
public class GameUIConnector : MonoBehaviour
{
    public TextMeshProUGUI mineralsCounterText;
    public TextMeshProUGUI healthCounterText;

    void Awake()
    {
        //注册矿物数显示文本
        if (UIManager.Instance != null && mineralsCounterText != null)
        {
            UIManager.Instance.RegisterMineralsText(mineralsCounterText);
        }
        //注册血量显示文本
        if (UIManager.Instance != null && healthCounterText != null)
        {
            UIManager.Instance.RegisterHealthText(healthCounterText);
        }
    }

    void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UnregisterMineralsText();
            UIManager.Instance.UnregisterHealthText();
        }
    }
}
