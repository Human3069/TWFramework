using System.Collections.Generic;
using TRavljen.UnitFormation.Formations;
using TRavljen.UnitFormation;
using UnityEngine;
using System;
using System.Collections;

namespace _TW_Framework
{
    public class FormationController : MonoBehaviour
    {
        public const float MAX_FORMABLE_THRESHOLD = 0.8f;

        private const float _MAX_FORMABLE_ANGLE = 120;
        private const string _LOG_FORMAT = "<color=white><b>[FormationController]</b></color> {0}";

        [SerializeField]
        protected MouseEventHandler _mouseEventHandler;

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
                            _unit.Initialize(this);

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

        protected void Awake()
        {
            _mouseEventHandler.OnDuringHandling += OnDuringHandling;
            _mouseEventHandler.OnEndHandling += OnEndHandling;
        }

        protected void OnDestroy()
        {
            _mouseEventHandler.OnEndHandling -= OnEndHandling;
            _mouseEventHandler.OnDuringHandling -= OnDuringHandling;
        }

        protected void Start()
        {
            lineStartPos = new Vector3(-3f, 0f, 0f);
            lineEndPos = new Vector3(3f, 0f, 0f);

            _unitCount = 7;
            _unitSpacing = 2f;
            _unitsPerRow = 4;
            formedUnitsPerColumn = 2;
            lastColumnCount = 3;
            _noiseAmount = 0f;

            CurrentFormation = new RectangleFormation((int)UnitsPerRow, UnitSpacing, true, IsPivotInMiddle);
            foreach (UnitHandler unit in UnitHandlerList)
            {
                unit.Initialize(this);
            }

            ApplyCurrentUnitFormation();
        }

        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                YieldPosition();
            }
        }

        private const float MAX_YIELDABLE_DISTANCE = 2.5f;

        protected void YieldPosition()
        {
            List<UnitHandler> movingUnitList = UnitHandlerList.FindAll(x => x.IsStopped == false);

            for (int i = 0; i < movingUnitList.Count; i++)
            {
                Vector3 middlePos = (movingUnitList[i].TargetPos + movingUnitList[i].transform.position) / 2f;

                float nearestDistance = float.MaxValue;
                UnitHandler nearestUnit = null;

                Collider[] foundColliders = Physics.OverlapSphere(middlePos, MAX_YIELDABLE_DISTANCE * UnitSpacing);
                for (int j = 0; j < foundColliders.Length; j++)
                {
                    if (foundColliders[j].TryGetComponent<UnitHandler>(out UnitHandler overlappedHandler) == true)
                    {
                        if (overlappedHandler.IsStopped == false ||
                            overlappedHandler == movingUnitList[i])
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
                        movingUnitList[i].SetTargetDestination(yieldedPos, currentFacingAngle);
                    }
                }
            }

            Debug.Log("yielded");
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
            ApplyMouseFormationing(lineStartPos, lineEndPos);
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

        protected float currentFacingAngle;

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
    }
}