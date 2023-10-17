using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPooledObject : MonoBehaviour
{
    void OnStockToPool()
    {
        Debug.Log("StockToPool");
    }

    void OnDestroy()
    {
        Debug.Log("Destroy");
    }
}
