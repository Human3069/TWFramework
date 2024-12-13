using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace _TW_Framework
{
    public class PlayerFormationController : FormationController
    {
        [Header("=== PlayerFormationController ===")]
        [SerializeField]
        protected MouseEventHandler mouseEventHandler;
       
        [Space(10)]
        [ReadOnly]
        public int SelectedIndex = -1;
        [ReadOnly]
        [SerializeField]
        public Vector2 selectedNormalRange = new Vector2(-1f, -1f);

        public Action OnSelectedAction;
        public Action OnDeselectedAction;

        public void Initialize(int selectedIndex)
        {
            SelectedIndex = selectedIndex;
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

        protected void OnClickEnemyHandler(FormationController controller)
        {
            TargetController = controller;
        }

        public void OnSelected()
        {
            mouseEventHandler.OnDuringHandling += OnDuringHandling;
            mouseEventHandler.OnEndHandling += OnEndHandling;
            mouseEventHandler.OnClickEnemyHandler += OnClickEnemyHandler;

            OnSelectedAction?.Invoke();
        }

        public void OnSelectionStateChanged(int selectedCount, int selectedIndex)
        {
            selectedNormalRange = new Vector2((float)selectedIndex / selectedCount, (float)(selectedIndex + 1f) / selectedCount);
        }

        public virtual void OnDeselected()
        {
            selectedNormalRange = new Vector2(-1f, -1f);

            mouseEventHandler.OnClickEnemyHandler -= OnClickEnemyHandler;
            mouseEventHandler.OnEndHandling -= OnEndHandling;
            mouseEventHandler.OnDuringHandling -= OnDuringHandling;

            OnDeselectedAction?.Invoke();
        }
    }
}