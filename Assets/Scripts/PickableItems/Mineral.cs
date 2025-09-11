using UnityEngine;

public class Mineral : MonoBehaviour, IPickupable
{
    [SerializeField] private int value = 1;

    public void OnPickup()
    {
        // 通知GameManager增加矿物数量
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMineral(value);
        }
        else
        {
            Debug.LogWarning("GameManager instance is null!");
        }

        ObjectPool.Instance.PushObject(gameObject);
    }
}
