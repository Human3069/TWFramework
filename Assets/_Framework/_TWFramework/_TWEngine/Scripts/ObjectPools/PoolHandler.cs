using System.Collections.Generic;
using System;
using UnityEngine;

namespace _TW_Framework._Pool_Internal
{
    [Serializable]
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
}