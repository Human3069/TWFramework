using Unity.VisualScripting;
using UnityEngine;

namespace _TW_Framework
{
    [CreateAssetMenu(fileName = "InitialInfo", menuName = "Scriptable Objects/InitialInfo")]
    public class InitialInfo : ScriptableObject
    {
        public string SceneName;

        [Header("=== Player Unit ===")]
        public UnitInfo[] PlayerUnitInfos;
        [SerializeField]
        private Vector3 playerUnitInfoPoint;
        [SerializeField]
        private float playerFacingAngle;

        [Header("=== Enemy Unit ===")]
        public UnitInfo[] EnemyUnitInfos;
        [SerializeField]
        private Vector3 enemyUnitInfoPoint;
        [SerializeField]
        private float enemyFacingAngle;

        [Space(10)]
        [SerializeField]
        private int maxUnitInfoCount = 20;
        [SerializeField]
        private float distancePerUnitInfo = 20f;
        public float UnitDistance;

        public void DrawGizmos()
        {
            (Vector3[] playerPoints, Vector3[] enemyPoints) = GetPoints();

            Gizmos.color = new Color(0f, 1f, 0f, 1f);
            foreach (Vector3 playerPoint in playerPoints)
            {
                Gizmos.DrawSphere(playerPoint, 2f);
            }

            Gizmos.color = new Color(1f, 0f, 0f, 1f);
            foreach (Vector3 enemyPoint in enemyPoints)
            {
                Gizmos.DrawSphere(enemyPoint, 2f);
            }
        }

        public (Vector3[], Vector3[]) GetPoints()
        {
            Vector3[] playerPoints = new Vector3[PlayerUnitInfos.Length];
            Vector3[] enemyPoints = new Vector3[EnemyUnitInfos.Length];

            for (int i = 0; i < PlayerUnitInfos.Length; i++)
            {
                float pointNormal = i - ((float)PlayerUnitInfos.Length / 2f) + 0.5f;
                float pointDistance = distancePerUnitInfo * pointNormal;

                playerPoints[i] = new Vector3(pointDistance, 0f, 0f) + playerUnitInfoPoint;
            }

            for (int i = 0; i < EnemyUnitInfos.Length; i++)
            {
                float pointNormal = i - ((float)EnemyUnitInfos.Length / 2f) + 0.5f;
                float pointDistance = distancePerUnitInfo * pointNormal;

                enemyPoints[i] = new Vector3(pointDistance, 0f, 0f) + enemyUnitInfoPoint;
            }

            return (playerPoints, enemyPoints);
        }

        public (float, float) GetFacingAngles()
        {
            return (playerFacingAngle, enemyFacingAngle);
        }
    }
}