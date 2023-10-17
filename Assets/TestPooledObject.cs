using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SR.ObjectPooler;

public class TestPooledObject : MonoBehaviour, IReleaseFromPoolHandler, IStockToPoolHandler
{
    public void OnStockToPool()
    {
        Debug.Log("StockToPool");
    }

    void OnDestroy()
    {
        Debug.Log("Destroy");
    }

    public void OnReleaseFromPool()
    {
        Debug.Log("ReleaseFromPool");
    }
}
