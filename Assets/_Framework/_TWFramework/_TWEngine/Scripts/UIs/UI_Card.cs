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

        [Space(10)]
        [SerializeField]
        protected Image iconImage;
        [SerializeField]
        protected TextMeshProUGUI contentText;

        protected int totalUnitCount;

        public void Initialize(UnitInfo unitInfo, BaseFormationController controller)
        {
            iconImage.sprite = unitInfo.IconSprite;

            controller.OnUnitCountChanged += OnUnitCountChanged;

            totalUnitCount = unitInfo.UnitCount;
            contentText.text = totalUnitCount + " / " + totalUnitCount;
        }

        protected void OnUnitCountChanged(int unitCount)
        {
            contentText.text = totalUnitCount + " / " + unitCount;
        }
    }
}