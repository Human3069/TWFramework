using System;
using UnityEngine;

namespace _TW_Framework.Internal
{
    [System.Serializable]
    public class KeySetting
    {
        public KeyCode _KeyCode;

        [ReadOnly]
        [SerializeField]
        private bool _isInput;
        public bool IsInput
        {
            get
            {
                return _isInput;
            }
            internal set
            {
                if (_isInput != value)
                {
                    _isInput = value;
                    onValueChanged?.Invoke(value);
                }
            }
        }

        internal Action<bool> onValueChanged;
    }
}