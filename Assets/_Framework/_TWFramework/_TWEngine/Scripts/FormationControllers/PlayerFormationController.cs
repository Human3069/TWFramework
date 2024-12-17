using System;
using System.Collections.Generic;
using TRavljen.UnitFormation;
using UnityEngine;

namespace _TW_Framework
{
    public class PlayerFormationController : BaseFormationController
    {
        [Header("=== PlayerFormationController ===")]
        [ReadOnly]
        [SerializeField]
        public Vector2 selectedNormalRange = new Vector2(-1f, -1f);
     
        public Action OnSelectedAction;
        public Action OnDeselectedAction;

        protected MouseEventHandler _mouseEventHandler;
        
        public void Initialize(UnitInfo unitInfo, Vector3 startPoint, float unitDistance, int selectedIndex, float facingAngle, MouseEventHandler mouseEventHandler)
        {
            _UnitInfo = unitInfo;
            for (int i = 0; i < unitInfo.UnitCount; i++)
            {
                GameObject unitObj = Instantiate(unitInfo.UnitPrefab);
                unitObj.transform.SetParent(this.transform);

                UnitHandler unit = unitObj.GetComponent<UnitHandler>();
                unit.Initialize(this, _teamType, _UnitInfo);

                UnitHandlerList.Add(unit);
            }

            lineStartPos = startPoint + (Vector3.left * unitDistance / 2f);
            lineEndPos = startPoint + (Vector3.right * unitDistance / 2f);

            this._unitCount = unitInfo.UnitCount;
            this._unitSpacing = _UnitInfo.GetPairValue(FormationType.Rectangle).UnitSpacing;
            this._noiseAmount = _UnitInfo.GetPairValue(FormationType.Rectangle).NoiseAmount;

            float _unitsPerRowUnclamped = unitDistance / UnitSpacing;
            int _unitsPerRow = (int)(unitDistance / UnitSpacing);
            this._unitsPerRowRemained = _unitsPerRowUnclamped - _unitsPerRow;

            _unitsPerRow = Mathf.Clamp(_unitsPerRow, 1, int.MaxValue);
            this.UnitsPerRow = _unitsPerRow;

            _currentFormation = new RectangleFormation((int)UnitsPerRow, UnitSpacing, true, IsPivotInMiddle);
            List<Vector3> posList = FormationPositionerEx.GetAlignedPositionList(UnitHandlerList.Count, CurrentFormation, startPoint, facingAngle);
            for (int i = 0; i < UnitHandlerList.Count; i++)
            {
                UnitHandlerList[i].transform.position = posList[i] + UnitFormationHelper.GetNoise(NoiseAmount);
                UnitHandlerList[i].transform.eulerAngles = new Vector3(0f, facingAngle, 0f);
            }

            SelectedIndex = selectedIndex;
            _mouseEventHandler = mouseEventHandler;

            GameObject silhouetteControllerObj = new GameObject("SilhouetteController");
            silhouetteControllerObj.transform.SetParent(this.transform);

            SilhouetteController silhouetteController = silhouetteControllerObj.AddComponent<SilhouetteController>();
            silhouetteController.Initialize(this, mouseEventHandler, unitInfo.SilhouettePrefab, posList, facingAngle);
        }

        public (Vector3, Vector3) GetControlPoints()
        {
            return (lineStartPos, lineEndPos);
        }

        public (Vector3, Vector3) GetControlPoints(Vector3 lineStartPos, Vector3 lineEndPos)
        {
            Vector3 controlStartPos = Vector3.Lerp(lineStartPos, lineEndPos, selectedNormalRange.x);
            Vector3 controlEndPos = Vector3.Lerp(lineStartPos, lineEndPos, selectedNormalRange.y);

            return (controlStartPos, controlEndPos);
        }

        public (Vector3, Vector3) GetControlPoints(Vector3 lineStartPos, Vector3 lineEndPos, out float length)
        {
            (Vector3 controlStartPos, Vector3 controlEndPos) = GetControlPoints(lineStartPos, lineEndPos);
            length = Vector3.Distance(controlStartPos, controlEndPos);

            return (controlStartPos, controlEndPos);
        }

        protected void OnDuringHandling(Vector3 lineStartPos, Vector3 lineEndPos)
        {
            GetControlPoints(lineStartPos, lineEndPos, out float controlLength);

            if (controlLength > MAX_FORMABLE_THRESHOLD)
            {
                float _unitsPerRowUnclamped = controlLength / UnitSpacing;
                int _unitsPerRow = (int)(controlLength / UnitSpacing);
                this._unitsPerRowRemained = _unitsPerRowUnclamped - _unitsPerRow;

                _unitsPerRow = Mathf.Clamp(_unitsPerRow, 1, int.MaxValue);
                this.UnitsPerRow = _unitsPerRow;
            }

            ReinstantiateFormation();
        }

        protected void OnEndHandling(Vector3 lineStartPos, Vector3 lineEndPos)
        {
            (Vector3 controlStartPos, Vector3 controlEndPos) = GetControlPoints(lineStartPos, lineEndPos);

            TargetController = null;
            ApplyMouseFormationing(controlStartPos, controlEndPos);
        }

        protected void OnClickEnemyHandler(BaseFormationController controller)
        {
            TargetController = controller;
        }

        public void OnSelected()
        {
            _mouseEventHandler.OnDuringHandling += OnDuringHandling;
            _mouseEventHandler.OnEndHandling += OnEndHandling;
            _mouseEventHandler.OnClickEnemyHandler += OnClickEnemyHandler;

            foreach (UnitHandler unit in UnitHandlerList)
            {
                unit.OnSelected();
            }
            OnSelectedAction?.Invoke();
        }

        public void OnSelectionStateChanged(int selectedCount, int selectedIndex)
        {
            selectedNormalRange = new Vector2((float)selectedIndex / selectedCount, (float)(selectedIndex + 1f) / selectedCount);
        }

        public virtual void OnDeselected()
        {
            selectedNormalRange = new Vector2(-1f, -1f);

            _mouseEventHandler.OnClickEnemyHandler -= OnClickEnemyHandler;
            _mouseEventHandler.OnEndHandling -= OnEndHandling;
            _mouseEventHandler.OnDuringHandling -= OnDuringHandling;

            foreach (UnitHandler unit in UnitHandlerList)
            {
                unit.OnDeselected();
            }
            OnDeselectedAction?.Invoke();
        }
    }
}