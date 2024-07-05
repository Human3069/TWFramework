using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public class SilhouetteController : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[SilhouetteController]</b></color> {0}";

        [SerializeField]
        protected MouseEventHandler _mouseEventHandler;
        [SerializeField]
        protected FormationController _controller;

        [Space(10)]
        [SerializeField]
        protected GameObject silhouettePrefab;
        [ReadOnly]
        [SerializeField]
        protected List<GameObject> silhouetteObjList = new List<GameObject>(); 

        protected bool isEventOn;

        protected void Awake()
        {
            _mouseEventHandler.OnStartHandling += OnStartHandling;
            _mouseEventHandler.OnDuringHandling += OnDuringHandling;
            _mouseEventHandler.OnEndHandling += OnEndHandling;
            _controller.OnUnitCountChanged += OnUnitCountChanged;
   
            for (int i = 0; i < _controller.UnitHandlerList.Count; i++)
            {
                GameObject _obj = Instantiate(silhouettePrefab);
                _obj.SetActive(false);
                _obj.transform.parent = this.transform;

                silhouetteObjList.Add(_obj);
            }
        }

        protected void OnDestroy()
        {
            _controller.OnUnitCountChanged -= OnUnitCountChanged;
            _mouseEventHandler.OnEndHandling -= OnEndHandling;
            _mouseEventHandler.OnDuringHandling -= OnDuringHandling;
            _mouseEventHandler.OnStartHandling -= OnStartHandling;
        }

        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) == true)
            {
                isEventOn = !isEventOn;
                SetSilhouettesActive(isEventOn);
            }
        }

        protected void OnUnitCountChanged(int unitCount)
        {
            if (silhouetteObjList.Count < unitCount)
            {
                for (int i = silhouetteObjList.Count; i < unitCount; i++)
                {
                    GameObject _obj = Instantiate(silhouettePrefab, transform.position, Quaternion.identity);
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
            SetSilhouettesActive(true);

            List<Vector3> currentPosList = new List<Vector3>();
            List<Vector3> posList;
            float facingAngle;

            for (int i = 0; i < silhouetteObjList.Count; i++)
            {
                currentPosList.Add(silhouetteObjList[i].transform.position);
            }

            (posList, facingAngle) = FormationPositionerEx.GetPositionListAndAngle(currentPosList, _controller.CurrentFormation, lineStartPos);

            for (int i = 0; i < silhouetteObjList.Count; i++)
            {
                silhouetteObjList[i].transform.position = posList[i];
                silhouetteObjList[i].transform.eulerAngles = new Vector3(0f, facingAngle, 0f);
            }
        }

        protected void OnDuringHandling(Vector3 lineStartPos, Vector3 lineEndPos, float lineLength)
        {
            Vector3 leftDirection = Vector3.Cross(lineEndPos - lineStartPos, Vector3.up);
            float _length = (lineStartPos - lineEndPos).magnitude;
            _length = Mathf.Clamp(_length, 0.001f, float.MaxValue);

            if (_length > FormationController.MAX_FORMABLE_THRESHOLD)
            {
                float normalizedRemained = _controller.UnitsPerRowRemained / _length;
                Vector3 middlePos = Vector3.LerpUnclamped(lineStartPos, lineEndPos, 0.5f - normalizedRemained);

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