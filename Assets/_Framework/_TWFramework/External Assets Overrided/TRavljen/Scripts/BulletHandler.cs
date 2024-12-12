using _TW_Framework._Pool_Internal;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TW_Framework
{
    public class BulletHandler : MonoBehaviour
    {
        protected Rigidbody _rigidbody;

        [SerializeField]
        protected float speed = 100f;
        [SerializeField]
        protected float lifeTime = 3f;

        protected void Awake()
        {
            _rigidbody = this.GetComponent<Rigidbody>();
        }

        protected void OnEnable()
        {
            _rigidbody.linearVelocity = this.transform.forward * speed;

            CheckLifetimeAsync().Forget();
        }

        protected async UniTaskVoid CheckLifetimeAsync()
        {
            await UniTask.WaitForSeconds(lifeTime);

            this.gameObject.ReturnPool(PoolerType.MusketBullet);
        }
    }
}