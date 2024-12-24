using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _TW_Framework
{
    [RequireComponent(typeof(MeshCollider))]
    public class MouseEventHandler : MonoBehaviour
    {
        [SerializeField]
        protected Camera _camera;
        [SerializeField]
        protected LineRenderer _lineRenderer;
        [SerializeField]
        protected GraphicRaycaster graphicRaycaster;

        [Space(10)]
        [SerializeField]
        protected LayerMask groundLayerMask;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected Vector3 lineStartPos;
        [ReadOnly]
        [SerializeField]
        protected Vector3 lineEndPos;
        [ReadOnly]
        [SerializeField]
        protected float lineLength;

        protected bool _isSelecting = false;
        public bool IsSelecting
        {
            get
            {
                return _isSelecting;
            }
            protected set
            {
                _isSelecting = value;
            }
        }

        protected bool _isHandling = false;
        public bool IsHandling
        {
            get
            {
                return _isHandling;
            }
            protected set
            {
                _isHandling = value;
            }
        }

        [Space(10)]
        [SerializeField]
        protected List<PlayerFormationController> selectedControllerList = new List<PlayerFormationController>();

        protected MeshCollider selectionMeshCollider;

        public Action<BaseFormationController> OnClickEnemyHandler;

        public Action<Vector3, Vector3> OnStartHandling;
        public Action<Vector3, Vector3> OnDuringHandling;
        public Action<Vector3, Vector3> OnEndHandling;

        public Action OnStartMouseSelect;
        public Action<List<PlayerFormationController>> OnDuringMouseSelect;
        public Action<List<PlayerFormationController>> OnEndMouseSelect;

        protected void Start()
        {
            _lineRenderer.enabled = false;
            selectionMeshCollider = this.GetComponent<MeshCollider>();

            Debug.Assert(selectionMeshCollider.convex == true && selectionMeshCollider.isTrigger == true);
        }

        protected void FixedUpdate()
        {
            if (Input.GetMouseButton(0) == true)
            {
                selectedControllerList.Clear();
            }
        }

        protected void Update()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> raycastResultList = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, raycastResultList);

            // UI 호버링 중일 땐 MouseEventHandler 동작하지 않게 막음.
            if (raycastResultList.Count > 0)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                PostOnLeftClicked().Forget();
            }

            if (Input.GetMouseButtonDown(1) == true)
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out RaycastHit hit);
                Collider[] overlappedColliders = Physics.OverlapSphere(hit.point, 1f);
                UnitHandler overlappedUnit = null;

                foreach (Collider overlappedCollider in overlappedColliders)
                {
                    if (overlappedCollider.TryGetComponent<UnitHandler>(out overlappedUnit) == true)
                    {
                        if (overlappedUnit._TeamType == TeamType.Enemy)
                        {
                            break;
                        }
                        else
                        {
                            overlappedUnit = null;
                        }
                    }
                }

                if (overlappedUnit == null)
                {
                    IsHandling = true;
                    _lineRenderer.enabled = true;

                    PostOnRightClicked(ray).Forget();
                }
                else
                {
                    if (OnClickEnemyHandler != null)
                    {
                        OnClickEnemyHandler(overlappedUnit.Controller);
                    }
                }
            }
        }

        protected async UniTaskVoid PostOnLeftClicked()
        {
            IsSelecting = true;

            Vector2 startScreenPoint = Input.mousePosition;
            Ray ray = _camera.ScreenPointToRay(startScreenPoint);
            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask);
            Vector3 startPos = hit.point;

            selectedControllerList.Clear();
            OnStartMouseSelect?.Invoke();

            while (Input.GetMouseButton(0) == true)
            {
                Vector2 endScreenPoint = Input.mousePosition;
                Pyramid selectionPyramid = new Pyramid(_camera, 2000f, startScreenPoint, endScreenPoint);

                Mesh pyramidMesh = selectionPyramid.GenerateMesh();
                selectionMeshCollider.sharedMesh = pyramidMesh;
                
                OnDuringMouseSelect?.Invoke(selectedControllerList);

                await UniTask.Yield(this.destroyCancellationToken);
            }

            selectionMeshCollider.sharedMesh = null;

            selectedControllerList.Sort(ComparisionFromDistance);
            int ComparisionFromDistance(PlayerFormationController x, PlayerFormationController y)
            {
                float xDistance = (startPos - x.GetMiddlePos()).sqrMagnitude;
                float yDistance = (startPos - y.GetMiddlePos()).sqrMagnitude;
                if (xDistance > yDistance)
                {
                    return 1;
                }
                else if (xDistance < yDistance)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }

            OnEndMouseSelect?.Invoke(selectedControllerList);
            IsSelecting = false;
        }

        protected void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent<UnitHandler>(out UnitHandler unit) == true)
            {
                if (unit._TeamType == TeamType.Player)
                {
                    PlayerFormationController playerController = unit.Controller as PlayerFormationController;
                    if (selectedControllerList.Contains(playerController) == false)
                    {
                        selectedControllerList.Add(playerController);
                    }
                }
            }
        }

        protected async UniTaskVoid PostOnRightClicked(Ray ray)
        {
            if (Physics.Raycast(ray, out RaycastHit _hit, Mathf.Infinity, groundLayerMask))
            {
                lineStartPos = _hit.point;
                lineEndPos = _hit.point;
            }
            _lineRenderer.SetPosition(0, lineStartPos + Vector3.up * 0.075f);

            if (OnStartHandling != null)
            {
                OnStartHandling(lineStartPos, lineEndPos);
            }

            while (Input.GetMouseButton(1) == true)
            {
                ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out _hit, Mathf.Infinity, groundLayerMask))
                {
                    lineEndPos = _hit.point;
                    _lineRenderer.SetPosition(1, lineEndPos + Vector3.up * 0.075f);
                }

                if (OnDuringHandling != null)
                {
                    OnDuringHandling(lineStartPos, lineEndPos);
                }

                await UniTask.NextFrame(destroyCancellationToken);
            }

            IsHandling = false;

            if (OnEndHandling != null)
            {
                OnEndHandling(lineStartPos, lineEndPos);
            }
        }
    }
}