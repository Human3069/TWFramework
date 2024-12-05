using _TW_Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation
{
    public static class UnitFormationHelper
    {
        /// <summary>
        /// Applies offset to the Z axes on positions in order to move positions
        /// from pivot in front of formation, to pivot in center of the formation.
        /// </summary>
        /// <param name="positions">Current positions, method will update the reference values.</param>
        /// <param name="rowCount">Row count produced with formation.</param>
        /// <param name="rowSpacing">Spacing between each row.</param>
        public static void ApplyFormationCentering(ref List<Vector3> positions, float rowCount, float rowSpacing)
        {
            float offsetZ = Mathf.Max(0, (rowCount - 1) * rowSpacing / 2);

            for (int i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                pos.z += offsetZ;
                positions[i] = pos;
            }
        }

        /// <summary>
        /// Generates random "noise" for the position. In reality takes random
        /// range in the offset, does not use actual Math noise methods.
        /// </summary>
        /// <param name="factor">Factor for which the position can be offset.</param>
        /// <returns>Returns local offset for axes X and Z.</returns>
        public static Vector3 GetNoise(float factor)
        {
            return new Vector3(Random.Range(-factor, factor), 0, Random.Range(-factor, factor));
        }

        // ���� : ������ Formation ������ ä �̵��� ��, Formation ��Ʈ�� ä �̵��� �� ����
        // ���� : ��, ��, �ڷ� Formation ������ ä �̵��� ��, Formation ��Ʈ�� ä �̵��� �� �� ���� (�Ϻ� �ܿ����� �ֱ� ��)
        public static void SetPositions_ShortestByIndex_Asc(FormationController _controller, List<Vector3> posList, float facingAngle)
        {
            Dictionary<Vector3, UnitHandler> selectedNearestDic = new Dictionary<Vector3, UnitHandler>();

            for (int i = 0; i < _controller.UnitHandlerList.Count; i++)
            {
                float nearestSqrDistance = float.MaxValue;
                Vector3 nearestPos = Vector3.zero;

                for (int k = 0; k < posList.Count; k++)
                {
                    if (selectedNearestDic.ContainsKey(posList[k]) == false)
                    {
                        float sqrDistance = (posList[k] - _controller.UnitHandlerList[i].transform.position).sqrMagnitude;
                        if (sqrDistance < nearestSqrDistance)
                        {
                            nearestSqrDistance = sqrDistance;
                            nearestPos = posList[k];
                        }
                    }
                }

                selectedNearestDic.Add(nearestPos, _controller.UnitHandlerList[i]);
                _controller.UnitHandlerList[i].SetTargetDestination(nearestPos + UnitFormationHelper.GetNoise(_controller.NoiseAmount), facingAngle);
            }
        }

        // ���� : �ڷ� Formation ������ ä �̵��� ��, Formation ��Ʈ�� ä �̵��� �� ����
        // ���� : ��, ��, ��� Formation ������ ä �̵��� ��, Formation ��Ʈ�� ä �̵��� �� �� ���� (�Ϻ� �ܿ����� �ֱ� ��)
        public static void SetPositions_ShortestByIndex_Desc(FormationController _controller, List<Vector3> posList, float facingAngle)
        {
            Dictionary<Vector3, UnitHandler> selectedNearestDic = new Dictionary<Vector3, UnitHandler>();

            for (int i = _controller.UnitHandlerList.Count - 1; i >= 0; i--)
            {
                float nearestSqrDistance = float.MaxValue;
                Vector3 nearestPos = Vector3.zero;

                for (int k = 0; k < posList.Count; k++)
                {
                    if (selectedNearestDic.ContainsKey(posList[k]) == false)
                    {
                        float sqrDistance = (posList[k] - _controller.UnitHandlerList[i].transform.position).sqrMagnitude;
                        if (sqrDistance < nearestSqrDistance)
                        {
                            nearestSqrDistance = sqrDistance;
                            nearestPos = posList[k];
                        }
                    }
                }

                selectedNearestDic.Add(nearestPos, _controller.UnitHandlerList[i]);
                _controller.UnitHandlerList[i].SetTargetDestination(nearestPos + UnitFormationHelper.GetNoise(_controller.NoiseAmount), facingAngle);
            }
        }
    }
}