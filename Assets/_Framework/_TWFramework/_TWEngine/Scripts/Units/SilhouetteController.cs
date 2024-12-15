using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _TW_Framework
{
    public class SilhouetteController : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[SilhouetteController]</b></color> {0}";

        protected MouseEventHandler _mouseEventHandler;
        protected PlayerFormationController _controller;
        protected GameObject _silhouettePrefab;

        [ReadOnly]
        [SerializeField]
        protected List<GameObject> silhouetteObjList = new List<GameObject>(); 

        protected bool isEventOn;

        public void Initialize(PlayerFormationController controller, MouseEventHandler mouseEvent, GameObject silhouettePrefab, List<Vector3> posList, float facingAngle)
        {
            _controller = controller;
            _mouseEventHandler = mouseEvent;
            _silhouettePrefab = silhouettePrefab;

            _controller.OnUnitCountChanged += OnUnitCountChanged;

            _controller.OnSelectedAction += OnSelected;
            _controller.OnDeselectedAction += OnDeselected;

            for (int i = 0; i < _controller.UnitHandlerList.Count; i++)
            {
                GameObject _obj = Instantiate(_silhouettePrefab);
                _obj.SetActive(false);
                _obj.transform.parent = this.transform;

                silhouetteObjList.Add(_obj);
            }

            for (int i = 0; i < silhouetteObjList.Count; i++)
            {
                silhouetteObjList[i].transform.position = posList[i];
                silhouetteObjList[i].transform.eulerAngles = new Vector3(0f, facingAngle, 0f);
            }
        }

        protected void OnDestroy()
        {
            _controller.OnDeselectedAction -= OnDeselected;
            _controller.OnSelectedAction -= OnSelected;

            _controller.OnUnitCountChanged -= OnUnitCountChanged;
        }

        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) == true)
            {
                isEventOn = !isEventOn;
                SetSilhouettesActive(isEventOn);
            }
        }

        protected void OnSelected()
        {
            _mouseEventHandler.OnStartHandling += OnStartHandling;
            _mouseEventHandler.OnDuringHandling += OnDuringHandling;
            _mouseEventHandler.OnEndHandling += OnEndHandling;
        }

        protected void OnDeselected()
        {
            _mouseEventHandler.OnEndHandling -= OnEndHandling;
            _mouseEventHandler.OnDuringHandling -= OnDuringHandling;
            _mouseEventHandler.OnStartHandling -= OnStartHandling;
        }

        protected void OnUnitCountChanged(int unitCount)
        {
            if (silhouetteObjList.Count < unitCount)
            {
                for (int i = silhouetteObjList.Count; i < unitCount; i++)
                {
                    GameObject _obj = Instantiate(_silhouettePrefab, transform.position, Quaternion.identity);
                    _obj.SetActive(false);
                    _obj.transform.parent = this.transform;

                    silhouetteObjList.Insert(i, _obj);
                }
            }
            else if (silhouetteObjList.Count > unitCount)
            {
                for (int i = silhouetteObjList.Count - 1; i >= unitCount; i--)
                {
                    GameObject _obj = silhouetteObjList[i].gameObject;
                    silhouetteObjList.RemoveAt(i);

                    Destroy(_obj);
                }
            }
        }

        protected void OnStartHandling(Vector3 lineStartPos, Vector3 lineEndPos)
        {
            (Vector3 controlStartPos, Vector3 controlEndPos) = _controller.GetControlPoints(lineStartPos, lineEndPos);

            SetSilhouettesActive(true);

            List<Vector3> currentPosList = new List<Vector3>();
            List<Vector3> posList;
            float facingAngle;

            for (int i = 0; i < silhouetteObjList.Count; i++)
            {
                currentPosList.Add(silhouetteObjList[i].transform.position);
            }

            (posList, facingAngle) = FormationPositionerEx.GetPositionListAndAngle(currentPosList, _controller.CurrentFormation, controlStartPos);

            for (int i = 0; i < silhouetteObjList.Count; i++)
            {
                silhouetteObjList[i].transform.position = posList[i];
                silhouetteObjList[i].transform.eulerAngles = new Vector3(0f, facingAngle, 0f);
            }
        }

        protected void OnDuringHandling(Vector3 lineStartPos, Vector3 lineEndPos)
        {
            (Vector3 controlStartPos, Vector3 controlEndPos) = _controller.GetControlPoints(lineStartPos, lineEndPos);

            Vector3 leftDirection = Vector3.Cross(controlEndPos - controlStartPos, Vector3.up);
            float _length = (controlStartPos - controlEndPos).magnitude;
            _length = Mathf.Clamp(_length, 0.001f, float.MaxValue);

            if (_length > BaseFormationController.MAX_FORMABLE_THRESHOLD)
            {
                float normalizedRemained = _controller.UnitsPerRowRemained / _length;
                Vector3 middlePos = Vector3.LerpUnclamped(controlStartPos, controlEndPos, 0.5f - normalizedRemained);

                float facingAngle = Mathf.Atan2(leftDirection.x, leftDirection.z) * Mathf.Rad2Deg;
                List<Vector3> posList = FormationPositionerEx.GetAlignedPositionList(silhouetteObjList.Count, _controller.CurrentFormation, middlePos, facingAngle);

                for (int i = 0; i < silhouetteObjList.Count; i++)
                {
                    silhouetteObjList[i].transform.position = posList[i];
                    silhouetteObjList[i].transform.eulerAngles = new Vector3(0f, facingAngle, 0f);
                }
            }
        }

        protected void OnEndHandling(Vector3 lineStartPos, Vector3 lineEndPos)
        {
            if (isEventOn == false)
            {
                SetSilhouettesActive(false);
            }
        }

        protected void SetSilhouettesActive(bool isActive)
        {
            for (int i = 0; i < silhouetteObjList.Count; i++)
            {
                silhouetteObjList[i].SetActive(isActive);
            }
        }
    }
}