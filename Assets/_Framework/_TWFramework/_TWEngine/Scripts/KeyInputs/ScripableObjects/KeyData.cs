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
}