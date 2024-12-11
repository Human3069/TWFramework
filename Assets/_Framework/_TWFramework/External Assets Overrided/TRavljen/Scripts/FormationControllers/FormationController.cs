using System.Collections.Generic;
using TRavljen.UnitFormation.Formations;
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

    public class FormationController : MonoBehaviour
    {
        public const float MAX_FORMABLE_THRESHOLD = 0.8f;
        private const float _MAX_FORMABLE_ANGLE = 120;

        private const string _LOG_FORMAT = "<color=white><b>[FormationController]</b></color> {0}";

        [Header("=== FormationController ===")]
        [SerializeField]
        protected TeamType teamType;
        [ReadOnly]
        [SerializeField]
        protected FormationController _targetController;
        protected FormationController TargetController
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

        [Space(10)]
        [SerializeField]
        protected Transform initialLineStartT;
        [SerializeField]
        protected Transform initialLineEndT;

        [Space(10)]
        [SerializeField]
        private GameObject unitPrefab = null;

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
                            GameObject _obj = Instantiate(unitPrefab, transform.position, Quaternion.identity);
                            UnitHandler _unit = _obj.GetComponent<UnitHandler>();
                            _unit.Initialize(this, teamType);

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

                            Destroy(_obj);
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
            Destroy(unitHandler.gameObject);

            UnitCount = UnitHandlerList.Count;
            ApplyCurrentUnitFormation();
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

        protected Vector3 lineStartPos;
        protected Vector3 lineEndPos;

        protected float currentFacingAngle;

        public delegate void Initialized(Vector3 startPos, Vector3 endPos);
        public event Initialized OnControllerInitialized;

        protected virtual void Start()
        {
            foreach (UnitHandler unit in UnitHandlerList)
            {
                unit.Initialize(this, teamType);
            }

            lineStartPos = initialLineStartT.position;
            lineEndPos = initialLineEndT.position;
            float lineLength = (lineStartPos - lineEndPos).magnitude;

            _unitCount = UnitHandlerList.Count;
            _unitSpacing = 2f;
            _noiseAmount = 0f;

            float _unitsPerRowUnclamped = lineLength / UnitSpacing;
            int _unitsPerRow = (int)(lineLength / UnitSpacing);
            this._unitsPerRowRemained = _unitsPerRowUnclamped - _unitsPerRow;

            _unitsPerRow = Mathf.Clamp(_unitsPerRow, 1, int.MaxValue);
            this.UnitsPerRow = _unitsPerRow;

            CurrentFormation = new RectangleFormation((int)UnitsPerRow, UnitSpacing, true, IsPivotInMiddle);

            PostStart().Forget();
        }

        protected async UniTaskVoid PostStart()
        {
            await UniTask.Yield();

            if (OnControllerInitialized != null)
            {
                OnControllerInitialized(lineStartPos, lineEndPos);
            }
        }

        protected async UniTask YieldPositionRoutine()
        {
            await UniTaskEx.WaitForSeconds(this, 0, 0.1f);

            List<UnitHandler> movingUnitList = UnitHandlerList.FindAll(x => x.IsStopped == false);
            while (movingUnitList.Count > 0)
            {
                movingUnitList = UnitHandlerList.FindAll(x => x.IsStopped == false);

                for (int i = 0; i < movingUnitList.Count; i++)
                {
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
                            nearestUnit.SetTargetDestination(movingUnitList[i].TargetPos, currentFacingAngle);
                            nearestUnit.IsYielding = true;
                            DrawYieldedStartAndDestGizmoRoutine(new KeyValuePair<Vector3, Vector3>(movingUnitList[i].TargetPos, nearestUnit.transform.position)).Forget();

                            movingUnitList[i].SetTargetDestination(yieldedPos, currentFacingAngle);
                            movingUnitList[i].IsYielding = true;
                            DrawYieldedStartAndDestGizmoRoutine(new KeyValuePair<Vector3, Vector3>(yieldedPos, movingUnitList[i].transform.position)).Forget();
                        }
                    }

                    await UniTask.Yield();
                }

                await UniTaskEx.WaitForSeconds(this, 0, yieldIterationInterval);
            }
        }

        public void ChangeUnitFormation(Type formationType)
        {
            if (formationType == typeof(RectangleFormation))
            {
                CurrentFormation = new RectangleFormation(UnitsPerRow, UnitSpacing, true, IsPivotInMiddle);
            }
            else if (formationType == typeof(CircleFormation))
            {
                CurrentFormation = new CircleFormation(UnitSpacing);
            }
            else if (formationType == typeof(LineFormation))
            {
                CurrentFormation = new LineFormation(UnitSpacing);
            }
            else if (formationType == typeof(TriangleFormation))
            {
                CurrentFormation = new TriangleFormation(UnitSpacing);
            }
            else if (formationType == typeof(ConeFormation))
            {
                CurrentFormation = new ConeFormation(UnitSpacing, IsPivotInMiddle);
            }
            else
            {
                Debug.Assert(false);
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
                float facingAngleDiff = Mathf.Abs(facingAngle - currentFacingAngle);

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
                float facingAngleDiff = Mathf.Abs(facingAngle - currentFacingAngle);

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

            currentFacingAngle = facingAngle;

            UniTaskEx.Cancel(this, 0);
            YieldPositionRoutine().Forget();
        }

        protected void ReinstantiateFormation()
        {
            if (_currentFormation is RectangleFormation)
            {
                _currentFormation = new RectangleFormation(UnitsPerRow, UnitSpacing, true, IsPivotInMiddle);
            }
            else if (_currentFormation is LineFormation)
            {
                _currentFormation = new LineFormation(UnitSpacing);
            }
            else if (_currentFormation is CircleFormation)
            {
                _currentFormation = new CircleFormation(UnitSpacing);
            }
            else if (_currentFormation is TriangleFormation)
            {
                _currentFormation = new TriangleFormation(UnitSpacing, pivotInMiddle: IsPivotInMiddle);
            }
            else if (_currentFormation is ConeFormation)
            {
                _currentFormation = new ConeFormation(UnitSpacing, IsPivotInMiddle);
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
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(initialLineStartT.position, initialLineEndT.position);
            }
        }
#endif
    }
}