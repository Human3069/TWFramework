using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _TW_Framework
{
    public class UI_FormationController : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_FormationController]</b></color> {0}";

        [Header("Cheat Panel")]
        [SerializeField]
        private Text unitCountText;
        [SerializeField]
        private Text unitSpacingText;
        [SerializeField]
        private Text unitNoiseText;

        [Header("Bottombar Panel")]
        [SerializeField]
        protected GameObject buttonPrefab;
        [SerializeField]
        protected Transform cardParent;

        [Space(10)]
        [SerializeField]
        protected List<Button> cardButtonList;

        protected int pinnedIndex = -1;

        protected void Awake()
        {
            TWManager.Instance.Player.OnSelectStateChanged += OnSelectStateChanged;

            unitCountText.text = "Unit Count: 13";
            unitSpacingText.text = "Unit Spacing : 2.00";
            unitNoiseText.text = "Unit Noise : 0.00";
        }

        public void Initialize(UnitInfo[] unitInfos)
        {
            for (int i = 0; i < unitInfos.Length; i++)
            {
                int localIndex = i;

                GameObject buttonObj = Instantiate(buttonPrefab, cardParent);
                Button button = buttonObj.GetComponent<Button>();
                button.onClick.AddListener(() => OnClickCardButton(localIndex));

                cardButtonList.Add(button);
            }
        }

        public void OnValueChangedUnitCountSlider(float _value)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.UnitCount = (int)_value);

            unitCountText.text = "Unit Count: " + _value;
        }

        public void OnValueChangedUnitSpacingSlider(float _value)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.UnitSpacing = _value);

            string _formatted = _value.ToString("0.00");
            unitSpacingText.text = "Unit Spacing : " + _formatted;
        }

        public void OnValueChangedUnitNoiseSlider(float _value)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.NoiseAmount = _value);

            unitNoiseText.text = "Unit Noise : " + _value.ToString("F2");
        }

        public void OnClickRectangleFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickRectangleFormationButton()");

            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.ChangeUnitFormation(new RectangleFormation(x)));
        }

        public void OnClickCircleFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickCircleFormationButton()");

            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.ChangeUnitFormation(new CircleFormation(x)));
        }

        public void OnClickLineFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickLineFormationButton()");

            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.ChangeUnitFormation(new LineFormation(x)));
        }

        public void OnClickTriangleFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickTriangleFormationButton()");

            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.ChangeUnitFormation(new TriangleFormation(x)));
        }

        public void OnClickConeFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickConeFormationButton()");

            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.ChangeUnitFormation(new ConeFormation(x)));
        }

        public void OnValueChangedPivotToggle(bool isOn)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.IsPivotInMiddle = isOn);
        }

        protected void OnEndMouseSelect(List<PlayerFormationController> controllerList)
        {
            foreach (PlayerFormationController controller in controllerList)
            {
                cardButtonList[controller.SelectedIndex].image.color = Color.green;
            }
        }

        protected void OnSelectStateChanged(Dictionary<int, bool> hasSelectedDic)
        {
            foreach (KeyValuePair<int, bool> pair in hasSelectedDic)
            {
                if (pair.Value == true)
                {
                    cardButtonList[pair.Key].image.color = Color.green;
                }
                else
                {
                    cardButtonList[pair.Key].image.color = Color.white;
                }
            }
        }

        protected void OnClickCardButton(int targetIndex)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();

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
                TWManager.Instance.Player.SelectController(index);
                cardButtonList[index].image.color = Color.green;
            }

            foreach (int index in deselectedIndexList)
            {
                TWManager.Instance.Player.DeselectController(index);
                cardButtonList[index].image.color = Color.white;
            }

            foreach (int index in selectedIndexList)
            {
                TWManager.Instance.Player.SelectStateChanged(index);
            }
        }
    }
}
