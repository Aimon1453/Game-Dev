

using UnityEngine;

public class CanvasRoot : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}