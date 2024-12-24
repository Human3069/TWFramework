using _TW_Framework.Internal;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TW_Framework
{
    [ExecuteAlways]
    public class MovableCameraHandler : MonoBehaviour
    {
        [SerializeField]
        protected Transform targetParentT;
        [SerializeField]
        protected Transform targetChildT;
        [SerializeField]
        protected Transform targetSmoothT;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float distanceBetweenOrigin;

        [Space(10)]
        [SerializeField]
        protected float moveSpeed = 1.0f;
        [SerializeField]
        protected float moveLerpThreshold = 1.0f;

        [Space(10)]
        [SerializeField]
        protected float rotateSpeed = 1.0f;
        [SerializeField]
        protected float rotateLerpThreshold = 1.0f;

        [Space(10)]
        [SerializeField]
        protected float accelerationMultiplier = 2f;

        [Space(10)]
        [SerializeField]
        protected Vector2Int mouseZoomRange = new Vector2Int(1, 30);

        [ReadOnly]
        [SerializeField]
        protected int _currentMouseZoom;
        protected int CurrentMouseZoom
        {
            get
            {
                return _currentMouseZoom;
            }
            set
            {
                if (_currentMouseZoom != value)
                {
                    _currentMouseZoom = Mathf.Clamp(value, mouseZoomRange.x, mouseZoomRange.y);
                    currentMouseZoomNormal = Mathf.InverseLerp(mouseZoomRange.x, mouseZoomRange.y, _currentMouseZoom);

                    Vector3 zoomEndPos = targetParentT.position + targetChildT.transform.forward * zoomOutDistance;
                    targetChildT.transform.position = Vector3.Lerp(targetParentT.position, zoomEndPos, currentMouseZoomNormal);
                }
            }
        }

        [ReadOnly]
        [SerializeField]
        protected float currentMouseZoomNormal;
        [SerializeField]
        protected float zoomOutDistance;

        protected void Awake()
        {
            if (Application.isPlaying == true)
            {
                GetInputAsync().Forget();
            }
        }

        protected void Update()
        {
            if (Application.isPlaying == false)
            {
                distanceBetweenOrigin = Vector3.Distance(targetParentT.position, targetChildT.transform.position);
            }
        }

        protected async UniTaskVoid GetInputAsync()
        {
            await UniTask.WaitUntil(() => KeyInputManager.Instance != null);

            CurrentMouseZoom = mouseZoomRange.x;

            while (this.enabled == true && Application.isPlaying == true)
            {
                HandleKeyInput();
                HandleMouseWheelInput();

                await UniTask.Yield(this.destroyCancellationToken);
            }
        }

        protected void HandleKeyInput()
        {
            float moveDelta = moveSpeed * Time.unscaledDeltaTime;
            float moveSmoothDelta = moveLerpThreshold * Time.unscaledDeltaTime;

            float rotateDelta = rotateSpeed * Time.unscaledDeltaTime;
            float rotateSmoothDelta = rotateLerpThreshold * Time.unscaledDeltaTime;

            bool isMoveForward = KeyType.Move_Forward.IsOn();
            bool isMoveBackward = KeyType.Move_Backward.IsOn();
            bool isMoveLeft = KeyType.Move_Left.IsOn();
            bool isMoveRight = KeyType.Move_Right.IsOn();

            bool isTurnLeft = KeyType.Turn_Left.IsOn();
            bool isTurnRight = KeyType.Turn_Right.IsOn();

            bool isAccelerated = KeyType.Accelerate_Move.IsOn();
            if (isAccelerated == true)
            {
                moveDelta *= accelerationMultiplier;
            }

            float verticalDelta = 0f;
            float horizontalDelta = 0f;
            if (isMoveForward == true)
            {
                verticalDelta = moveDelta;
            }
            else if (isMoveBackward == true)
            {
                verticalDelta = -moveDelta;
            }

            if (isMoveLeft == true)
            {
                horizontalDelta = -moveDelta;
            }
            else if (isMoveRight == true)
            {
                horizontalDelta = moveDelta;
            }

            if (isTurnLeft == true)
            {
                Physics.Raycast(targetChildT.position, targetChildT.forward, out RaycastHit hit, Mathf.Infinity);
                targetParentT.RotateAround(hit.point, Vector3.up, -rotateDelta);
            }
            else if (isTurnRight == true)
            {
                Physics.Raycast(targetChildT.position, targetChildT.forward, out RaycastHit hit, Mathf.Infinity);
                targetParentT.RotateAround(hit.point, Vector3.up, rotateDelta);
            }

            targetParentT.Translate(horizontalDelta, 0f, verticalDelta);

            targetSmoothT.position = Vector3.Lerp(targetSmoothT.position, targetChildT.position, moveSmoothDelta);
            targetSmoothT.rotation = Quaternion.Lerp(targetSmoothT.rotation, targetChildT.rotation, rotateSmoothDelta);
        }

        protected void HandleMouseWheelInput()
        {
            CurrentMouseZoom += (int)Input.mouseScrollDelta.y;
        }
    }
}