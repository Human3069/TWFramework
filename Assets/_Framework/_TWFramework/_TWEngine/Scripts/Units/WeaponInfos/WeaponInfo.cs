using UnityEngine;

namespace _TW_Framework
{
    [CreateAssetMenu(fileName = "WeaponInfo", menuName = "Scriptable Objects/WeaponInfo")]
    public class WeaponInfo : ScriptableObject
    {
        public PoolerType _PoolerType;

        [Space(10)]
        public float Accuracy;
        public float Damage;
        public float Range;
        public float ReloadTime;
        public int MaxAmmo;
    }
}