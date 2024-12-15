using System;
using UnityEngine;

namespace _TW_Framework
{
    public enum UnitType
    {
        General,
        Cavalry,
        Artillery,
        Skirmisher,
        Regular,
        Conscript,
    }

    [Flags]
    public enum FormationType
    {
        Rectangle = 1,
        Skirmish = 2,
        Triangle = 4,
        Cone = 8
    }

    [CreateAssetMenu(fileName = "UnitInfo", menuName = "Scriptable Objects/UnitInfo")]
    public class UnitInfo : ScriptableObject
    {
        public UnitType _UnitType;

        [Header("=== Damageable Infos ===")]
        public int MaxHealth;

        [Space(10)]
        public float MeleeDamage;
        public float MeleeSpeed;
        public float MeleeRange;

        [Space(10)]
        public WeaponInfo _WeaponInfo;

        [Header("=== Formation Infos ===")]
        public FormationType _FormationType;
        public int UnitCount;
        public float UnitSpacing;
        public float NoiseAmount;
        public GameObject UnitPrefab;
        public GameObject SilhouettePrefab;
    }
}