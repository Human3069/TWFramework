using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace _TW_Framework.Internal
{
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

        public static async UniTask RegisterAsync(this KeyType type, Action<bool> _onValueChanged)
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