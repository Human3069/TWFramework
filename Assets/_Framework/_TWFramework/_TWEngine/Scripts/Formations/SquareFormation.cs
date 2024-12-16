using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public struct SquareFormation : IFormation
    {
        private float spacing;

        public SquareFormation(BaseFormationController controller) : this(controller.UnitSpacing) { }

        public SquareFormation(float spacing)
        {
            this.spacing = spacing;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> unitPositions = new List<Vector3>();

            int sideLength = Mathf.CeilToInt(Mathf.Sqrt(unitCount + 9)); // +9 to account for the empty center
            float offset = (sideLength - 1) * spacing / 2;

            for (int i = 0; i < unitCount + 9; i++)
            {
                int row = i / sideLength;
                int column = i % sideLength;

                // Skip the center 3x3 area
                if (row >= (sideLength / 2) - 1 && row <= (sideLength / 2) + 1 &&
                    column >= (sideLength / 2) - 1 && column <= (sideLength / 2) + 1)
                {
                    continue;
                }

                if (unitPositions.Count >= unitCount)
                {
                    break;
                }

                float x = column * spacing - offset;
                float z = row * spacing - offset;

                unitPositions.Add(new Vector3(x, 0, -z));
            }

            return unitPositions;
        }
    }
}
