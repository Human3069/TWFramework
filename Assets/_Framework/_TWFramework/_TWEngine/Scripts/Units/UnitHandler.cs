using Cysharp.Threading.Tasks;
using EPOOutline;
using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace _TW_Framework
{
    public enum UnitCategory
    {
        Infantry,
        Cavalry,
    }

    [RequireComponent(typeof(Animation))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Collider))]
    public class UnitHandler : MonoBehaviour, IDamageable
    {
        protected Animation _animation;
        protected NavMeshAgent _agent;
        protected AudioSource _audioSource;
        protected Outlinable _outlinable;
        protected Collider[] colliders;

        [HideInInspector]
        public BaseFormationController Controller;
        [HideInInspector]
        public TeamType _TeamType;

        [SerializeField]
        protected UnitCategory unitCategory;

        [Header("=== IDamageable ===")]
        [ReadOnly]
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
        [SerializeField]
        protected AudioClip[] fireClips;

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
                        // _UnitState = UnitState.Idle;
                    }
                    else
                    {
                        // _UnitState = UnitState.Moving;
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
        [ReadOnly]
        [SerializeField]
        protected MeleeAttack meleeAttack;
        [ReadOnly]
        [SerializeField]
        protected RangedAttack rangedAttack;

        public Vector3 MiddlePos
        {
            get
            {
                return this.transform.position + Vector3.up;
            }
        }

        public Vector3 Velocity
        {
            get
            {
                return _agent.velocity;
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
                           this.isInitialized == true &&
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

        protected bool isInitialized = false;

        protected void Awake()
        {
            _animation = this.GetComponent<Animation>();
            _agent = this.GetComponent<NavMeshAgent>();
            _audioSource = this.GetComponent<AudioSource>();
            _outlinable = this.GetComponent<Outlinable>();
            colliders = this.GetComponents<Collider>();
        }

        protected void OnDestroy()
        {
            meleeAttack = null;
            rangedAttack = null;
        }

        public void Initialize(BaseFormationController controller, TeamType teamType, UnitInfo unitInfo)
        {
            IsDead = false;

            this.Controller = controller;
            this._TeamType = teamType;

            maxHealth = unitInfo.MaxHealth;
            CurrentHealth = maxHealth;

            isInitialized = true;

            meleeAttack.Initialize(this, unitInfo.MeleeDamage, unitInfo.MeleeSpeed, unitInfo.MeleeRange);
            rangedAttack.Initialize(this, unitInfo._WeaponInfo);
        }

        public void OnSelected()
        {
            _outlinable.enabled = true;
        }

        public void OnDeselected()
        {
            _outlinable.enabled = false;
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

        public void SetTargetDestinationImmediately(Vector3 position, float newFacingAngle)
        {
            if (_agent == null)
            {
                _agent = this.GetComponent<NavMeshAgent>();
            }

            faceOnDestination = true;

            _agent.Warp(position);
            this.transform.eulerAngles = new Vector3(0f, newFacingAngle, 0f);

            facingAngle = newFacingAngle;
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

        void IDamageable.OnDead(DieType dieType, Vector3 thrownPower)
        {
            Controller.RemoveUnit(this);
            IsDead = true;

            _agent.enabled = false;
            _audioSource.enabled = false;
            Destroy(_outlinable);
            this.enabled = false;

            if (dieType == DieType.Animated)
            {
                _animation.PlayQueued(unitCategory + "_DefaultDead");
                foreach (Collider collider in colliders)
                {
                    collider.enabled = false;
                }
            }
            else if (dieType == DieType.Physical)
            {
                foreach (Collider collider in colliders)
                {
                    collider.isTrigger = false;
                }

                Rigidbody _rigidbody = this.AddComponent<Rigidbody>();
                _rigidbody.angularDamping = 3f;
                _rigidbody.AddForce(thrownPower, ForceMode.Impulse);
                PostDeadPhysicallyAsync(_rigidbody).Forget();
            }
        }

        protected async UniTaskVoid PostDeadPhysicallyAsync(Rigidbody _rigidbody)
        {
            CancellationToken token = _rigidbody.GetCancellationTokenOnDestroy();

            await UniTask.WaitForSeconds(0.1f, cancellationToken : token);
            await UniTask.WaitWhile(() => _rigidbody.linearVelocity.sqrMagnitude > 0.01f, cancellationToken : token);

            Destroy(_rigidbody);
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
        }

        void IDamageable.OnDamaged()
        {
            // _animation.PlayQueued("OnDamaged");
        }

        public void PlayAnimation(string clipName)
        {
            _animation.PlayQueued(clipName);
        }

        [Serializable]
        public class MeleeAttack
        {
            protected UnitHandler _unitHandler;

            [Space(10)]
            [ReadOnly]
            [SerializeField]
            protected float meleeAttackDamage = 10f;
            [ReadOnly]
            [SerializeField]
            protected float meleeAttackSpeed = 1f;
            [ReadOnly]
            [SerializeField]
            protected float meleeAttackRange = 2f;

            public virtual void Initialize(UnitHandler unitHandler, float attackDamage, float attackSpeed, float attackRange)
            {
                _unitHandler = unitHandler;

                meleeAttackDamage = attackDamage;
                meleeAttackSpeed = attackSpeed;
                meleeAttackRange = attackRange;

                SeekAttackableUnitAsync().Forget();
            }

            protected async UniTask SeekAttackableUnitAsync()
            {
                await UniTask.WaitForSeconds(UnityEngine.Random.Range(1f, 2f));

                while (_unitHandler.IsValid == true)
                {
                    Collider[] overlappedColliders = Physics.OverlapSphere(_unitHandler.transform.position, meleeAttackRange);
                    foreach (Collider overlappedCollider in overlappedColliders)
                    {
                        if (overlappedCollider.TryGetComponent<UnitHandler>(out UnitHandler overlappedUnit) == true &&
                            overlappedUnit.IsValid == true &&
                            overlappedUnit._TeamType != _unitHandler._TeamType)
                        {
                            await AttackAsync(overlappedUnit);
                        }
                    }

                    await UniTask.WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
                }
            }

            protected async UniTask AttackAsync(UnitHandler targetUnit)
            {
                while (targetUnit.IsValid == true &&
                      (targetUnit.transform.position - _unitHandler.transform.position).magnitude < meleeAttackRange &&
                      _unitHandler.IsValid == true)
                {
                    _unitHandler.PlayAnimation("MeleeAttack");
                    IDamageable damageable = targetUnit;
                    damageable.TakeDamage(meleeAttackDamage, DieType.Animated, Vector3.zero);

                    await UniTask.WaitForSeconds(UnityEngine.Random.Range(meleeAttackSpeed - 0.5f, meleeAttackSpeed + 0.5f));
                }
            }
        }

        [Serializable]
        public class RangedAttack
        {
            protected UnitHandler _unitHandler;

            [ReadOnly]
            public PoolerType _PoolerType;

            [Space(10)]
            [ReadOnly]
            [SerializeField]
            protected float rangedAttackDamage = 10f;
            [ReadOnly]
            [SerializeField]
            protected float rangedAttackSpeed = 1f;

            [Space(10)]
            [ReadOnly]
            [SerializeField]
            protected float reloadingRemained;

            [Space(10)]
            [ReadOnly]
            [SerializeField]
            protected float rangedAttackRange = 10f;
            [ReadOnly]
            [SerializeField]
            protected float rangedAttackAngle = 45f;

            [Space(10)]
            [ReadOnly]
            [SerializeField]
            protected float accuracy = 100f; // ranged 0 ~ 100

            [Space(10)]
            [ReadOnly]
            [SerializeField]
            protected int maxAmmoCount = 5;
            [ReadOnly]
            [SerializeField]
            protected int currentAmmoCount;

            public virtual void Initialize(UnitHandler unitHandler, WeaponInfo weaponInfo)
            {
                _unitHandler = unitHandler;

                maxAmmoCount = weaponInfo.MaxAmmo;
                currentAmmoCount = maxAmmoCount;

                rangedAttackDamage = weaponInfo.Damage;
                rangedAttackSpeed = weaponInfo.ReloadTime;
                rangedAttackRange = weaponInfo.Range;
                accuracy = weaponInfo.Accuracy;

                _PoolerType = weaponInfo._PoolerType;

                SeekAttackableUnitAsync().Forget();
            }

            protected async UniTask SeekAttackableUnitAsync()
            {
                await UniTask.WaitForSeconds(UnityEngine.Random.Range(1f, 2f));

                while (_unitHandler.IsValid == true)
                {
                    Collider[] overlappedColliders = Physics.OverlapSphere(_unitHandler.transform.position, rangedAttackRange);
                    foreach (Collider overlappedCollider in overlappedColliders)
                    {
                        Vector3 directionToTarget = (overlappedCollider.transform.position - _unitHandler.transform.position).normalized;
                        float angleToTarget = Vector3.Angle(_unitHandler.transform.forward, directionToTarget);

                        if (angleToTarget < rangedAttackAngle * 0.5f)
                        {
                            if (overlappedCollider.TryGetComponent<UnitHandler>(out UnitHandler overlappedUnit) == true &&
                                overlappedUnit.IsValid == true &&
                                overlappedUnit._TeamType != _unitHandler._TeamType)
                            {
                                if (_unitHandler.Controller.TargetController == null)
                                {
                                    await AttackAsync(overlappedUnit);
                                }
                                else
                                {
                                    if (_unitHandler.Controller.TargetController == overlappedUnit.Controller)
                                    {
                                        await AttackAsync(overlappedUnit);
                                    }
                                }
                            }
                        }
                    }

                    await UniTask.WaitForSeconds(1f);
                }
            }

            protected async UniTask AttackAsync(UnitHandler targetUnit)
            {
                while (targetUnit.IsValid == true &&
                      (targetUnit.transform.position - _unitHandler.transform.position).magnitude < rangedAttackRange &&
                       _unitHandler.IsValid == true &&
                       currentAmmoCount > 0)
                {
                    while (reloadingRemained > 0f)
                    {
                        reloadingRemained -= Time.deltaTime;
                        await UniTask.Yield();
                    }

                    await UniTask.WaitUntil(() => _unitHandler.IsStopped == true);

                    if (targetUnit.IsValid == false ||
                       (targetUnit.transform.position - _unitHandler.transform.position).magnitude > rangedAttackRange ||
                       _unitHandler.IsValid == false)
                    {
                        break;
                    }

                    _unitHandler.PlayAnimation("RangedAttack");

                    int randomIndex = UnityEngine.Random.Range(0, _unitHandler.fireClips.Length);
                    _unitHandler._audioSource.PlayOneShot(_unitHandler.fireClips[randomIndex]);

                    _unitHandler.muzzleFlash.Play();
                    _PoolerType.EnablePool<BulletHandler>(BeforePool);
                    void BeforePool(BulletHandler bullet)
                    {
                        bullet.Initialize(_unitHandler._TeamType, rangedAttackDamage);

                        Vector3 maxRandomed = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f), 0f);
                        Vector3 accuracyApplied = Vector3.Lerp(maxRandomed, Vector3.zero, accuracy / 100f);

                        bullet.transform.position = _unitHandler.muzzleFlash.transform.position;
                        float distance = (targetUnit.MiddlePos - _unitHandler.MiddlePos).magnitude;

                        PredictDataBundle predictBundle = PredictDataBundle.GetPredictData(_PoolerType);
                        Vector3 predictPos = predictBundle.GetPredictedPosition(_unitHandler.muzzleFlash.transform.position, targetUnit);
                        Debug.DrawLine(_unitHandler.MiddlePos, predictPos, Color.blue, 0.1f);
                        Debug.DrawLine(predictPos, targetUnit.MiddlePos, Color.blue, 0.5f);

                        Vector3 direction = (predictPos - _unitHandler.muzzleFlash.transform.position).normalized;
                        bullet.transform.eulerAngles = Quaternion.LookRotation(direction).eulerAngles + accuracyApplied;
                    }

                    currentAmmoCount--;
                    reloadingRemained = UnityEngine.Random.Range(rangedAttackSpeed - 0.5f, rangedAttackSpeed + 0.5f);
                }
            }
        }
    }
}