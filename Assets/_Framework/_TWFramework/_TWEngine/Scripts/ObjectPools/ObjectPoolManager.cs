using _TW_Framework._Pool_Internal;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public enum PoolerType
    {
        MusketBullet,
        
        Hit_Dirt,
    }

    public class ObjectPoolManager : BaseSingleton<ObjectPoolManager>
    {
        private const string LOG_FORMAT = "<color=white><b>[ObjectPoolManager]</b></color> {0}";

        [SerializeField]
        [SerializedDictionary("PoolerType", "PoolHandler")]
        protected SerializedDictionary<PoolerType, PoolHandler> _poolHandlers;

        protected override void Awake()
        {
            base.Awake();

            foreach (KeyValuePair<PoolerType, PoolHandler> pair in _poolHandlers)
            {
                GameObject poolHandlerParent = new GameObject("PoolHandler_" + pair.Key);
                poolHandlerParent.transform.SetParent(this.transform);

                pair.Value.Initialize(poolHandlerParent.transform);
            }
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