﻿using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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

        [Header("Selection Panel")]
        [SerializeField]
        protected Image selectionImage;

        [Header("Bottombar Panel - Formations")]
        [SerializeField]
        protected Toggle rectangleFormationToggle;
        [SerializeField]
        protected Toggle triangleFormationToggle;
        [SerializeField]
        protected Toggle coneFormationToggle;
        [SerializeField]
        protected Toggle squareFormationToggle;
        [SerializeField]
        protected Toggle skirmishFormationToggle;

        [Header("Bottombar Panel - AttackType")]
        [SerializeField]
        protected Toggle meleeAttackToggle;
        [SerializeField]
        protected Toggle rangeAttackToggle;

        [Header("Bottombar Panel - IngameSpeed")]
        [SerializeField]
        protected Toggle _0xSpeedToggle;
        [SerializeField]
        protected Toggle _0_5xSpeedToggle;
        [SerializeField]
        protected Toggle _1xSpeedToggle;
        [SerializeField]
        protected Toggle _2xSpeedToggle;
        [SerializeField]
        protected Toggle _4xSpeedToggle;

        [Header("Bottombar Panel - Cards")]
        [SerializeField]
        protected UI_Card cardPrefab;
        [SerializeField]
        protected Transform cardParent;

        [Space(10)]
        [SerializeField]
        protected List<Button> cardButtonList;

        public static int PinnedIndex = -1;

        protected void Awake()
        {
            TWManager.Instance.Player.MouseEventHandler.OnStartMouseSelect += OnStartMouseSelect;
            TWManager.Instance.Player.MouseEventHandler.OnDuringMouseSelect += OnDuringMouseSelect;
            TWManager.Instance.Player.OnSelectStateChanged += OnSelectStateChanged;

            selectionImage.gameObject.SetActive(false);

            unitCountText.text = "Unit Count: 13";
            unitSpacingText.text = "Unit Spacing : 2.00";
            unitNoiseText.text = "Unit Noise : 0.00";

            rectangleFormationToggle.onValueChanged.AddListener(OnValueChangedRectangleFormationToggle);
            triangleFormationToggle.onValueChanged.AddListener(OnValueChangedTriangleFormationToggle);
            coneFormationToggle.onValueChanged.AddListener(OnValueChangedConeFormationToggle);
            squareFormationToggle.onValueChanged.AddListener(OnValueChangedSquareFormationToggle);
            skirmishFormationToggle.onValueChanged.AddListener(OnValueChangedSkirmishFormationToggle);

            meleeAttackToggle.onValueChanged.AddListener(OnValueChangedMeleeAttackToggle);
            void OnValueChangedMeleeAttackToggle(bool isOn)
            {
                if (isOn == true)
                {
                    List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
                    selectedControllerList.ForEach(x => x.AttackType = AttackType.Melee);
                }
            }

            rangeAttackToggle.onValueChanged.AddListener(OnValueChangedRangeAttackToggle);
            void OnValueChangedRangeAttackToggle(bool isOn)
            {
                if (isOn == true)
                {
                    List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
                    selectedControllerList.ForEach(x => x.AttackType = AttackType.Ranged);
                }
            }

            _0xSpeedToggle.onValueChanged.AddListener(OnValueChanged0xToggle);
            void OnValueChanged0xToggle(bool isOn)
            {
                if (isOn == true)
                {
                    Time.timeScale = 0f;
                }
            }

            _0_5xSpeedToggle.onValueChanged.AddListener(OnValueChanged0_5xToggle);
            void OnValueChanged0_5xToggle(bool isOn)
            {
                if (isOn == true)
                {
                    Time.timeScale = 0.5f;
                }
            }

            _1xSpeedToggle.onValueChanged.AddListener(OnValueChanged1xToggle);
            void OnValueChanged1xToggle(bool isOn)
            {
                if (isOn == true)
                {
                    Time.timeScale = 1f;
                }
            }

            _2xSpeedToggle.onValueChanged.AddListener(OnValueChanged2xToggle);
            void OnValueChanged2xToggle(bool isOn)
            {
                if (isOn == true)
                {
                    Time.timeScale = 2f;
                }
            }

            _4xSpeedToggle.onValueChanged.AddListener(OnValueChanged4xToggle);
            void OnValueChanged4xToggle(bool isOn)
            {
                if (isOn == true)
                {
                    Time.timeScale = 4f;
                }
            }
        }

        public void Initialize(UnitInfo[] unitInfos, List<BaseFormationController> controllerList)
        {
            for (int i = 0; i < unitInfos.Length; i++)
            {
                UI_Card cardInstance = Instantiate(cardPrefab, cardParent);

                cardButtonList.Add(cardInstance.SelectButton);
                cardInstance.Initialize(unitInfos[i], this, controllerList[i]);
            }
        }

        public void OnClickAnyButton(List<int> selectedIndexList, List<int> deselectedIndexList)
        {
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

            ResetFormationToggles();
        }

        public void OnControllerDead(int allDeadControllerIndex)
        {
            cardButtonList.RemoveAt(allDeadControllerIndex);
            ResetFormationToggles();
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
            selectedControllerList.ForEach(x => x.ChangeUnitFormation(FormationType.Rectangle));
        }

        public void OnClickCircleFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickCircleFormationButton()");

            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.ChangeUnitFormation(FormationType.Circle));
        }

        public void OnClickLineFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickLineFormationButton()");

            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.ChangeUnitFormation(FormationType.Line));
        }

        public void OnClickTriangleFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickTriangleFormationButton()");

            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.ChangeUnitFormation(FormationType.Triangle));
        }

        public void OnClickConeFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickConeFormationButton()");

            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.ChangeUnitFormation(FormationType.Cone));
        }

        public void OnValueChangedPivotToggle(bool isOn)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            selectedControllerList.ForEach(x => x.IsPivotInMiddle = isOn);
        }

        protected void OnValueChangedRectangleFormationToggle(bool isOn)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            if (isOn == true)
            {
                selectedControllerList.ForEach(x => x.ChangeUnitFormation(FormationType.Rectangle));
            }
        }

        protected void OnValueChangedTriangleFormationToggle(bool isOn)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            if (isOn == true)
            {
                selectedControllerList.ForEach(x => x.ChangeUnitFormation(FormationType.Triangle));
            }
        }

        protected void OnValueChangedConeFormationToggle(bool isOn)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();

            if (isOn == true)
            {
                selectedControllerList.ForEach(x => x.ChangeUnitFormation(FormationType.Cone));
            }
        }

        protected void OnValueChangedSquareFormationToggle(bool isOn)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();

            if (isOn == true)
            {
                selectedControllerList.ForEach(x => x.ChangeUnitFormation(FormationType.Square));
            }
        }

        protected void OnValueChangedSkirmishFormationToggle(bool isOn)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();

            if (isOn == true)
            {
                selectedControllerList.ForEach(x => x.ChangeUnitFormation(FormationType.Skirmish));
            }
        }

        protected void OnEndMouseSelect(List<PlayerFormationController> controllerList)
        {
            foreach (PlayerFormationController controller in controllerList)
            {
                cardButtonList[controller.SelectedIndex].image.color = Color.green;
            }
        }

        protected void OnStartMouseSelect()
        {
            // Selection Image
            SetSelectionImageRectAsync().Forget();

            // Bottombar
            List<PlayerFormationController> allControllerList = TWManager.Instance.Player.GetAllControllerList();
            foreach (PlayerFormationController controller in allControllerList)
            {
                cardButtonList[controller.SelectedIndex].image.color = Color.white;
            }
        }

        protected async UniTaskVoid SetSelectionImageRectAsync()
        {
            Vector2 mousePos = Input.mousePosition;
            selectionImage.rectTransform.position = mousePos;
            selectionImage.gameObject.SetActive(true);

            while (TWManager.Instance.Player.MouseEventHandler.IsSelecting == true)
            {
                Vector2 currentMousePos = Input.mousePosition;
                Vector2 mouseDelta = currentMousePos - mousePos;

                selectionImage.rectTransform.pivot = new Vector2(mouseDelta.x < 0 ? 1 : 0, mouseDelta.y < 0 ? 1 : 0);
                selectionImage.rectTransform.sizeDelta = new Vector2(Mathf.Abs(mouseDelta.x), Mathf.Abs(mouseDelta.y));

                await UniTask.Yield();
            }
            selectionImage.gameObject.SetActive(false);
        }

        protected void OnDuringMouseSelect(List<PlayerFormationController> controllerList)
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

            ResetFormationToggles();
        }

        protected void OnClickCardButton(int targetIndex)
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            List<PlayerFormationController> deselectedControllerList = TWManager.Instance.Player.GetAllControllerList();

            List<int> deselectedIndexList = deselectedControllerList.ConvertAll(x => x.SelectedIndex);
            List<int> selectedIndexList = selectedControllerList.ConvertAll(x => x.SelectedIndex);

            if (Input.GetKey(KeyCode.LeftShift) == true)
            {
                if (PinnedIndex != -1)
                {
                    int towardedIndex = PinnedIndex;
                    while (towardedIndex != targetIndex)
                    {
                        towardedIndex = (int)Mathf.MoveTowards(towardedIndex, targetIndex, 1);
                        selectedIndexList.Add(towardedIndex);
                    }
                }
            }
            else if (Input.GetKey(KeyCode.LeftControl) == true)
            {
                PinnedIndex = targetIndex;

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
                PinnedIndex = targetIndex;

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

            ResetFormationToggles();

#if UNITY_EDITOR
            UnityEditor.Selection.objects = selectedControllerList.ConvertAll(x => x.gameObject).ToArray();
#endif
        }

        protected void ResetFormationToggles()
        {
            List<PlayerFormationController> selectedControllerList = TWManager.Instance.Player.GetSelectedControllerList();
            if (selectedControllerList.Count == 0)
            {
                rectangleFormationToggle.gameObject.SetActive(false);
                squareFormationToggle.gameObject.SetActive(false);
                skirmishFormationToggle.gameObject.SetActive(false);
                triangleFormationToggle.gameObject.SetActive(false);
                coneFormationToggle.gameObject.SetActive(false);
            }
            else
            {
                FormationType commonType = FormationType.Rectangle | FormationType.Square | FormationType.Skirmish | FormationType.Triangle | FormationType.Cone | FormationType.Circle | FormationType.Line;
                FormationType selectedType = FormationType.Rectangle | FormationType.Square | FormationType.Skirmish | FormationType.Triangle | FormationType.Cone | FormationType.Circle | FormationType.Line;
                foreach (PlayerFormationController controller in selectedControllerList)
                {
                    FormationType availableType = controller._UnitInfo._FormationType;
                    FormationType currentType = controller.CurrentFormationType;

                    commonType &= availableType;
                    selectedType &= currentType;
                }

                rectangleFormationToggle.gameObject.SetActive(commonType.HasFlag(FormationType.Rectangle));
                squareFormationToggle.gameObject.SetActive(commonType.HasFlag(FormationType.Square));
                skirmishFormationToggle.gameObject.SetActive(commonType.HasFlag(FormationType.Skirmish));
                triangleFormationToggle.gameObject.SetActive(commonType.HasFlag(FormationType.Triangle));
                coneFormationToggle.gameObject.SetActive(commonType.HasFlag(FormationType.Cone));

                rectangleFormationToggle.SetIsOnWithoutNotify(false);
                squareFormationToggle.SetIsOnWithoutNotify(false);
                skirmishFormationToggle.SetIsOnWithoutNotify(false);
                triangleFormationToggle.SetIsOnWithoutNotify(false);
                coneFormationToggle.SetIsOnWithoutNotify(false);

                SelectTypeToToggle(selectedType)?.SetIsOnWithoutNotify(true);
            }
        }

        protected Toggle SelectTypeToToggle(FormationType type)
        {
            if (type == FormationType.Rectangle)
            {
                return rectangleFormationToggle;
            }
            else if (type == FormationType.Square)
            {
                return squareFormationToggle;
            }
            else if (type == FormationType.Skirmish)
            {
                return skirmishFormationToggle;
            }
            else if (type == FormationType.Triangle)
            {
                return triangleFormationToggle;
            }
            else if (type == FormationType.Cone)
            {
                return coneFormationToggle;
            }
            else
            {
                return null;
            }
        }
    }
}
