using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
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
        Square = 2,
        Skirmish = 4,
        Triangle = 8,
        Cone = 16,

        Circle = 32,
        Line = 64,
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
        public GameObject UnitPrefab;
        public GameObject SilhouettePrefab;

        [Space(10)]
        [SerializedDictionary("Type", "Pair")]
        public SerializedDictionary<FormationType, FormationTypePair> FormationTypeDic = new SerializedDictionary<FormationType, FormationTypePair>();

        [Space(10)]
        public Sprite IconSprite;

        public FormationTypePair GetPairValue(FormationType type)
        {
            foreach (KeyValuePair<FormationType, FormationTypePair> pair in FormationTypeDic)
            {
                if (pair.Key.HasFlag(type) == true)
                {
                    return pair.Value;
                }
            }

            Debug.LogError("FormationTypeDic does not contain the key : " + type);
            return new FormationTypePair();
        }
    }

    [System.Serializable]
    public struct FormationTypePair
    {
        public FormationTypePair(float unitSpacing, float noiseAmount)
        {
            UnitSpacing = unitSpacing;
            NoiseAmount = noiseAmount;
        }

        public float UnitSpacing;
        public float NoiseAmount;
    }
}