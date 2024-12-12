using _TW_Framework.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public enum KeyType
    {
        Move_Forward,
        Move_Backward,
        Move_Left,
        Move_Right,

        Turn_Left,
        Turn_Right,

        Accelerate_Move,
    }

    public class KeyInputManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[KeyInputManager]</b></color> {0}";

        protected static KeyInputManager _instance;
        public static KeyInputManager Instance
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

        public KeyData _KeyData;

        protected virtual void Awake()
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
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }
             
            Instance = null;
        }

        protected virtual void Update()
        {
            foreach (KeyValuePair<KeyType, KeySetting> pair in _KeyData.KeyInputDic)
            {
                pair.Value.IsInput = Input.GetKey(pair.Value._KeyCode);
            }
        }
    }
}