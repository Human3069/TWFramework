using System.Collections.Generic;
using TRavljen.UnitFormation;
using UnityEngine;

namespace _TW_Framework
{
    public struct ConeFormation : IFormation
    {
        private float spacing;
        private bool pivotInMiddle;

        public ConeFormation(BaseFormationController controller) : this(controller.UnitSpacing, controller.IsPivotInMiddle) { }

        public ConeFormation(float spacing, bool pivotInMiddle = true)
        {
            this.spacing = spacing;
            this.pivotInMiddle = pivotInMiddle;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> unitPositions = new List<Vector3>();

            float currentRowOffset = 0f;
            float x, z;
            int columnsInRow;
            int row;

            for (row = 0; unitPositions.Count < unitCount; row++)
            {
                columnsInRow = row + 1;

                x = 0 * spacing + currentRowOffset;
                z = row * spacing;

                unitPositions.Add(new Vector3(x, 0, -z));

                if (unitPositions.Count < unitCount && columnsInRow > 1)
                {
                    x = (columnsInRow - 1) * spacing + currentRowOffset;
                    z = row * spacing;

                    unitPositions.Add(new Vector3(x, 0, -z));
                }

                currentRowOffset -= spacing / 2;
            }

            if (pivotInMiddle == true)
            {
                UnitFormationHelper.ApplyFormationCentering(ref unitPositions, row, spacing);
            }

            return unitPositions;
        }

    }
}