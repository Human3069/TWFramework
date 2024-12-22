using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
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
      
                float distance = (this.transform.position - _current).magnitude;
                Vector3 direction = (this.transform.position - _current).normalized;
            
                RaycastHit[] hits = Physics.RaycastAll(_current, direction, distance);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.TryGetComponent<UnitHandler>(out UnitHandler hitHandler) == true)
                    {
                        if (this._teamType != hitHandler._TeamType &&
                            hitHandler.IsDead == false)
                        {
                            float currentPower = _rigidbody.linearVelocity.magnitude / 10f;
                            currentPower = Mathf.Lerp(0f, currentPower, Random.Range(0.25f, 1f));

                            IDamageable damageable = hitHandler as IDamageable;
                            damageable.TakeDamage(_damage, DieType.Physical, this.transform.forward * currentPower);
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

                            contactCount++;

                            float currentVelocity = _rigidbody.linearVelocity.magnitude;
                            this.transform.forward = GetReflectedAngle(direction, hit.normal);
                            _rigidbody.linearVelocity = this.transform.forward * (currentVelocity / (contactCount + 1));
                        }

                        OnHitAction?.Invoke(this, hit);
                        OnHitAction = null;
                    }
                }

                currentPos = this.transform.position;
                await UniTask.Yield(this.destroyCancellationToken);
            }
        }

        protected Vector3 GetReflectedAngle(Vector3 inDirection, Vector3 inNormal)
        {
            Vector3 reflected = Vector3.Reflect(inDirection, inNormal);
            return reflected;
        }
    }
}