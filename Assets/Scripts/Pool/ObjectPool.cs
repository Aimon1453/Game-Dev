using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ObjectPool : Singleton<ObjectPool>
{
    private Dictionary<string, Queue<GameObject>> objectPool = new Dictionary<string, Queue<GameObject>>();
    private GameObject pool;

    public GameObject GetObject(GameObject prefab)
    {
        GameObject _object;

        if (!objectPool.ContainsKey(prefab.name) || objectPool[prefab.name].Count == 0)
        {
            _object = Instantiate(prefab);
            PushObject(_object);
            if (pool == null)
                pool = new GameObject("ObjectPool");
            GameObject child = GameObject.Find(prefab.name);
            if (!child)
            {
                child = new GameObject(prefab.name);
                child.transform.SetParent(pool.transform);
            }
            _object.transform.SetParent(child.transform);
        }
        _object = objectPool[prefab.name].Dequeue();
        _object.SetActive(true);
        return _object;
    }

    //对生成的预制体指定位置和旋转
    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        if (!objectPool.ContainsKey(prefab.name) || objectPool[prefab.name].Count == 0)
        {
            obj = Instantiate(prefab);
            PushObject(obj);
            if (pool == null)
                pool = new GameObject("ObjectPool");
            GameObject child = GameObject.Find(prefab.name);
            if (!child)
            {
                child = new GameObject(prefab.name);
                child.transform.SetParent(pool.transform);
            }
            obj.transform.SetParent(child.transform);
        }
        obj = objectPool[prefab.name].Dequeue();

        obj.transform.position = position;
        obj.transform.rotation = rotation;

        // 再激活
        obj.SetActive(true);

        return obj;
    }

    public void PushObject(GameObject prefab)
    {

        string _name = prefab.name.Replace("(Clone)", string.Empty);
        if (!objectPool.ContainsKey(_name))
            objectPool.Add(_name, new Queue<GameObject>());
        objectPool[_name].Enqueue(prefab);
        prefab.SetActive(false);
    }

    public void ClearPool()
    {
        foreach (var queue in objectPool.Values)
        {
            foreach (var obj in queue)
            {
                Destroy(obj);
            }
        }
        objectPool.Clear();
    }
}
