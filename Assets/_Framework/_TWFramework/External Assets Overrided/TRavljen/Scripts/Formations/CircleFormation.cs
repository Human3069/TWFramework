using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    struct CircleFormation : IFormation
    {
        private float spacing;
        private float circleAngle;

        public CircleFormation(FormationController controller) : this(controller.UnitSpacing) { }

        public CircleFormation(float spacing, float circleAngle = 360f)
        {
            this.spacing = spacing;
            this.circleAngle = Mathf.Clamp(circleAngle, 0f, 360f);
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            if (unitCount <= 1)
            {
                return new List<Vector3>() { Vector3.zero };
            }

            List<Vector3> unitPositions = new List<Vector3>();
            float x, y;
            float angle = 0f;

            var angleIncrement = circleAngle / unitCount;
            var a = angleIncrement / 2;
            var radius = (spacing / 2) / Mathf.Sin(a * Mathf.Deg2Rad);

            for (int i = 0; i < unitCount; i++)
            {
                x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                y = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

                unitPositions.Add(new Vector3(x, 0, y));

                angle += angleIncrement;
            }

            return unitPositions;
        }
    }
}