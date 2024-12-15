using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public class EnemyFormationHandler : BaseFormationHandler
    {
        public override void Initialize(UnitInfo[] unitInfos, Vector3[] startPoints, float unitDistance, float facingAngle)
        {
            for (int i = 0; i < unitInfos.Length; i++)
            {
                GameObject controllerObj = new GameObject("Controller_" + unitInfos[i]._UnitType);
                controllerObj.transform.SetParent(this.transform);

                EnemyFormationController controller = controllerObj.AddComponent<EnemyFormationController>();
                controller.Initialize(unitInfos[i], startPoints[i], unitDistance, i, facingAngle);

                allControllerList.Add(controller);
            }
        }
    }
}