using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public class MouseEventHandler : MonoBehaviour
    {
        [SerializeField]
        protected LineRenderer _lineRenderer;

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

        protected void Update()
        {
            if (Input.GetMouseButtonDown(1) == true)
            {
                IsHandling = true;
                _lineRenderer.enabled = true;

                StartCoroutine(PostOnClicked());
            }
        }

        protected IEnumerator PostOnClicked()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit _hit, 100, groundLayerMask))
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
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out _hit, 100, groundLayerMask))
                {
                    lineEndPos = _hit.point;
                    _lineRenderer.SetPosition(1, lineEndPos + Vector3.up * 0.075f);
                }
                lineLength = (lineStartPos - lineEndPos).magnitude;

                if (OnDuringHandling != null)
                {
                    OnDuringHandling(lineStartPos, lineEndPos, lineLength);
                }

                yield return null;
            }

            IsHandling = false;

            if (OnEndHandling != null)
            {
                OnEndHandling(lineStartPos, lineEndPos);
            }
        }
    }
}