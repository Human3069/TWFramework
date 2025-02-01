using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public enum OrderState
    {
        none,

        Defend,
        Attack,
    }

    public class EnemyFormationHandler : BaseFormationHandler
    {
        private const string LOG_FORMAT = "<color=white><b>[EnemyFormationHandler]</b></color> {0}";

        [SerializeField]
        protected OrderState _orderState;
        public OrderState _OrderState
        {
            get
            {
                return _orderState;
            }
            set
            {
                _orderState = value;

                switch (value)
                {
                    case OrderState.Defend:
                        DoAllDefend();
                        break;

                    case OrderState.Attack:
                        DoAllAttack();
                        break;

                    default:
                        throw new System.Exception("Unknown HandlerState: " + value);
                }
            }
        }

        public override void Initialize(UnitInfo[] unitInfos, Vector3[] startPoints, float unitDistance, float facingAngle)
        {
            for (int i = 0; i < unitInfos.Length; i++)
            {
                GameObject controllerObj = new GameObject("Controller_" + unitInfos[i]._UnitType);
                controllerObj.transform.SetParent(this.transform);

                EnemyFormationController controller = controllerObj.AddComponent<EnemyFormationController>();
                controller.Initialize(unitInfos[i], startPoints[i], unitDistance, i, facingAngle);

                allControllerList.Add(controller);
            }
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (_OrderState != OrderState.none)
            {
                _OrderState = _orderState;
            }
        }
#endif

        protected void DoAllDefend()
        {
            Debug.LogFormat(LOG_FORMAT, "DoDefend");

            foreach (EnemyFormationController controller in allControllerList)
            {
                controller.TargetController = null;
            }
        }

        protected void DoAllAttack()
        {
            Debug.LogFormat(LOG_FORMAT, "DoAttack");

            List<PlayerFormationController> playerControllerList = TWManager.Instance.Player.GetAllControllerList();

            foreach (EnemyFormationController controller in allControllerList)
            {
                PlayerFormationController nearestController = null;
                float minDistance = float.MaxValue;

                foreach (PlayerFormationController playerController in playerControllerList)
                {
                    float distance = Vector3.Distance(controller.GetMiddlePos(), playerController.GetMiddlePos());
                    Debug.Log("dis : " + distance + ", cont : " + playerController.gameObject.name);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestController = playerController;
                    }
                }

                controller.TargetController = nearestController;
            }
        }
    }
}