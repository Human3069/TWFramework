using System;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public class IngameManager : BaseSingleton<IngameManager>
    {
        private const string LOG_FORMAT = "<color=white><b>[IngameManager]</b></color> {0}";

        [SerializeField]
        protected MouseEventHandler mouseEventHandler;

        [Space(10)]
        [SerializeField]
        protected List<PlayerFormationController> allControllerList = new List<PlayerFormationController>();
        [ReadOnly]
        [SerializeField]
        protected List<PlayerFormationController> selectedControllerList = new List<PlayerFormationController>();

        public Action<Dictionary<int, bool>> OnSelectStateChanged;

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < allControllerList.Count; i++)
            {
                allControllerList[i].Initialize(i);
            }

            mouseEventHandler.OnEndMouseSelect += OnEndMouseSelect;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            mouseEventHandler.OnEndMouseSelect -= OnEndMouseSelect;
        }

        public List<PlayerFormationController> GetSelectedControllerList()
        {
            return selectedControllerList;
        }

        protected void OnEndMouseSelect(List<PlayerFormationController> controllerList)
        {
            List<PlayerFormationController> deselectedControllerList = new List<PlayerFormationController>(allControllerList);
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
            SelectController(allControllerList[index]);
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
            DeselectController(allControllerList[index]);
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
            SelectStateChanged(allControllerList[index]);
        }

        public void SelectStateChanged(PlayerFormationController controller)
        {
            controller.OnSelectionStateChanged(selectedControllerList.Count, selectedControllerList.IndexOf(controller));
        }
    }
}