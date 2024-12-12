using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _TW_Framework
{
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

        public delegate void ClickEnemyHandler(FormationController controller);
        public event ClickEnemyHandler OnClickEnemyHandler;


        public delegate void StartHandling(Vector3 lineStartPos, Vector3 lineEndPos);
        public event StartHandling OnStartHandling;

        public delegate void DuringHandling(Vector3 lineStartPos, Vector3 lineEndPos, float lineLength);
        public event DuringHandling OnDuringHandling;

        public delegate void EndHandling(Vector3 lineStartPos, Vector3 lineEndPos);
        public event EndHandling OnEndHandling;

        protected void Start()
        {
            _lineRenderer.enabled = false;
        }

        protected List<Vector3> gizmoPosList = new List<Vector3>();
        protected async UniTaskVoid DrawSphereForMoment(Vector3 pos)
        {
            gizmoPosList.Add(pos);
            await UniTask.WaitForSeconds(0.5f);
            gizmoPosList.Remove(pos);
        }

        protected void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
            foreach (Vector3 pos in gizmoPosList)
            {
                Gizmos.DrawSphere(pos, 10f);
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

            // Take Cast Sphere Damage
            if (Input.GetMouseButtonDown(0) == true)
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit) == true)
                {
                    float maxSplashRadius = 10f;
                    float maxDamage = 100f;

                    Collider[] overlappedColliders = Physics.OverlapSphere(hit.point, maxSplashRadius);
                    DrawSphereForMoment(hit.point).Forget();

                    foreach (Collider overlappedCollider in overlappedColliders)
                    {
                        if (overlappedCollider.TryGetComponent<IDamageable>(out IDamageable damageable) == true)
                        {
                            float distance = (hit.point - overlappedCollider.transform.position).magnitude;
                            float damage = Mathf.Lerp(maxDamage, 0f, distance / maxSplashRadius);

                            damageable.TakeDamage(damage);
                        }
                    }
                }
            }

            if (Input.GetMouseButtonDown(1) == true)
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out RaycastHit hit);

                if (hit.collider != null &&
                    hit.collider.TryGetComponent<UnitHandler>(out UnitHandler unit) == true &&
                    unit._TeamType == TeamType.Enemy)
                {
                    if (OnClickEnemyHandler != null)
                    {
                        OnClickEnemyHandler(unit.Controller);
                    }
                }
                else
                {
                    IsHandling = true;
                    _lineRenderer.enabled = true;

                    PostOnClicked(ray).Forget();
                }
            }
        }

        protected async UniTaskVoid PostOnClicked(Ray ray)
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
                lineLength = (lineStartPos - lineEndPos).magnitude;

                if (OnDuringHandling != null)
                {
                    OnDuringHandling(lineStartPos, lineEndPos, lineLength);
                }

                await UniTask.NextFrame();
            }

            IsHandling = false;

            if (OnEndHandling != null)
            {
                OnEndHandling(lineStartPos, lineEndPos);
            }
        }
    }
}