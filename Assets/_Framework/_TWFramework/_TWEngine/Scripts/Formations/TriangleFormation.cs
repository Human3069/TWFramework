using System.Collections;
using System.Collections.Generic;
using TRavljen.UnitFormation;
using UnityEngine;

namespace _TW_Framework
{
    public struct TriangleFormation : IFormation
    {
        private float spacing;
        private bool centerUnits;
        private bool pivotInMiddle;

        public TriangleFormation(BaseFormationController controller) : this(controller.UnitSpacing, true, controller.IsPivotInMiddle) { }

        public TriangleFormation(float spacing, bool centerUnits = true, bool pivotInMiddle = false)
        {
            this.spacing = spacing;
            this.centerUnits = centerUnits;
            this.pivotInMiddle = pivotInMiddle;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> unitPositions = new List<Vector3>();

            float currentRowOffset = 0f;
            float x, z;
            int row;

            for (row = 0;  unitPositions.Count < unitCount; row++)
            {
                int columnsInRow = row + 1;
                int firstIndexInRow = unitPositions.Count;

                for (int column = 0; column < columnsInRow; column++)
                {
                    x = column * spacing + currentRowOffset;
                    z = row * spacing;

                    if (centerUnits && row != 0 && firstIndexInRow + columnsInRow > unitCount)
                    {
                        int emptySlots = firstIndexInRow + columnsInRow - unitCount;
                        x += emptySlots / 2f * spacing;
                    }

                    unitPositions.Add(new Vector3(x, 0, -z));

                    if (unitPositions.Count >= unitCount)
                    {
                        break;
                    }
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
