using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace _TW_Framework
{
    public class UnitHandler : MonoBehaviour
    {
        protected NavMeshAgent _navMeshAgent;

        protected float facingAngle = 0f;
        protected bool faceOnDestination = false;

        [SerializeField, Tooltip("Speed with which the unit will rotate towards the formation facing angle.")]
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

#if true
        private void OnDrawGizmos()
        {
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = Color.red;

            Vector3 labelPos = this.transform.position;

            string title = TargetColumn.ToString();
            Handles.Label(labelPos, title, labelStyle);
        }
#endif
    }
}