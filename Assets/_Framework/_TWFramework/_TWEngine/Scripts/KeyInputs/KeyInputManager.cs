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

    public class KeyInputManager : BaseSingleton<KeyInputManager>
    {
        public KeyData _KeyData;

        protected virtual void Update()
        {
            foreach (KeyValuePair<KeyType, KeySetting> pair in _KeyData.KeyInputDic)
            {
                pair.Value.IsInput = Input.GetKey(pair.Value._KeyCode);
            }
        }
    }
}