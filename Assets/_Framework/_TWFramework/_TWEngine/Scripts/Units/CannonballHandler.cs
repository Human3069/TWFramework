using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TW_Framework
{
    public class CannonballHandler : BulletHandler
    {
        // [Header("=== CannonballHandler ===")]

        protected int contactCount = 0;

        public override void Initialize(TeamType type, float damage)
        {
            base.Initialize(type, damage);

            contactCount = 0;
        }

        protected override async UniTaskVoid CheckRaycastAsync()
        {
            while (this.enabled == true)
            {
                Vector3 _current = currentPos == Vector3.zero ? this.transform.position : currentPos;
                float distance = (_current - this.transform.position).magnitude;

                RaycastHit[] hits = Physics.RaycastAll(_current, this.transform.forward, distance);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.TryGetComponent<UnitHandler>(out UnitHandler hitHandler) == true)
                    {
                        if (this._teamType != hitHandler._TeamType)
                        {
                            IDamageable damageable = hitHandler as IDamageable;
                            damageable.TakeDamage(_damage);
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
                        }

                        contactCount++;

                        float currentVelocity = _rigidbody.linearVelocity.magnitude;
                        this.transform.forward = GetReflectedAngle(this.transform.forward, hit.normal, Random.Range(5f, 20f));
                        _rigidbody.linearVelocity = this.transform.forward * (currentVelocity / (contactCount + 1));
                    }
                }

                currentPos = this.transform.position;
                await UniTask.Yield(this.destroyCancellationToken);
            }
        }

        protected Vector3 GetReflectedAngle(Vector3 inDirection, Vector3 inNormal, float additionalAngle)
        {
            Vector3 reflected = Vector3.Reflect(inDirection, inNormal);
            Quaternion asAngleAxis = Quaternion.AngleAxis(additionalAngle, -Vector3.forward);

            return asAngleAxis * reflected;
        }
    }
}