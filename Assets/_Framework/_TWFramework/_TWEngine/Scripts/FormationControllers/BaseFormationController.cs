using System.Collections.Generic;
using TRavljen.UnitFormation;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace _TW_Framework
{
    public enum TeamType
    {
        Player,
        Enemy,
        Neutral
    }

    public abstract class BaseFormationController : MonoBehaviour
    {
        private const string _LOG_FORMAT = "<color=white><b>[BaseFormationController]</b></color> {0}";

        public const float MAX_FORMABLE_THRESHOLD = 0.8f;
        private const float _MAX_FORMABLE_ANGLE = 120;

        protected TeamType _teamType
        {
            get
            {
                if (this is PlayerFormationController)
                {
                    return TeamType.Player;
                }
                else if (this is EnemyFormationController)
                {
                    return TeamType.Enemy;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        [Header("=== FormationController ===")]
        [ReadOnly]
        [SerializeField]
        protected BaseFormationController _targetController;
        protected BaseFormationController TargetController
        {
            get
            {
                return _targetController;
            }
            set
            {
                _targetController = value;
                if (value != null)
                {
                    OnAssignedTargetController().Forget();
                }
            }
        }

        protected virtual async UniTask OnAssignedTargetController()
        {
            while (TargetController != null && TargetController.UnitHandlerList.Count != 0)
            {
                Vector3 currentLeftDirection = (lineEndPos - lineStartPos).normalized;
                float length = (lineEndPos - lineStartPos).magnitude;

                Vector3 targetMiddlePos = TargetController.GetMiddlePos();
                Vector3 targetLeftPos = targetMiddlePos + (currentLeftDirection * length * 0.5f);
                Vector3 targetRightPos = targetMiddlePos - (currentLeftDirection * length * 0.5f);

                ApplyMouseFormationing(targetLeftPos, targetRightPos);

                await UniTask.WaitForSeconds(0.5f);
            }
        }

        protected GameObject _unitPrefab = null;

        [SerializeField]
        protected List<UnitHandler> _unitHandlerList = new List<UnitHandler>();
        public List<UnitHandler> UnitHandlerList
        {
            get
            {
                return _unitHandlerList;
            }
            protected set
            {
                _unitHandlerList = value;
            }
        }

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected int _unitCount;
        public int UnitCount
        {
            get
            {
                return _unitCount;
            }
            set
            {
                if (_unitCount != value)
                {
                    _unitCount = value;

                    if (UnitHandlerList.Count < value)
                    {
                        for (int i = UnitHandlerList.Count; i < value; i++)
                        {
                            GameObject _obj = Instantiate(_unitPrefab, transform.position, Quaternion.identity);
                            UnitHandler _unit = _obj.GetComponent<UnitHandler>();

                            _unit.Initialize(this, _teamType, _UnitInfo);

                            _obj.transform.parent = this.transform;

                            UnitHandlerList.Insert(i, _unit);
                        }

                        ApplyCurrentUnitFormation();
                    }
                    else if (UnitHandlerList.Count > value)
                    {
                        for (int i = UnitHandlerList.Count - 1; i >= value; i--)
                        {
                            GameObject _obj = UnitHandlerList[i].gameObject;
                            UnitHandlerList.RemoveAt(i);

                            // Destroy(_obj);
                        }
                    }

                    if (OnUnitCountChanged != null)
                    {
                        OnUnitCountChanged(value);
                    }
                }
            }
        }

        public void RemoveUnit(UnitHandler unitHandler)
        {
            UnitHandlerList.Remove(unitHandler);
            // Destroy(unitHandler.gameObject);

            UnitCount = UnitHandlerList.Count;
            ApplyCurrentUnitFormation();

            if (UnitCount == 0)
            {
                if (this is PlayerFormationController)
                {
                    TWManager.Instance.Player.OnAllUnitsDead(SelectedIndex);
                }
                else if (this is EnemyFormationController)
                {
                    TWManager.Instance.Enemy.OnAllUnitsDead(SelectedIndex);
                }
                else
                {
                    throw new NotImplementedException("");
                }

                // Destroy(this.gameObject);
            }
        }

        public delegate void UnitCountChanged(int unitCount);
        public event UnitCountChanged OnUnitCountChanged;

        [ReadOnly]
        [SerializeField]
        protected float _unitSpacing;
        public float UnitSpacing
        {
            get
            {
                return _unitSpacing;
            }
            set
            {
                if (_unitSpacing != value)
                {
                    _unitSpacing = value;

                    ReinstantiateFormation();
                    ApplyCurrentUnitFormation();
                }
            }
        }

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected int _unitsPerRow;
        public int UnitsPerRow
        {
            get
            {
                return _unitsPerRow;
            }
            set
            {
                if (_unitsPerRow != value)
                {
                    _unitsPerRow = Mathf.Clamp(value, 1, UnitHandlerList.Count);
                    formedUnitsPerColumn = Mathf.Clamp(UnitHandlerList.Count / value, 1, int.MaxValue);
                    lastColumnCount = UnitHandlerList.Count % value;
                }
            }
        }

        [ReadOnly]
        [SerializeField]
        protected float _unitsPerRowRemained;
        public float UnitsPerRowRemained
        {
            get
            {
                return _unitsPerRowRemained;
            }
        }

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected int formedUnitsPerColumn;

        [ReadOnly]
        [SerializeField]
        protected int lastColumnCount;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float _noiseAmount;
        public float NoiseAmount
        {
            get
            {
                return _noiseAmount;
            }
            set
            {
                if (_noiseAmount != value)
                {
                    _noiseAmount = value;

                    ApplyCurrentUnitFormation();
                }
            }
        }

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected bool _isPivotInMiddle;
        public bool IsPivotInMiddle
        {
            get
            {
                return _isPivotInMiddle;
            }
            set
            {
                if (_isPivotInMiddle != value)
                {
                    _isPivotInMiddle = value;

                    ReinstantiateFormation();
                    ApplyCurrentUnitFormation();
                }
            }
        }

        [Header("=== Yieldable ===")]
        [SerializeField]
        protected float yieldIterationInterval = 0.5f;
        [SerializeField]
        protected float yieldRadiusPerUnit = 2.5f;

        protected IFormation _currentFormation;
        public IFormation CurrentFormation
        {
            get
            {
                return _currentFormation;
            }
            protected set
            {
                _currentFormation = value;

                ApplyCurrentUnitFormation();
            }
        }

        [HideInInspector]
        public FormationType CurrentFormationType = FormationType.Rectangle;

        public Action OnUnitFormationChanged;

        [ReadOnly]
        [SerializeField]
        protected Vector3 lineStartPos;
        [ReadOnly]
        [SerializeField]
        protected Vector3 lineEndPos;

        [HideInInspector]
        public float CurrentFacingAngle;
        [ReadOnly]
        public int SelectedIndex = -1;

        [HideInInspector]
        public UnitInfo _UnitInfo;

        public void Initialize(UnitInfo unitInfo, Vector3 startPoint, float unitDistance, int selectedIndex, float facingAngle)
        {
            _UnitInfo = unitInfo;
            _unitPrefab = _UnitInfo.UnitPrefab;

            for (int i = 0; i < _UnitInfo.UnitCount; i++)
            {
                GameObject unitObj = Instantiate(_UnitInfo.UnitPrefab);
                unitObj.transform.SetParent(this.transform);

                UnitHandler unit = unitObj.GetComponent<UnitHandler>();
                unit.Initialize(this, _teamType, _UnitInfo);

                UnitHandlerList.Add(unit);
            }

            lineStartPos = startPoint + (Vector3.left * unitDistance / 2f);
            lineEndPos = startPoint + (Vector3.right * unitDistance / 2f);

            this._unitCount = _UnitInfo.UnitCount;
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
        }

        public void ResetSelectedIndex(int index)
        {
            SelectedIndex = index;
        }

        protected async UniTask YieldPositionRoutine()
        {
            await UniTaskEx.WaitForSeconds(this, 0, 0.1f);

            List<UnitHandler> movingUnitList = UnitHandlerList.FindAll(x => x.IsStopped == false && x.IsValid == true);
            while (movingUnitList.Count > 0)
            {
                movingUnitList = UnitHandlerList.FindAll(x => x.IsStopped == false && x.IsValid == true);

                for (int i = 0; i < movingUnitList.Count; i++)
                {
                    if (movingUnitList[i].IsValid == false)
                    {
                        continue;
                    }

                    Vector3 middlePos = (movingUnitList[i].TargetPos + movingUnitList[i].transform.position) / 2f;
                    DrawStartAndDestGizmoRoutine(new KeyValuePair<Vector3, Vector3>(movingUnitList[i].TargetPos, movingUnitList[i].transform.position)).Forget();

                    float nearestDistance = float.MaxValue;
                    UnitHandler nearestUnit = null;

                    Collider[] foundColliders = Physics.OverlapSphere(middlePos, yieldRadiusPerUnit * UnitSpacing);
                    DrawSphereGizmoRoutine(new KeyValuePair<Vector3, float>(middlePos, yieldRadiusPerUnit * UnitSpacing)).Forget();

                    for (int j = 0; j < foundColliders.Length; j++)
                    {
                        if (foundColliders[j].TryGetComponent<UnitHandler>(out UnitHandler overlappedHandler) == true)
                        {
                            if (overlappedHandler.IsStopped == false ||
                                overlappedHandler.IsYielding == true ||
                                overlappedHandler == movingUnitList[i] ||
                                overlappedHandler.Controller != this)
                            {
                                continue;
                            }

                            float distance = (overlappedHandler.TargetPos - middlePos).sqrMagnitude;
                            if (distance < nearestDistance)
                            {
                                nearestDistance = distance;
                                nearestUnit = overlappedHandler;
                            }
                        }
                    }

                    if (nearestUnit != null)
                    {
                        Vector3 yieldedPos = nearestUnit.TargetPos;

                        float movingUnitDistance = (movingUnitList[i].TargetPos - movingUnitList[i].transform.position).magnitude;
                        float yieldedToStartDistance = (yieldedPos - movingUnitList[i].transform.position).magnitude;
                        float yieldedToEndDistance = (yieldedPos - movingUnitList[i].TargetPos).magnitude;

                        if (movingUnitDistance > yieldedToStartDistance &&
                            movingUnitDistance > yieldedToEndDistance)
                        {
                            nearestUnit.SetTargetDestination(movingUnitList[i].TargetPos, CurrentFacingAngle);
                            nearestUnit.IsYielding = true;
                            DrawYieldedStartAndDestGizmoRoutine(new KeyValuePair<Vector3, Vector3>(movingUnitList[i].TargetPos, nearestUnit.transform.position)).Forget();

                            movingUnitList[i].SetTargetDestination(yieldedPos, CurrentFacingAngle);
                            movingUnitList[i].IsYielding = true;
                            DrawYieldedStartAndDestGizmoRoutine(new KeyValuePair<Vector3, Vector3>(yieldedPos, movingUnitList[i].transform.position)).Forget();
                        }
                    }

                    await UniTask.Yield();
                }

                await UniTaskEx.WaitForSeconds(this, 0, yieldIterationInterval);
            }
        }

        protected void ApplyMouseFormationing(Vector3 startPos, Vector3 endPos)
        {
            this.lineStartPos = startPos;
            this.lineEndPos = endPos;

            ApplyCurrentUnitFormation();
        }

        protected void ApplyCurrentUnitFormation()
        {
            Vector3 leftDirection = Vector3.Cross(lineEndPos - lineStartPos, Vector3.up);
            float _length = (lineStartPos - lineEndPos).magnitude;
            _length = Mathf.Clamp(_length, 0.001f, float.MaxValue);

            float normalizedRemained = this._unitsPerRowRemained / _length;

            Vector3 middlePos = Vector3.LerpUnclamped(lineStartPos, lineEndPos, 0.5f - normalizedRemained);
 
            List<Vector3> posList;
            float facingAngle;

            if (_length > MAX_FORMABLE_THRESHOLD)
            {
                facingAngle = Mathf.Atan2(leftDirection.x, leftDirection.z) * Mathf.Rad2Deg;
                float facingAngleDiff = Mathf.Abs(facingAngle - CurrentFacingAngle);

                posList = FormationPositionerEx.GetAlignedPositionList(UnitHandlerList.Count, CurrentFormation, middlePos, facingAngle);

                if (facingAngleDiff < 90 || facingAngleDiff > (360 - 90))
                {
                    UnitFormationHelper.SetPositions_ShortestByIndex_Desc(this, posList, facingAngle);
                }
                else
                {
                    UnitFormationHelper.SetPositions_ShortestByIndex_Asc(this, posList, facingAngle);
                }
            }
            else
            {
                List<Vector3> currentPosList = new List<Vector3>();
                for (int i = 0; i < UnitHandlerList.Count; i++)
                {
                    currentPosList.Add(UnitHandlerList[i].transform.position);
                }

                (posList, facingAngle) = FormationPositionerEx.GetPositionListAndAngle(currentPosList, CurrentFormation, lineStartPos);
                float facingAngleDiff = Mathf.Abs(facingAngle - CurrentFacingAngle);

                if (facingAngleDiff < _MAX_FORMABLE_ANGLE || facingAngleDiff > (360 - _MAX_FORMABLE_ANGLE))
                {
                    for (int i = 0; i < UnitHandlerList.Count; i++)
                    {
                        Vector3 pos = posList[i] + UnitFormationHelper.GetNoise(NoiseAmount);
                        UnitHandlerList[i].SetTargetDestination(pos, facingAngle);
                    }
                }
                else
                {
                    UnitFormationHelper.SetPositions_ShortestByIndex_Asc(this, posList, facingAngle);
                }
            }

            CurrentFacingAngle = facingAngle;

            UniTaskEx.Cancel(this, 0);
            YieldPositionRoutine().Forget();
        }

        public void ChangeUnitFormation(FormationType type)
        {
            CurrentFormationType = type;
            _unitSpacing = _UnitInfo.GetPairValue(type).UnitSpacing;
            _noiseAmount = _UnitInfo.GetPairValue(type).NoiseAmount;

            if (type == FormationType.Rectangle)
            {
                CurrentFormation = new RectangleFormation(this);
            }
            else if (type == FormationType.Skirmish)
            {
                CurrentFormation = new RectangleFormation(this);
            }
            else if (type == FormationType.Square)
            {
                CurrentFormation = new SquareFormation(this);
            }
            else if (type == FormationType.Circle)
            {
                CurrentFormation = new CircleFormation(this);
            }
            else if (type == FormationType.Line)
            {
                CurrentFormation = new LineFormation(this);
            }
            else if (type == FormationType.Triangle)
            {
                CurrentFormation = new TriangleFormation(this);
            }
            else if (type == FormationType.Cone)
            {
                CurrentFormation = new ConeFormation(this);
            }
            else
            {
                Debug.Assert(false);
            }

            OnUnitFormationChanged?.Invoke();
        }

        protected void ReinstantiateFormation()
        {
            if (_currentFormation is RectangleFormation)
            {
                _currentFormation = new RectangleFormation(this);
            }
            else if (_currentFormation is LineFormation)
            {
                _currentFormation = new LineFormation(this);
            }
            else if (_currentFormation is CircleFormation)
            {
                _currentFormation = new CircleFormation(this);
            }
            else if (_currentFormation is TriangleFormation)
            {
                _currentFormation = new TriangleFormation(this);
            }
            else if (_currentFormation is ConeFormation)
            {
                _currentFormation = new ConeFormation(this);
            }
        }

        public Vector3 GetMiddlePos()
        {
            Vector3 middlePos = Vector3.zero;
            foreach (UnitHandler unit in UnitHandlerList)
            {
                middlePos += unit.transform.position;
            }

            return middlePos / UnitHandlerList.Count;
        }

#if UNITY_EDITOR
        protected List<KeyValuePair<Vector3, float>> gizmoSphereList = new List<KeyValuePair<Vector3, float>>();
        protected List<KeyValuePair<Vector3, Vector3>> startAndDestList = new List<KeyValuePair<Vector3, Vector3>>();
        protected List<KeyValuePair<Vector3, Vector3>> yieldedStartAndDestList = new List<KeyValuePair<Vector3, Vector3>>();

        protected async UniTaskVoid DrawSphereGizmoRoutine(KeyValuePair<Vector3, float> pair)
        {
            gizmoSphereList.Add(pair);
            await UniTask.WaitForSeconds(0.1f);
            gizmoSphereList.Remove(pair);
        }

        protected async UniTaskVoid DrawStartAndDestGizmoRoutine(KeyValuePair<Vector3, Vector3> pair)
        {
            startAndDestList.Add(pair);
            await UniTask.WaitForSeconds(0.1f);
            startAndDestList.Remove(pair);
        }

        protected async UniTaskVoid DrawYieldedStartAndDestGizmoRoutine(KeyValuePair<Vector3, Vector3> pair)
        {
            yieldedStartAndDestList.Add(pair);
            await UniTask.WaitForSeconds(0.1f);
            yieldedStartAndDestList.Remove(pair);
        }

        protected void OnDrawGizmos()
        {
            if (Application.isPlaying == true)
            {
                foreach (KeyValuePair<Vector3, float> pair in gizmoSphereList)
                {
                    Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
                    Gizmos.DrawSphere(pair.Key, pair.Value);
                }

                foreach (KeyValuePair<Vector3, Vector3> pair in startAndDestList)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(pair.Key, pair.Value);
                }

                foreach (KeyValuePair<Vector3, Vector3> pair in yieldedStartAndDestList)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(pair.Key, pair.Value);
                }
            }
        }
#endif
    }
}