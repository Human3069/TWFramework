using UnityEngine;

namespace _TW_Framework
{
    public class TWManager : BaseSingleton<TWManager>
    {
        [SerializeField]
        protected InitialInfo _InitialInfo;

        [Space(10)]
        public PlayerFormationHandler Player;
        public EnemyFormationHandler Enemy;

        protected override void Awake()
        {
            base.Awake();

            (Vector3[] playerPoints, Vector3[] enemyPoints) = _InitialInfo.GetPoints();
            (float playerFacingAngle, float enemyFacingAngle) = _InitialInfo.GetFacingAngles();

            Player.Initialize(_InitialInfo.PlayerUnitInfos, playerPoints, _InitialInfo.UnitDistance, playerFacingAngle);
            Enemy.Initialize(_InitialInfo.EnemyUnitInfos, enemyPoints, _InitialInfo.UnitDistance, enemyFacingAngle);
        }

        protected void OnDrawGizmos()
        {
            _InitialInfo.DrawGizmos();
        }
    }
}