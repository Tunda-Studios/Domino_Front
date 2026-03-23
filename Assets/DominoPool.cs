using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoPool : MonoBehaviour
{
    public GameObject dominoPrefab;
    public Transform poolParent;

    public int initialSize = 30;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        // Prewarm pool
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(dominoPrefab, poolParent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get(Transform parent)
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(dominoPrefab);
        }

        obj.SetActive(true);
        obj.transform.SetParent(parent, false);

        // Reset transform 
        var rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = Vector2.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
        }

        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(poolParent, false);
        pool.Enqueue(obj);
    }
}
