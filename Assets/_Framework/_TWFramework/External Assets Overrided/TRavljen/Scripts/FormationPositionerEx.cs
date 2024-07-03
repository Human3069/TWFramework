using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TRavljen.UnitFormation.Formations;

namespace _TW_Framework
{
    public class FormationPositionerEx
    {
        public static List<Vector3> GetAlignedPositionList(int unitCount, IFormation formation, Vector3 targetPosition, float targetAngle)
        {
            List<Vector3> posList = formation.GetPositions(unitCount);
            Vector3 angleVector = new Vector3(0f, targetAngle, 0f);

            for (int i = 0; i < posList.Count; i++)
            {
                posList[i] = RotatePointAroundPivot(targetPosition + posList[i], targetPosition, angleVector);
            }

            return posList;
        }

        public static (List<Vector3> posList, float facingAngle) GetPositionListAndAngle(List<Vector3> currentPositions, IFormation formation, Vector3 targetPosition, float rotationThreshold = 4.0f)
        {
            if (currentPositions.Count == 0)
            {
                Debug.LogWarning("Cannot generate formation for an empty game object list.");
                return (new List<Vector3>(), 0f);
            }

            Vector3 sum = Vector3.zero;
            for (int i = 0; i < currentPositions.Count; i++)
            {
                sum += currentPositions[i];
            }

            Vector3 centerPos = sum / currentPositions.Count;
            Vector3 direction = targetPosition - centerPos;
            float angle = 0;

            if (direction.magnitude > rotationThreshold)
            {
                angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            }

            List<Vector3> formationPositions = GetAlignedPositionList(currentPositions.Count, formation, targetPosition, angle);

            return (formationPositions, angle);
        }

        private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }
    }
}
