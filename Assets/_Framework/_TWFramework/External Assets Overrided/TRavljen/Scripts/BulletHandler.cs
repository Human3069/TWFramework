using _TW_Framework._Pool_Internal;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
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

            this.gameObject.ReturnPool(PoolerType.MusketBullet);
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

                            this.gameObject.ReturnPool(PoolerType.MusketBullet);
                        }
                    }
                    else
                    {
                        this.gameObject.ReturnPool(PoolerType.MusketBullet);
                    }
                }

                currentPos = this.transform.position;
                await UniTask.Yield(this.destroyCancellationToken);
            }
        }
    }
}