using _TW_Framework._Pool_Internal;
using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public enum PoolerType
    {
        MusketBullet,
    }

    public class ObjectPoolManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[ObjectPoolManager]</b></color> {0}";

        protected static ObjectPoolManager _instance;
        public static ObjectPoolManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [SerializeField]
        [SerializedDictionary("PoolerType", "PoolHandler")]
        protected SerializedDictionary<PoolerType, PoolHandler> _poolHandlers;

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this.gameObject);
                return;
            }

            foreach (KeyValuePair<PoolerType, PoolHandler> pair in _poolHandlers)
            {
                GameObject poolHandlerParent = new GameObject("PoolHandler_" + pair.Key);
                poolHandlerParent.transform.SetParent(this.transform);

                pair.Value.Initialize(poolHandlerParent.transform);
            }
        }

        protected void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        public PoolHandler GetPoolHandler(PoolerType type)
        {
            if (_poolHandlers.ContainsKey(type) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "PoolHandler not found for type : " + type);
                return null;
            }

            return _poolHandlers[type];
        }
    }
}

namespace _TW_Framework._Pool_Internal
{
    [System.Serializable]
    public class PoolHandler
    {
        [SerializeField]
        protected GameObject prefab;
        [SerializeField]
        protected int initCount = 10;

        private Queue<GameObject> poolQueue = new Queue<GameObject>();

        private Transform allocatedT;

        internal void Initialize(Transform parentT)
        {
            allocatedT = parentT;

            for (int i = 0; i < initCount; i++)
            {
                GameObject obj = GameObject.Instantiate(prefab, allocatedT);
                obj.SetActive(false);

                poolQueue.Enqueue(obj);
            }
        }

        internal GameObject EnableObj(Action<GameObject> beforeEnableAction = null)
        {
            if (poolQueue.TryDequeue(out GameObject obj) == false)
            {
                obj = GameObject.Instantiate(prefab, allocatedT);
            }

            beforeEnableAction?.Invoke(obj);
            obj.SetActive(true);

            return obj;
        }

        internal T EnableObj<T>(Action<T> beforeEnableAction = null) where T : MonoBehaviour
        {
            if (poolQueue.TryDequeue(out GameObject obj) == false)
            {
                obj = GameObject.Instantiate(prefab, allocatedT);
            }

            T component = obj.GetComponent<T>();

            beforeEnableAction?.Invoke(component);
            obj.SetActive(true);

            return component;
        }

        internal void DisableObj(GameObject obj)
        {
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }

    public static class PoolHelper
    {
        public static GameObject EnablePool(this PoolerType type, Action<GameObject> beforeEnableAction = null)
        {
            GameObject pooledObj = ObjectPoolManager.Instance.GetPoolHandler(type).EnableObj(beforeEnableAction);
            return pooledObj;
        }

        public static T EnablePool<T>(this PoolerType type, Action<T> beforeEnableAction = null) where T : MonoBehaviour
        {
            T pooledComponent = ObjectPoolManager.Instance.GetPoolHandler(type).EnableObj(beforeEnableAction);
            return pooledComponent;
        }

        public static void ReturnPool(this GameObject obj, PoolerType type)
        {
            ObjectPoolManager.Instance.GetPoolHandler(type).DisableObj(obj);
        }
    }
}