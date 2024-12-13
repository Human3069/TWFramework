using System;
using UnityEngine;

namespace _TW_Framework
{
    public static class ObjectPoolUtility
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