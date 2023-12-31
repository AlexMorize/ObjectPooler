using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections;

namespace SR.ObjectPooler
{
    public class ObjectPooler : MonoBehaviour
    {
        private Dictionary<string, PoolOfObject> poolers = new Dictionary<string, PoolOfObject>();
        private static ObjectPooler singleton
        {
            get
            {
                if (!instance)
                { 
                    GameObject obj = new GameObject("ObjectPooler");
                    instance = obj.AddComponent<ObjectPooler>();
                }
                return instance;
            }
        }
        private static ObjectPooler instance;
        //Do not EVER use this value
        
        #region Pool Management
        /// <summary>
        /// Populate a pool. Create the pool if not existing yet.
        /// </summary>
        /// <param name="obj">Prefab linked to the pool</param>
        /// <param name="amount">Amount of instance to populate the pool with</param>
        public static void PopulatePool(GameObject obj, int amount)
        {
            PoolOfObject currentPool = GetOrCreatePool(obj.name);
            for (int i = 0; i < amount; i++)
            {
                currentPool.StockObjectInPool(Instantiate(obj));
            }
        }

        //Create a new PoolOfObject and add it to the list 'Poolers'
        private static PoolOfObject CreatePool(string ID)
        {
            PoolOfObject currentPool = new PoolOfObject();
            singleton.poolers.Add(ID, currentPool);

#if UNITY_EDITOR //On editor ONLY, we want to store object of pools in a container (more readable)
            currentPool.CreateContainer(ID, singleton.transform);
#endif
            return currentPool;
        }

        //Get an existing pool or create one if not
        private static PoolOfObject GetOrCreatePool(string ID)
        {
            PoolOfObject currentPool;
            if (!singleton.poolers.TryGetValue(ID, out currentPool))
                currentPool = CreatePool(ID);

            return currentPool;
        }
        #endregion

        #region Instantiate
        /// <summary>
        /// Instantiate a GameObject by picking it in the corresponding Pool, or if not existing, create one.
        /// It also create a pool if not existing. DO NOT WORK WITH DONT DESTROY ON LOAD !
        /// </summary>
        /// <returns>The instantiate GameObject</returns>
        public static GameObject InstantiateFromPool(GameObject obj)
        {
            PoolOfObject currentPool = GetOrCreatePool(obj.name);

            if (currentPool.IsObjectInPool)
            {
                return currentPool.ReleaseObjectFromPool();
            }

            return Instantiate(obj);
        }

        /// <summary>
        /// Instantiate a GameObject by picking it in the corresponding Pool, or if not existing, create one.
        /// It also create a pool if not existing. DO NOT WORK WITH DONT DESTROY ON LOAD !
        /// </summary>
        /// <returns>The instantiate GameObject</returns>
        public static GameObject InstantiateFromPool(GameObject obj, Vector3 position, Quaternion rotation)
        {
            PoolOfObject currentPool = GetOrCreatePool(obj.name);

            if (currentPool.IsObjectInPool)
            {
                GameObject result = currentPool.ReleaseObjectFromPool();
                result.transform.position = position;
                result.transform.rotation = rotation;
                return result;
            }

            return Instantiate(obj, position, rotation);
        }
        #endregion

        #region Destroy

        /// <summary>
        /// Disable a gameobject and store it in the pool.  DO NOT WORK WITH DONT DESTROY ON LOAD !
        /// </summary>
        /// <param name="obj"></param>
        public static void StockToPool(GameObject obj)
        {
            string poolID = obj.name.Substring(0, obj.name.Length - 7);
            //When unity instantiate an object, the name change and add (Clone) to the end
            //Above line remove (Clone) at the end to get the PoolID
            if (singleton.poolers.TryGetValue(poolID, out PoolOfObject currentPool))
            {
                IStockToPoolHandler[] stockToPoolHandles = obj.GetComponents<IStockToPoolHandler>();
                for (int i = 0; i < stockToPoolHandles.Length; i++)
                    stockToPoolHandles[i].OnStockToPool();
                currentPool.StockObjectInPool(obj);
            }
            else
                throw new System.Exception($"You are trying to Destroy an object ({obj}) that was not created by the ObjectPooler");
        }

        public static async void StockToPool(GameObject obj, float duration)
        {
            await Task.Delay(Mathf.RoundToInt(duration * 1000));
            if(obj) //Checking if object still exist (Obj is destroy on changing scene)
                StockToPool(obj);
        }
        #endregion

        #region Coroutine
        /// <summary>
        /// Wait the end of frame to call OnReleaseFromPool
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        IEnumerator ReleaseFromPoolCoroutine(GameObject obj)
        {
            yield return new WaitForEndOfFrame();
            IReleaseFromPoolHandler[] releaseFromPoolHandles = obj.GetComponents<IReleaseFromPoolHandler>();
            for (int i = 0; i < releaseFromPoolHandles.Length; i++)
                releaseFromPoolHandles[i].OnReleaseFromPool();
        }
        #endregion

        private class PoolOfObject
        {
            private List<GameObject> inactiveObjects;
            private HashSet<GameObject> stockedHash;

#if UNITY_EDITOR //On editor ONLY, we want to store object of pools in a container (more readable)
            private Transform poolContainer;

            /// <summary>
            /// THIS METHOD ONLY WORK ON UNITY_EDITOR
            /// Create a container to store object from this pool
            /// </summary>
            /// <param name="name"></param>
            /// <param name="parent"></param>
            public void CreateContainer(string name, Transform parent)
            {
                poolContainer = new GameObject(name).transform;
                poolContainer.parent = parent;
            }
#endif

            public PoolOfObject()
            {
                inactiveObjects = new List<GameObject>();
                stockedHash = new HashSet<GameObject>();
            }

            public void StockObjectInPool(GameObject obj)
            {
                //Do not sotck an object if it is already stocked
                if (!stockedHash.Add(obj))
                    return;

#if UNITY_EDITOR//On editor ONLY, we want to store object of pools in a container (more readable)
                obj.transform.parent = poolContainer;
#else
                gameObject.transform.parent = null;
#endif
                obj.SetActive(false);
                inactiveObjects.Add(obj);
            }

            public GameObject ReleaseObjectFromPool()
            {
                GameObject result = inactiveObjects[inactiveObjects.Count - 1];
                // Récupération du dernier éléments plutot que du premier
                // List.RemoveAt(0) est moins performant que List.RemoveAt(LeDernier)
                inactiveObjects.RemoveAt(inactiveObjects.Count - 1);//Remove from list
                stockedHash.Remove(result);//Remove from hashset, this hashet is only used to check if elements is already present in the list in StockObjectInPool()
                result.SetActive(true);

                //Call OnReleaseFromPool, with a one frame delay
                singleton.StartCoroutine(singleton.ReleaseFromPoolCoroutine(result));
#if UNITY_EDITOR
                result.transform.parent = null;
                //On build this operation is made in StockObjectInPool()
                //We apply it here, on Editor, to get a similar result
#endif
                return result;
            }

            public bool PoolContains(GameObject obj)
            {
                return stockedHash.Contains(obj);
            }

            public bool IsObjectInPool => inactiveObjects.Count > 0;
        }
    }
}
