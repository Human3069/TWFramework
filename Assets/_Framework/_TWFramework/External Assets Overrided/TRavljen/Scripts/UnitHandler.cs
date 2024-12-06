using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace _TW_Framework
{
    public class UnitHandler : MonoBehaviour
    {
        protected NavMeshAgent _navMeshAgent;
        protected FormationController _controller;

        protected float facingAngle = 0f;
        protected bool faceOnDestination = false;

        protected float rotationSpeed = 100;

        [HideInInspector]
        public bool FacingRotationEnabled = true;
        
        [ReadOnly]
        public int TargetColumn = -1;

        public bool IsWithinStoppingDistance
        {
            get
            {
                return Vector3.Distance(transform.position, _navMeshAgent.destination) <= _navMeshAgent.stoppingDistance;
            }
        }

        protected bool _isStopped = false;
        public bool IsStopped
        {
            get
            {
                return _isStopped;
            }
            protected set
            {
                if (_isStopped != value)
                {
                    _isStopped = value;

                    if (value == true)
                    {
                        IsYielding = false;
                    }
                }
            }
        }

        public Vector3 TargetPos
        {
            get
            {
                return _navMeshAgent.destination;
            }
        }

        [HideInInspector]
        public bool IsYielding = false;

        public void Initialize(FormationController controller)
        {
            this._controller = controller;
        }

        protected void Start()
        {
            _navMeshAgent = this.GetComponent<NavMeshAgent>();
        }

        protected void Update()
        {
            if (Vector3.Distance(_navMeshAgent.destination, transform.position) < _navMeshAgent.stoppingDistance && faceOnDestination == true)
            {
                float currentAngle = transform.rotation.eulerAngles.y;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, facingAngle, rotationSpeed * Time.deltaTime);

                this.transform.rotation = Quaternion.AngleAxis(newAngle, Vector3.up);
            }

            IsStopped = Vector3.Distance(_navMeshAgent.destination, this.transform.position) < _navMeshAgent.stoppingDistance;
        }

        public void SetTargetDestination(Vector3 newTargetDestination, float newFacingAngle)
        {
            if (_navMeshAgent == null)
            {
                _navMeshAgent = this.GetComponent<NavMeshAgent>();
            }

            faceOnDestination = true;

            _navMeshAgent.destination = newTargetDestination;
            facingAngle = newFacingAngle;
        }
    }
}