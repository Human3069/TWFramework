using UnityEngine;

namespace _TW_Framework
{
    public abstract class BaseSingleton<T> : MonoBehaviour where T : BaseSingleton<T>
    {
        private const string LOG_FORMAT = "<color=white><b>[BaseSingleton]</b></color> {0}";

        protected static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
        }

        protected virtual void OnDestroy()
        {
          
        }
    }
}