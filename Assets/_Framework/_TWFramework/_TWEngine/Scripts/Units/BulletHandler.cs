using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TW_Framework
{
    public class BulletHandler : MonoBehaviour
    {
        protected Rigidbody _rigidbody;
        protected TeamType _teamType;

        [SerializeField]
        protected float speed = 100f;
        [SerializeField]
        protected float lifeTime = 3f;

        protected Vector3 currentPos = Vector3.zero;
        protected float _damage = 0f;

        protected void Awake()
        {
            _rigidbody = this.GetComponent<Rigidbody>();
        }

        public void Initialize(TeamType type, float damage)
        {
            _teamType = type;
            _damage = damage;

            currentPos = Vector3.zero;
        }

        protected void OnEnable()
        {
            _rigidbody.linearVelocity = this.transform.forward * speed;

            CheckLifetimeAsync().Forget();
            CheckRaycastAsync().Forget();
        }

        protected async UniTaskVoid CheckLifetimeAsync()
        {
            await UniTask.WaitForSeconds(lifeTime);

            if (this.gameObject.activeSelf == true)
            {
                this.gameObject.ReturnPool(PoolerType.MusketBullet);
            }
        }

        protected async UniTaskVoid CheckRaycastAsync()
        {
            while (this.enabled == true)
            {
                Vector3 _current = currentPos == Vector3.zero ? this.transform.position : currentPos;
                float distance = (_current - this.transform.position).magnitude;

                if (Physics.Raycast(_current, this.transform.forward, out RaycastHit hit, distance) == true)
                {
                    if (hit.collider.TryGetComponent<UnitHandler>(out UnitHandler hitHandler) == true)
                    {
                        if (this._teamType != hitHandler._TeamType)
                        {
                            IDamageable damageable = hitHandler as IDamageable;
                            damageable.TakeDamage(_damage);

                            if (this.gameObject.activeSelf == true)
                            {
                                this.gameObject.ReturnPool(PoolerType.MusketBullet);
                            }
                        }
                    }
                    else
                    {
                        if (hit.collider.gameObject.isStatic == true)
                        {
                            PoolerType.Hit_Dirt.EnablePool(OnBeforeEnablePool);
                            void OnBeforeEnablePool(GameObject obj)
                            {
                                obj.transform.position = hit.point;
                                obj.transform.rotation = Quaternion.LookRotation(hit.normal);

                                PostOnPoolEnabled().Forget();
                                async UniTaskVoid PostOnPoolEnabled()
                                {
                                    await UniTask.WaitForSeconds(10, cancellationToken : obj.GetCancellationTokenOnDestroy());
                                    obj.ReturnPool(PoolerType.Hit_Dirt);
                                }
                            }
                        }

                        if (this.gameObject.activeSelf == true)
                        {
                            this.gameObject.ReturnPool(PoolerType.MusketBullet);
                        }
                    }
                }

                currentPos = this.transform.position;
                await UniTask.Yield(this.destroyCancellationToken);
            }
        }
    }
}