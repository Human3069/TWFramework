using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public abstract class BaseFormationHandler : MonoBehaviour
    {
        [Header("=== BaseFormationHandler ===")]
        [SerializeField]
        protected List<BaseFormationController> allControllerList = new List<BaseFormationController>();

        public abstract void Initialize(UnitInfo[] unitInfos, Vector3[] startPoints, float unitDistance, float facingAngle);

        public virtual void OnAllControllerUnitsDead(int index)
        {
            allControllerList.RemoveAt(index);

            for (int i = 0; i < allControllerList.Count; i++)
            {
                allControllerList[i].ResetSelectedIndex(i);
            }
        }
    }
}