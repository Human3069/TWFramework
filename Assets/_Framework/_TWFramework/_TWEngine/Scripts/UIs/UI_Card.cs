using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _TW_Framework
{
    public class UI_Card : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_Card]</b></color> {0}";

        public Button SelectButton;
        protected UI_FormationController _uiController;
        protected BaseFormationController _controller;

        [Space(10)]
        [SerializeField]
        protected Image iconImage;
        [SerializeField]
        protected TextMeshProUGUI contentText;

        protected int totalUnitCount;

        public void Initialize(UnitInfo unitInfo, UI_FormationController uiController, BaseFormationController controller)
        {
            _uiController = uiController;
            _controller = controller;
            _controller.OnUnitCountChanged += OnUnitCountChanged;

            SelectButton.onClick.AddListener(OnClickButton);

            iconImage.sprite = unitInfo.IconSprite;

            totalUnitCount = unitInfo.UnitCount;
            contentText.text = totalUnitCount + " / " + totalUnitCount;
        }

        public void OnClickButton()
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            List<PlayerFormationController> deselectedControllerList = TWManager.Instance.Player.GetAllControllerList();

            List<int> deselectedIndexList = deselectedControllerList.ConvertAll(x => x.SelectedIndex);
            List<int> selectedIndexList = selectedControllerList.ConvertAll(x => x.SelectedIndex);

            int targetIndex = _controller.SelectedIndex;
            if (Input.GetKey(KeyCode.LeftShift) == true)
            {
                if (UI_FormationController.PinnedIndex != -1)
                {
                    int towardedIndex = UI_FormationController.PinnedIndex;
                    while (towardedIndex != targetIndex)
                    {
                        towardedIndex = (int)Mathf.MoveTowards(towardedIndex, targetIndex, 1);
                        selectedIndexList.Add(towardedIndex);
                    }
                }
            }
            else if (Input.GetKey(KeyCode.LeftControl) == true)
            {
                UI_FormationController.PinnedIndex = targetIndex;

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
                UI_FormationController.PinnedIndex = targetIndex;

                selectedIndexList.Clear();
                selectedIndexList.Add(targetIndex);
            }

            _uiController.OnClickAnyButton(selectedIndexList, deselectedIndexList);
        }

        protected void OnUnitCountChanged(int unitCount)
        {
            contentText.text = unitCount + " / " + totalUnitCount;

            if (unitCount == 0)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}