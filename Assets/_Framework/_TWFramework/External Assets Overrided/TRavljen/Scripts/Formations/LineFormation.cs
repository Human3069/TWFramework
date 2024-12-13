using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public struct LineFormation : IFormation
    {
        private float spacing;

        public LineFormation(FormationController controller) : this(controller.UnitSpacing) {}

        public LineFormation(float spacing)
        {
            this.spacing = spacing;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> unitPositions = new List<Vector3>();

            float offset = (unitCount-1) * spacing / 2f;
            for (int index = 0; index < unitCount; index++)
            {
                unitPositions.Add(new Vector3(index * spacing - offset, 0, 0));
            }

            return unitPositions;
        }
    }
}
