using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SR.ObjectPooler;

public class PoolTester : MonoBehaviour
{
    [SerializeField] private GameObject obj;

    [SerializeField] private bool instantiateOne;    
    [SerializeField] private bool destroyLast;    
    [SerializeField] private bool destroyLastDelay;    
    [SerializeField] private int population = 1;    
    [SerializeField] private bool populate;

    private GameObject Last;

    private void InstOne()
    {
        if (!instantiateOne) return;
        instantiateOne = false;
        Last = ObjectPooler.InstantiateFromPool(obj);
    }
    private void DestroyLast()
    {
        if (!destroyLast) return;
        destroyLast = false;
        ObjectPooler.StockToPool(Last);
    }

    private void DestroyLastDelay()
    {
        if (!destroyLastDelay) return;
        destroyLastDelay = false;
        ObjectPooler.StockToPool(Last,2);
    }

    private void Populate()
    {
        if (!populate) return;
        populate = false;
        ObjectPooler.PopulatePool(obj,population);
    }

    private void FixedUpdate()
    {
        Populate();
        DestroyLast();
        DestroyLastDelay();
        InstOne();
    }
}
