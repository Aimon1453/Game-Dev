using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get // 如果实例不存在，就创建一个新的实例
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();

                if (_instance == null)
                {
                    // 自动从Resources加载
                    GameObject prefab = Resources.Load<GameObject>(typeof(T).Name);
                    if (prefab != null)
                    {
                        GameObject obj = Instantiate(prefab);
                        obj.name = typeof(T).Name;
                        _instance = obj.GetComponent<T>();
                    }
                    else
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        _instance = obj.AddComponent<T>();
                    }
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        //如果有实例了，并且不是我，就销毁我自己
        if (_instance != null && _instance != this as T)
        {
            Destroy(gameObject);
        }
        else
        {
            //把自己设为这个单例
            _instance = this as T;

            DontDestroyOnLoad(transform.root.gameObject);
        }
    }
}
