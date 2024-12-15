using _KMH_Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public class PlayerFormationHandler : BaseFormationHandler
    {
        private const string LOG_FORMAT = "<color=white><b>[PlayerFormationHandler]</b></color> {0}";

        [Header("=== PlayerFormationHandler ===")]
        [SerializeField]
        protected MouseEventHandler mouseEventHandler;
        [SerializeField]
        protected UI_FormationController uiController;

        [ReadOnly]
        [SerializeField]
        protected List<PlayerFormationController> selectedControllerList = new List<PlayerFormationController>();

        public Action<Dictionary<int, bool>> OnSelectStateChanged;

        protected virtual void Awake()
        {
            mouseEventHandler.OnEndMouseSelect += OnEndMouseSelect;
        }

        protected virtual void OnDestroy()
        {
            mouseEventHandler.OnEndMouseSelect -= OnEndMouseSelect;
        }

        public override void Initialize(UnitInfo[] unitInfos, Vector3[] startPoints, float unitDistance, float facingAngle)
        {
            for (int i = 0; i < unitInfos.Length; i++)
            {
                GameObject controllerObj = new GameObject("Controller_" + unitInfos[i]._UnitType);
                controllerObj.transform.SetParent(this.transform);

                PlayerFormationController controller = controllerObj.AddComponent<PlayerFormationController>();
                controller.Initialize(unitInfos[i], startPoints[i], unitDistance, i, facingAngle, mouseEventHandler);

                allControllerList.Add(controller);
            }

            uiController.Initialize(unitInfos);
        }

        public override void OnAllUnitsDead(BaseFormationController controller)
        {
            if (selectedControllerList.Contains(controller as PlayerFormationController) == true)
            {
                selectedControllerList.Remove(controller as PlayerFormationController);
            }

            base.OnAllUnitsDead(controller);
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