using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace _TW_Framework
{
    [RequireComponent(typeof(Animation))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class UnitHandler : MonoBehaviour, IDamageable
    {
        protected Animation _animation;
        protected NavMeshAgent _agent;

        [HideInInspector]
        public FormationController Controller;
        [HideInInspector]
        public TeamType _TeamType;

        [Header("=== IDamageable ===")]
        [SerializeField]
        protected float maxHealth = 100f;
        [ReadOnly]
        [SerializeField]
        protected float _currentHealth;
        public float CurrentHealth
        {
            get
            {
                return _currentHealth;
            }
            set
            {
                if (_currentHealth != value)
                {
                    _currentHealth = Mathf.Clamp(value, 0, maxHealth);
                }
            }
        }

        [Space(10)]
        [SerializeField]
        protected float meleeAttackDamage = 10f;
        [SerializeField]
        protected float meleeAttackSpeed = 1f;

        [Space(10)]
        [SerializeField]
        protected float rangedAttackDamage = 10f;
        [SerializeField]
        protected float rangedAttackSpeed = 1f;
        [SerializeField]
        protected float rangedAttackRange = 10f;
        [SerializeField]
        protected float rangedAttackAngle = 45f;

        [Space(10)]
        [ReadOnly]
        public UnitHandler MeleeAttackingUnit;
        [ReadOnly]
        public UnitHandler RangedAttackingUnit;

        protected float facingAngle = 0f;
        protected bool faceOnDestination = false;

        protected float rotationSpeed = 100;

        [HideInInspector]
        public bool FacingRotationEnabled = true;
        
        public bool IsWithinStoppingDistance
        {
            get
            {
                return Vector3.Distance(transform.position, _agent.destination) <= _agent.stoppingDistance;
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
                return _agent.destination;
            }
        }

        [HideInInspector]
        public bool IsYielding = false;
        [HideInInspector]
        public bool IsDead = false;

        protected void Awake()
        {
            _animation = this.GetComponent<Animation>();
            _agent = this.GetComponent<NavMeshAgent>();
        }

        public void Initialize(FormationController controller, TeamType teamType)
        {
            IsDead = false;

            this.Controller = controller;
            this._TeamType = teamType;

            CurrentHealth = maxHealth;
        }

        protected void OnEnable()
        {
            SeekToMeleeAttackAsync().Forget();
            SeekToRangeAttackAsync().Forget();
        }

        protected async UniTaskVoid SeekToMeleeAttackAsync()
        {
            while (MeleeAttackingUnit == null && IsDead == false)
            {
                Collider[] overlappedColliders = Physics.OverlapSphere(this.transform.position, 2f);
                foreach (Collider overlappedCollider in overlappedColliders)
                {
                    if (overlappedCollider.TryGetComponent<UnitHandler>(out UnitHandler unitHandler) == true)
                    {
                        if (unitHandler._TeamType != this._TeamType)
                        {
                            MeleeAttackingUnit = unitHandler;
                            MeleeAttackAsync().Forget();
                        
                            return;
                        }
                    }
                }

                await UniTask.WaitForSeconds(0.5f);
            }
        }

        protected async UniTaskVoid SeekToRangeAttackAsync()
        {
            while (RangedAttackingUnit == null && IsDead == false)
            {
                Collider[] overlappedColliders = Physics.OverlapSphere(this.transform.position, rangedAttackRange);
                foreach (Collider overlappedCollider in overlappedColliders)
                {
                    Vector3 directionToTarget = (overlappedCollider.transform.position - transform.position).normalized;
                    float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

                    if (angleToTarget < rangedAttackAngle * 0.5f)
                    {
                        if (overlappedCollider.TryGetComponent<UnitHandler>(out UnitHandler unitHandler) == true)
                        {
                            if (unitHandler._TeamType != this._TeamType)
                            {
                                RangedAttackingUnit = unitHandler;
                                RangedAttackAsync().Forget();

                                return;
                            }
                        }
                    }
                }

                await UniTask.WaitForSeconds(0.5f);
            }
        }

        public async UniTaskVoid MeleeAttackAsync()
        {
            while (MeleeAttackingUnit != null && IsDead == false)
            {
                IDamageable damageable = MeleeAttackingUnit;
                damageable.TakeDamage(meleeAttackDamage);

                await UniTask.WaitForSeconds(meleeAttackSpeed);
            }

            SeekToMeleeAttackAsync().Forget();
        }

        public async UniTaskVoid RangedAttackAsync()
        {
            while (RangedAttackingUnit != null && IsDead == false)
            {
                IDamageable damageable = RangedAttackingUnit;
                damageable.TakeDamage(rangedAttackRange);

                await UniTask.WaitForSeconds(rangedAttackSpeed);
            }

            SeekToRangeAttackAsync().Forget();
        }

        protected void Update()
        {
            if (Vector3.Distance(_agent.destination, transform.position) < _agent.stoppingDistance && faceOnDestination == true)
            {
                float currentAngle = transform.rotation.eulerAngles.y;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, facingAngle, rotationSpeed * Time.deltaTime);

                this.transform.rotation = Quaternion.AngleAxis(newAngle, Vector3.up);
            }

            IsStopped = Vector3.Distance(_agent.destination, this.transform.position) < _agent.stoppingDistance;
        }

        public void SetTargetDestination(Vector3 newTargetDestination)
        {
            if (_agent == null)
            {
                _agent = this.GetComponent<NavMeshAgent>();
            }

            faceOnDestination = true;

            _agent.destination = newTargetDestination;
        }

        public void SetTargetDestination(Vector3 newTargetDestination, float newFacingAngle)
        {
            if (_agent == null)
            {
                _agent = this.GetComponent<NavMeshAgent>();
            }

            faceOnDestination = true;

            _agent.destination = newTargetDestination;
            facingAngle = newFacingAngle;
        }

        void IDamageable.OnDead()
        {
            if (IsDead == false)
            {
                Controller.RemoveUnit(this);
            }
            IsDead = true;
        }

        void IDamageable.OnDamaged()
        {
            _animation.PlayQueued("OnDamaged");
        }
    }
}