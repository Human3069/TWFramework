using UnityEngine;

namespace _TW_Framework
{
    public class PlayerFormationController : FormationController
    {
        [Header("=== PlayerFormationController ===")]
        [SerializeField]
        protected MouseEventHandler mouseEventHandler;

        protected virtual void Awake()
        {
            mouseEventHandler.OnDuringHandling += OnDuringHandling;
            mouseEventHandler.OnEndHandling += OnEndHandling;

            mouseEventHandler.OnClickEnemyHandler += OnClickEnemyHandler;
        }

        protected virtual void OnDestroy()
        {
            mouseEventHandler.OnClickEnemyHandler -= OnClickEnemyHandler;

            mouseEventHandler.OnEndHandling -= OnEndHandling;
            mouseEventHandler.OnDuringHandling -= OnDuringHandling;
        }

        protected void OnDuringHandling(Vector3 lineStartPos, Vector3 lineEndPos, float lineLength)
        {
            if (lineLength > MAX_FORMABLE_THRESHOLD)
            {
                float _unitsPerRowUnclamped = lineLength / UnitSpacing;
                int _unitsPerRow = (int)(lineLength / UnitSpacing);
                this._unitsPerRowRemained = _unitsPerRowUnclamped - _unitsPerRow;

                _unitsPerRow = Mathf.Clamp(_unitsPerRow, 1, int.MaxValue);
                this.UnitsPerRow = _unitsPerRow;
            }

            ReinstantiateFormation();
        }

        protected void OnEndHandling(Vector3 lineStartPos, Vector3 lineEndPos)
        {
            TargetController = null;
            ApplyMouseFormationing(lineStartPos, lineEndPos);
        }

        protected void OnClickEnemyHandler(FormationController controller)
        {
            TargetController = controller;
        }
    }
}