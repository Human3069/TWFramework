using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace _TW_Framework.Internal
{
    // [CreateAssetMenu(fileName = "BindingKeysData", menuName = "Scriptable Objects/BindingKeysData")]
    public class KeyData : ScriptableObject
    {
        [SerializedDictionary("Name", "KeyCode")]
        public SerializedDictionary<KeyType, KeySetting> KeyInputDic = new SerializedDictionary<KeyType, KeySetting>();
    }

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

        internal event ValueChanged onValueChanged;
    }

    public delegate void ValueChanged(bool isOn);

    public static class KeySettingUtility
    {
        public static bool IsOn(this KeyType type)
        {
            SerializedDictionary<KeyType, KeySetting> dic = KeyInputManager.Instance._KeyData.KeyInputDic;
            if (dic.ContainsKey(type) == false)
            {
                Debug.LogError("Key doesn't exist!");
                return false;
            }

            return dic[type].IsInput;
        }

        public static async UniTask RegisterAsync(this KeyType type, ValueChanged _onValueChanged)
        {
            await UniTask.WaitUntil(() => KeyInputManager.Instance != null);

            SerializedDictionary<KeyType, KeySetting> dic = KeyInputManager.Instance._KeyData.KeyInputDic;
            if (dic.ContainsKey(type) == false)
            {
                Debug.LogError("Key doesn't exist!");
                return;
            }

            dic[type].onValueChanged += _onValueChanged;
        }
    }
}