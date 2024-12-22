using System;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public class PlayerFormationHandler : BaseFormationHandler
    {
        private const string LOG_FORMAT = "<color=white><b>[PlayerFormationHandler]</b></color> {0}";

        [Header("=== PlayerFormationHandler ===")]
        public MouseEventHandler MouseEventHandler;
        [SerializeField]
        protected UI_FormationController uiController;

        [ReadOnly]
        [SerializeField]
        protected List<PlayerFormationController> selectedControllerList = new List<PlayerFormationController>();

        public Action<Dictionary<int, bool>> OnSelectStateChanged;

        protected virtual void Awake()
        {
            MouseEventHandler.OnEndMouseSelect += OnEndMouseSelect;
        }

        protected virtual void OnDestroy()
        {
            MouseEventHandler.OnEndMouseSelect -= OnEndMouseSelect;
        }

        public override void Initialize(UnitInfo[] unitInfos, Vector3[] startPoints, float unitDistance, float facingAngle)
        {
            for (int i = 0; i < unitInfos.Length; i++)
            {
                GameObject controllerObj = new GameObject("Controller_" + unitInfos[i]._UnitType);
                controllerObj.transform.SetParent(this.transform);

                PlayerFormationController controller = controllerObj.AddComponent<PlayerFormationController>();
                controller.Initialize(unitInfos[i], startPoints[i], unitDistance, i, facingAngle, MouseEventHandler);

                allControllerList.Add(controller);
            }

            uiController.Initialize(unitInfos, allControllerList);
        }

        public override void OnAllControllerUnitsDead(int index)
        {
            selectedControllerList.Remove(allControllerList[index] as PlayerFormationController);

            base.OnAllControllerUnitsDead(index);
            uiController.OnControllerDead(index);
        }

        public List<PlayerFormationController> GetAllControllerList()
        {
            return allControllerList.ConvertAll(x => x as PlayerFormationController);
        }

        public List<PlayerFormationController> GetSelectedControllerList()
        {
            return selectedControllerList;
        }

        protected void OnEndMouseSelect(List<PlayerFormationController> controllerList)
        {
            List<PlayerFormationController> deselectedControllerList = new List<PlayerFormationController>(allControllerList.ConvertAll(x => x as PlayerFormationController));
            deselectedControllerList.RemoveAll(x => controllerList.Contains(x));
            
            controllerList.ForEach(SelectController);
            deselectedControllerList.ForEach(DeselectController);
            controllerList.ForEach(SelectStateChanged);

            Dictionary<int, bool> hasSelectedDic = new Dictionary<int, bool>();
            foreach (PlayerFormationController controller in allControllerList)
            {
                hasSelectedDic.Add(controller.SelectedIndex, controllerList.Contains(controller) == true);
            }

            OnSelectStateChanged?.Invoke(hasSelectedDic);
        }

        public void SelectController(int index)
        {
            SelectController(allControllerList[index] as PlayerFormationController);
        }

        public void SelectController(PlayerFormationController controller)
        {
            if (selectedControllerList.Contains(controller) == false)
            {
                selectedControllerList.Add(controller);
                controller.OnSelected();
            }
        }

        public void DeselectController(int index)
        {
            DeselectController(allControllerList[index] as PlayerFormationController);
        }

        public void DeselectController(PlayerFormationController controller)
        {
            if (selectedControllerList.Contains(controller) == true)
            {
                selectedControllerList.Remove(controller);
                controller.OnDeselected();
            }
        }

        public void SelectStateChanged(int index)
        {
            SelectStateChanged(allControllerList[index] as PlayerFormationController);
        }

        public void SelectStateChanged(PlayerFormationController controller)
        {
            controller.OnSelectionStateChanged(selectedControllerList.Count, selectedControllerList.IndexOf(controller));
        }
    }
}