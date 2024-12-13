using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public interface IFormation
    {
        List<Vector3> GetPositions(int unitCount);
    }
}