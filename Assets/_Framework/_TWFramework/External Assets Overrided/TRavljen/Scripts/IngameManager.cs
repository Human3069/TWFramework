using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public class IngameManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[IngameManager]</b></color> {0}";

        protected static IngameManager _instance;
        public static IngameManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [SerializeField]
        protected List<PlayerFormationController> allControllerList = new List<PlayerFormationController>();
        [ReadOnly]
        [SerializeField]
        protected List<PlayerFormationController> selectedControllerList = new List<PlayerFormationController>();

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this.gameObject);
                return;
            }

            for (int i = 0; i < allControllerList.Count; i++)
            {
                allControllerList[i].Initialize(i);
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        public List<PlayerFormationController> GetSelectedControllerList()
        {
            return selectedControllerList;
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