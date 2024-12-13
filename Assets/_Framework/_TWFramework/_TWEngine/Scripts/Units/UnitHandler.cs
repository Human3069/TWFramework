using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace _TW_Framework
{
    public enum UnitState
    {
        Idle,
        Moving,
        MeleeAttacking,
        RangedAttacking,
        Dead
    }

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

        [ReadOnly]
        [SerializeField]
        protected UnitState _unitState = UnitState.Idle;
        public UnitState _UnitState
        {
            get
            {
                return _unitState;
            }
            set
            {
                if (_unitState != value)
                {
                    _unitState = value;

                    if (OnStateChanged != null)
                    {
                        OnStateChanged.Invoke(value);
                    }
                }
            }
        }

        public Action<UnitState> OnStateChanged;

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
        protected ParticleSystem muzzleFlash;

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
                        _UnitState = UnitState.Idle;
                    }
                    else
                    {
                        _UnitState = UnitState.Moving;
                        UniTaskEx.Cancel(this, 0);
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

        [Space(10)]
        [SerializeField]
        protected MeleeAttack meleeAttack;
        [SerializeField]
        protected RangedAttack rangedAttack;

        public Vector3 MiddlePos
        {
            get
            {
                return this.transform.position + Vector3.up;
            }
        }

        public bool IsValid
        {
            get
            {
                try
                {
                    return this.transform != null &&
                           this.gameObject != null &&
                            _animation != null &&
                           _agent != null &&
                           IsDead == false &&
                           CurrentHealth != 0f;
                }
                catch
                {
                    return false;
                }
            }
        }

        protected void Awake()
        {
            _animation = this.GetComponent<Animation>();
            _agent = this.GetComponent<NavMeshAgent>();

            meleeAttack.Initialize(this);
            rangedAttack.Initialize(this);
        }

        protected void OnDestroy()
        {
            UniTaskEx.Cancel(this, 0);

            meleeAttack = null;
            rangedAttack = null;
        }

        public void Initialize(FormationController controller, TeamType teamType)
        {
            IsDead = false;

            this.Controller = controller;
            this._TeamType = teamType;

            CurrentHealth = maxHealth;
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

        public void PlayAnimation(string clipName)
        {
            _animation.PlayQueued(clipName);
        }

        [System.Serializable]
        public class MeleeAttack
        {
            protected UnitHandler _unitHandler;

            [ReadOnly]
            public UnitHandler MeleeAttackingUnit;

            [Space(10)]
            [SerializeField]
            protected float meleeAttackDamage = 10f;
            [SerializeField]
            protected float meleeAttackSpeed = 1f;

            public virtual void Initialize(UnitHandler unitHandler)
            {
                _unitHandler = unitHandler;
                _unitHandler.OnStateChanged += OnStateChanged;
            }

            protected void OnStateChanged(UnitState unitState)
            {
                if (unitState == UnitState.Idle)
                {
                    SeekToMeleeAttackAsync().Forget();
                }
                else if (unitState == UnitState.Moving)
                {
                    MeleeAttackingUnit = null;
                }
            }

            public async UniTask SeekToMeleeAttackAsync()
            {
                while (MeleeAttackingUnit == null && _unitHandler.IsDead == false && _unitHandler._UnitState == UnitState.Idle)
                {
                    Collider[] overlappedColliders = Physics.OverlapSphere(_unitHandler.transform.position, 2f);
                    foreach (Collider overlappedCollider in overlappedColliders)
                    {
                        if (overlappedCollider.TryGetComponent<UnitHandler>(out UnitHandler unitHandler) == true)
                        {
                            if (unitHandler._TeamType != _unitHandler._TeamType)
                            {
                                MeleeAttackingUnit = unitHandler;
                                MeleeAttackAsync().Forget();

                                return;
                            }
                        }
                    }

                    await UniTaskEx.WaitForSeconds(_unitHandler, 0, UnityEngine.Random.Range(0.4f, 0.6f));
                }
            }

            protected async UniTask MeleeAttackAsync()
            {
                while (MeleeAttackingUnit != null && (_unitHandler._UnitState == UnitState.Idle || _unitHandler._UnitState == UnitState.MeleeAttacking) && _unitHandler.IsDead == false)
                {
                    _unitHandler._UnitState = UnitState.MeleeAttacking;
                    _unitHandler.PlayAnimation("MeleeAttack");

                    IDamageable damageable = MeleeAttackingUnit;
                    damageable.TakeDamage(meleeAttackDamage);

                    await UniTaskEx.WaitForSeconds(_unitHandler, 0, UnityEngine.Random.Range(meleeAttackSpeed - 0.5f, meleeAttackSpeed + 0.5f));
                }

                if (_unitHandler.IsStopped == true)
                {
                    _unitHandler._UnitState = UnitState.Idle;
                }
                else
                {
                    _unitHandler._UnitState = UnitState.Moving;
                }
            }
        }

        [System.Serializable]
        public class RangedAttack
        {
            protected UnitHandler _unitHandler;

            [ReadOnly]
            public UnitHandler RangedAttackingUnit;

            [Space(10)]
            [SerializeField]
            protected float rangedAttackDamage = 10f;
            [SerializeField]
            protected float rangedAttackSpeed = 1f;

            [Space(10)]
            [ReadOnly]
            [SerializeField]
            protected float reloadingRemained;

            [Space(10)]
            [SerializeField]
            protected float rangedAttackRange = 10f;
            [SerializeField]
            protected float rangedAttackAngle = 45f;

            [Space(10)]
            [SerializeField]
            protected float accuracy = 100f; // ranged 0 ~ 100

            [Space(10)]
            [SerializeField]
            protected int maxAmmoCount = 5;
            [ReadOnly]
            [SerializeField]
            protected int currentAmmoCount;

            public virtual void Initialize(UnitHandler unitHandler)
            {
                _unitHandler = unitHandler;
                _unitHandler.OnStateChanged += OnStateChanged;

                currentAmmoCount = maxAmmoCount;
            }

            protected void OnStateChanged(UnitState unitState)
            {
                if (unitState == UnitState.Idle)
                {
                    SeekToRangedAttackAsync().Forget();
                }
                else if (unitState == UnitState.Moving)
                {
                    RangedAttackingUnit = null;
                }
            }

            public async UniTask SeekToRangedAttackAsync()
            {
                while (RangedAttackingUnit == null && _unitHandler.IsDead == false && _unitHandler._UnitState == UnitState.Idle && currentAmmoCount > 0)
                {
                    Collider[] overlappedColliders = Physics.OverlapSphere(_unitHandler.transform.position, rangedAttackRange);
                    foreach (Collider overlappedCollider in overlappedColliders)
                    {
                        Vector3 directionToTarget = (overlappedCollider.transform.position - _unitHandler.transform.position).normalized;
                        float angleToTarget = Vector3.Angle(_unitHandler.transform.forward, directionToTarget);

                        if (angleToTarget < rangedAttackAngle * 0.5f)
                        {
                            if (overlappedCollider.TryGetComponent<UnitHandler>(out UnitHandler unitHandler) == true)
                            {
                                if (unitHandler._TeamType != _unitHandler._TeamType)
                                {
                                    RangedAttackingUnit = unitHandler;
                                    RangedAttackAsync().Forget();

                                    return;
                                }
                            }
                        }
                    }

                    await UniTaskEx.WaitForSeconds(_unitHandler, 0, UnityEngine.Random.Range(0.4f, 0.6f));
                }
            }

            protected async UniTask RangedAttackAsync()
            {
                while (RangedAttackingUnit != null && (_unitHandler._UnitState == UnitState.Idle || _unitHandler._UnitState == UnitState.RangedAttacking) && _unitHandler.IsDead == false && currentAmmoCount > 0)
                {
                    while (reloadingRemained > 0f)
                    {
                        reloadingRemained -= Time.deltaTime;
                        await UniTaskEx.Yield(_unitHandler, 0);
                    }

                    if (_unitHandler.IsValid == false || RangedAttackingUnit.IsValid == false)
                    {
                        break;
                    }

                    _unitHandler._UnitState = UnitState.RangedAttacking;
                    _unitHandler.PlayAnimation("RangedAttack");
                    if (_unitHandler.muzzleFlash != null)
                    {
                        _unitHandler.muzzleFlash.Play();

                        PoolerType.MusketBullet.EnablePool<BulletHandler>(BeforePool);
                        void BeforePool(BulletHandler bullet)
                        {
                            bullet.Initialize(_unitHandler._TeamType, rangedAttackDamage);

                            Vector3 maxRandomed = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f), 0f);
                            Vector3 accuracyApplied = Vector3.Lerp(maxRandomed, Vector3.zero, accuracy / 100f);

                            bullet.transform.position = _unitHandler.muzzleFlash.transform.position;
                            Vector3 direction = (RangedAttackingUnit.MiddlePos - _unitHandler.MiddlePos).normalized;
                            bullet.transform.eulerAngles = Quaternion.LookRotation(direction).eulerAngles + accuracyApplied;
                        }
                    }

                    currentAmmoCount--;

                    reloadingRemained = UnityEngine.Random.Range(rangedAttackSpeed - 0.5f, rangedAttackSpeed + 0.5f);
                    while (reloadingRemained > 0f)
                    {
                        reloadingRemained -= Time.deltaTime;
                        await UniTaskEx.Yield(_unitHandler, 0);
                    }
                }

                if (_unitHandler.IsStopped == true)
                {
                    _unitHandler._UnitState = UnitState.Idle;
                }
                else
                {
                    _unitHandler._UnitState = UnitState.Moving;
                }
            }
        }
    }
}