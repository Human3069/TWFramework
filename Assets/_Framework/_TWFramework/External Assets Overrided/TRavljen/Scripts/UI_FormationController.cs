using UnityEngine;
using TRavljen.UnitFormation.Formations;
using UnityEngine.UI;

namespace _TW_Framework
{
    public class UI_FormationController : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_FormationController]</b></color> {0}";

        [SerializeField]
        protected FormationController _formationController;

        [Space(10)]
        [SerializeField]
        private Text unitCountText;
        [SerializeField]
        private Text unitSpacingText;
        [SerializeField]
        private Text unitsPerRowText;
        [SerializeField]
        private Text unitNoiseText;

        private void Start()
        {
            unitCountText.text = "Unit Count: 13";
            unitSpacingText.text = "Unit Spacing : 2.00";
            unitsPerRowText.text = "Units per ROW: 4";
            unitNoiseText.text = "Unit Noise : 0.00";
        }

        public void OnValueChangedUnitsPerRowSlider(float _value)
        {
            _formationController.UnitsPerRow = (int)_value;

            unitsPerRowText.text = "Units per ROW: " + _value;
        }

        public void OnValueChangedUnitCountSlider(float _value)
        {
            _formationController.UnitCount = (int)_value;

            unitCountText.text = "Unit Count: " + _value;
        }

        public void OnValueChangedUnitSpacingSlider(float _value)
        {
            _formationController.UnitSpacing = _value;

            string _formatted = _value.ToString("0.00");
            unitSpacingText.text = "Unit Spacing : " + _formatted;
        }

        public void OnValueChangedUnitNoiseSlider(float _value)
        {
            _formationController.NoiseAmount = _value;

            unitNoiseText.text = "Unit Noise : " + _value.ToString("F2");
        }

        public void OnClickRectangleFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickRectangleFormationButton()");

            _formationController.ChangeUnitFormation(typeof(RectangleFormation));
        }

        public void OnClickCircleFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickCircleFormationButton()");

            _formationController.ChangeUnitFormation(typeof(CircleFormation));
        }

        public void OnClickLineFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickLineFormationButton()");

            _formationController.ChangeUnitFormation(typeof(LineFormation));
        }

        public void OnClickTriangleFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickTriangleFormationButton()");

            _formationController.ChangeUnitFormation(typeof(TriangleFormation));
        }

        public void OnClickConeFormationButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickConeFormationButton()");

            _formationController.ChangeUnitFormation(typeof(ConeFormation));
        }

        public void OnValueChangedPivotToggle(bool isOn)
        {
            _formationController.IsPivotInMiddle = isOn;
        }
    }
}
