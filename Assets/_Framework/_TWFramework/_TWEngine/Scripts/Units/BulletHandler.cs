using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace _TW_Framework
{
    public enum DieType
    {
        Animated,
        Physical
    }

    public class BulletHandler : MonoBehaviour
    {
        protected Rigidbody _rigidbody;
   
        [Header("=== BulletHandler ===")]
        [SerializeField]
        protected PoolerType poolerType;
        [SerializeField]
        protected float speed = 100f;
        [SerializeField]
        protected float lifeTime = 3f;

        protected Vector3 currentPos = Vector3.zero;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected TeamType _teamType;
        [ReadOnly]
        [SerializeField]
        protected float _damage = 0f;

        public Action<BulletHandler, RaycastHit> OnHitAction;

        protected virtual void Awake()
        {
            _rigidbody = this.GetComponent<Rigidbody>();
        }

        public virtual void Initialize(TeamType type, float damage)
        {
            _teamType = type;
            _damage = damage;

            currentPos = Vector3.zero;
        }

        protected virtual void OnEnable()
        {
            _rigidbody.linearVelocity = this.transform.forward * speed;

            CheckLifetimeAsync().Forget();
            CheckRaycastAsync().Forget();
        }

        protected virtual async UniTaskVoid CheckLifetimeAsync()
        {
            await UniTask.WaitForSeconds(lifeTime);

            if (this.gameObject.activeSelf == true)
            {
                this.gameObject.ReturnPool(poolerType);
            }
        }

        protected virtual async UniTaskVoid CheckRaycastAsync()
        {
            while (this.enabled == true)
            {
                Vector3 _current = currentPos == Vector3.zero ? this.transform.position : currentPos;
                float distance = (_current - this.transform.position).magnitude;

                if (Physics.Raycast(_current, this.transform.forward, out RaycastHit hit, distance) == true)
                {
                    if (hit.collider.TryGetComponent<UnitHandler>(out UnitHandler hitHandler) == true)
                    {
                        if (this._teamType != hitHandler._TeamType && hitHandler.IsDead == false)
                        {
                            IDamageable damageable = hitHandler as IDamageable;
                            damageable.TakeDamage(_damage, DieType.Animated, Vector3.zero);

                            if (this.gameObject.activeSelf == true)
                            {
                                this.gameObject.ReturnPool(poolerType);
                            }
                        }
                    }
                    else
                    {
                        if (hit.collider.gameObject.isStatic == true)
                        {
                            PoolerType hitPoolerType;
                            if (poolerType == PoolerType.MusketBullet)
                            {
                                hitPoolerType = PoolerType.Musket_HitDirt;
                            }
                            else if (poolerType == PoolerType.CannonBall)
                            {
                                hitPoolerType = PoolerType.Cannon_HitDirt;
                            }
                            else
                            {
                                throw new System.NotImplementedException();
                            }

                            hitPoolerType.EnablePool(OnBeforeEnablePool);
                            void OnBeforeEnablePool(GameObject obj)
                            {
                                obj.transform.position = hit.point;
                                obj.transform.rotation = Quaternion.LookRotation(hit.normal);

                                PostOnPoolEnabled().Forget();
                                async UniTaskVoid PostOnPoolEnabled()
                                {
                                    await UniTask.WaitForSeconds(10, cancellationToken: obj.GetCancellationTokenOnDestroy());
                                    obj.ReturnPool(hitPoolerType);
                                }
                            }

                            if (this.gameObject.activeSelf == true)
                            {
                                this.gameObject.ReturnPool(poolerType);
                            }
                        }
                    }

                    OnHitAction?.Invoke(this, hit);
                    OnHitAction = null;
                }

                currentPos = this.transform.position;
                await UniTask.Yield(this.destroyCancellationToken);
            }
        }
    }
}