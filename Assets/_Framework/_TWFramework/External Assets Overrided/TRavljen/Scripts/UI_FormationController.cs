using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace _TW_Framework
{
    public class UI_FormationController : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_FormationController]</b></color> {0}";

        [SerializeField]
        private Text unitCountText;
        [SerializeField]
        private Text unitSpacingText;
        [SerializeField]
        private Text unitNoiseText;

        [Space(10)]
        [SerializeField]
        protected Button[] cardButtons;

        protected int pinnedIndex = -1;

        protected void Start()
        {
            unitCountText.text = "Unit Count: 13";
            unitSpacingText.text = "Unit Spacing : 2.00";
            unitNoiseText.text = "Unit Noise : 0.00";

            cardButtons[0].onClick.AddListener(() => OnClickCardButton(0));
            cardButtons[1].onClick.AddListener(() => OnClickCardButton(1));
            cardButtons[2].onClick.AddListener(() => OnClickCardButton(2));
        }

        public void OnValueChangedUnitCountSlider(float _value)
        {
            foreach (PlayerFormationController controller in IngameManager.Instance.GetSelectedControllerList())
            {
                controller.UnitCount = (int)_value;
            }

            unitCountText.text = "Unit Count: " + _value;
        }

        public void OnValueChangedUnitSpacingSlider(float _value)
        {
            foreach (PlayerFormationController controller in IngameManager.Instance.GetSelectedControllerList())
            {
                controller.UnitSpacing = _value;
            }

            string _formatted = _value.ToString("0.00");
            unitSpacingText.text = "Unit Spacing : " + _formatted;
        }

        public void OnValueChangedUnitNoiseSlider(float _value)
        {
            foreach (PlayerFormationController controller in IngameManager.Instance.GetSelectedControllerList())
            {
                controller.NoiseAmount = _value;
            }

            unitNoiseText.text = "Unit Noise : " + _value.ToString("F2");
        }

        public void OnClickRectangleFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickRectangleFormationButton()");

            foreach (PlayerFormationController controller in IngameManager.Instance.GetSelectedControllerList())
            {
                controller.ChangeUnitFormation(new RectangleFormation(controller));
            }
        }

        public void OnClickCircleFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickCircleFormationButton()");

            foreach (PlayerFormationController controller in IngameManager.Instance.GetSelectedControllerList())
            {
                controller.ChangeUnitFormation(new CircleFormation(controller));
            }
        }

        public void OnClickLineFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickLineFormationButton()");

            foreach (PlayerFormationController controller in IngameManager.Instance.GetSelectedControllerList())
            {
                controller.ChangeUnitFormation(new LineFormation(controller));
            }
        }

        public void OnClickTriangleFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickTriangleFormationButton()");

            foreach (PlayerFormationController controller in IngameManager.Instance.GetSelectedControllerList())
            {
                controller.ChangeUnitFormation(new TriangleFormation(controller));
            }
        }

        public void OnClickConeFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickConeFormationButton()");

            foreach (PlayerFormationController controller in IngameManager.Instance.GetSelectedControllerList())
            {
                controller.ChangeUnitFormation(new ConeFormation(controller));
            }
        }

        public void OnValueChangedPivotToggle(bool isOn)
        {
            foreach (PlayerFormationController controller in IngameManager.Instance.GetSelectedControllerList())
            {
                controller.IsPivotInMiddle = isOn;
            }
        }

        protected void OnClickCardButton(int targetIndex)
        {
            List<PlayerFormationController> selectedControllerList = IngameManager.Instance.GetSelectedControllerList();

            List<int> deselectedIndexList = new List<int>() { 0, 1, 2 };
            List<int> selectedIndexList = selectedControllerList.ConvertAll(x => x.SelectedIndex);

            if (Input.GetKey(KeyCode.LeftShift) == true)
            {
                if (pinnedIndex != -1)
                {
                    int towardedIndex = pinnedIndex;
                    while (towardedIndex != targetIndex)
                    {
                        towardedIndex = (int)Mathf.MoveTowards(towardedIndex, targetIndex, 1);
                        selectedIndexList.Add(towardedIndex);
                    }
                }
            }
            else if (Input.GetKey(KeyCode.LeftControl) == true)
            {
                pinnedIndex = targetIndex;

                if (selectedIndexList.Contains(targetIndex) == true)
                {
                    selectedIndexList.Remove(targetIndex);
                }
                else
                {
                    selectedIndexList.Add(targetIndex);
                }
            }
            else
            {
                pinnedIndex = targetIndex;

                selectedIndexList.Clear();
                selectedIndexList.Add(targetIndex);
            }

            deselectedIndexList.RemoveAll(x => selectedIndexList.Contains(x) == true);
            foreach (int index in selectedIndexList)
            {
                IngameManager.Instance.SelectController(index);
                cardButtons[index].image.color = Color.green;
            }

            foreach (int index in deselectedIndexList)
            {
                IngameManager.Instance.DeselectController(index);
                cardButtons[index].image.color = Color.white;
            }

            foreach (int index in selectedIndexList)
            {
                IngameManager.Instance.SelectStateChanged(index);
            }
        }
    }
}
