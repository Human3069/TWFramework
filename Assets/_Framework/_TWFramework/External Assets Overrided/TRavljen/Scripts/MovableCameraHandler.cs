using _TW_Framework.Internal;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TW_Framework
{
    public class MovableCameraHandler : MonoBehaviour
    {
        [SerializeField]
        protected Camera _camera;

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

                    Vector3 zoomEndPos = this.transform.position + _camera.transform.forward * zoomOutDistance;
                    _camera.transform.position = Vector3.Lerp(this.transform.position, zoomEndPos, currentMouseZoomNormal);
                }
            }
        }

        [ReadOnly]
        [SerializeField]
        protected float currentMouseZoomNormal;
        [SerializeField]
        protected float zoomOutDistance;

        protected Vector3 requiredPos;
        protected Quaternion requiredRot;

        protected void Awake()
        {
            GetInputAsync().Forget();
        }

        protected async UniTaskVoid GetInputAsync()
        {
            await UniTask.WaitUntil(() => KeyInputManager.Instance != null);

            requiredPos = this.transform.position;
            requiredRot = this.transform.rotation;

            CurrentMouseZoom = mouseZoomRange.x;

            while (this.gameObject.activeSelf == true)
            {
                HandleKeyInput();
                HandleMouseWheelInput();

                await UniTask.Yield();
            }
        }

        protected void HandleKeyInput()
        {
            float moveDelta = moveSpeed * Time.deltaTime;
            float moveSmoothDelta = moveLerpThreshold * Time.deltaTime;

            float rotateDelta = rotateSpeed * Time.deltaTime;
            float rotateSmoothDelta = rotateLerpThreshold * Time.deltaTime;

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
                rotateDelta *= accelerationMultiplier;
            }

            if (isMoveForward == true)
            {
                requiredPos += this.transform.forward * moveDelta;
            }
            else if (isMoveBackward == true)
            {
                requiredPos -= this.transform.forward * moveDelta;
            }

            if (isMoveLeft == true)
            {
                requiredPos -= this.transform.right * moveDelta;
            }
            else if (isMoveRight == true)
            {
                requiredPos += this.transform.right * moveDelta;
            }

            if (isTurnLeft == true)
            {
                requiredRot *= Quaternion.Euler(0, -rotateDelta, 0);
            }
            else if (isTurnRight == true)
            {
                requiredRot *= Quaternion.Euler(0, rotateDelta, 0);
            }

            this.transform.position = Vector3.Lerp(this.transform.position, requiredPos, moveSmoothDelta);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, requiredRot, rotateSmoothDelta);
        }

        protected void HandleMouseWheelInput()
        {
            CurrentMouseZoom += (int)Input.mouseScrollDelta.y;
        }
    }
}