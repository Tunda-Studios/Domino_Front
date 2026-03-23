using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoPool : MonoBehaviour
{
    public GameObject dominoPrefab;
    public Transform poolParent;

    private Queue<GameObject> pool = new Queue<GameObject>();
    // Start is called before the first frame update

    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(dominoPrefab, poolParent);
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
