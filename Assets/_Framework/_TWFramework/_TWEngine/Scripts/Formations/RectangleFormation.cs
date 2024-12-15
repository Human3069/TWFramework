using System.Collections;
using System.Collections.Generic;
using TRavljen.UnitFormation;
using UnityEngine;

namespace _TW_Framework
{
    public struct RectangleFormation : IFormation
    {
        public int ColumnCount { get; private set; }

        private float spacing;
        private bool centerUnits;
        private bool pivotInMiddle;

        public RectangleFormation(BaseFormationController controller) : this(controller.UnitsPerRow, controller.UnitSpacing, true, controller.IsPivotInMiddle) { }

        public RectangleFormation(int columnCount, float spacing, bool centerUnits = true, bool pivotInMiddle = false)
        {
            this.ColumnCount = columnCount;
            this.spacing = spacing;
            this.centerUnits = centerUnits;
            this.pivotInMiddle = pivotInMiddle;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> unitPositions = new List<Vector3>();
            int unitsPerRow = Mathf.Min(ColumnCount, unitCount);
            float offsetX = (unitsPerRow - 1) * spacing / 2f;

            if (unitsPerRow == 0)
            {
                return new List<Vector3>();
            }

            float rowCount = unitCount / ColumnCount + (unitCount % ColumnCount > 0 ? 1 : 0);
            float x, y, column;
            int firstIndexInRow;

            for (int row = 0; unitPositions.Count < unitCount; row++)
            {
                firstIndexInRow = row * ColumnCount;
                if (centerUnits && row != 0 && firstIndexInRow + ColumnCount > unitCount)
                {
                    int emptySlots = firstIndexInRow + ColumnCount - unitCount;
                    offsetX -= emptySlots / 2f * spacing;
                }

                for (column = 0; column < ColumnCount; column++)
                {
                    if (firstIndexInRow + column < unitCount)
                    {
                        x = column * spacing - offsetX;
                        y = row * spacing;

                        Vector3 newPosition = new Vector3(x, 0, -y);
                        unitPositions.Add(newPosition);
                    }
                    else
                    {
                        if (pivotInMiddle == true)
                        {
                            UnitFormationHelper.ApplyFormationCentering(ref unitPositions, rowCount, spacing);
                        }

                        return unitPositions;
                    }
                }
            }

            if (pivotInMiddle == true)
            {
                UnitFormationHelper.ApplyFormationCentering(ref unitPositions, rowCount, spacing);
            }

            return unitPositions;
        }
    }
}