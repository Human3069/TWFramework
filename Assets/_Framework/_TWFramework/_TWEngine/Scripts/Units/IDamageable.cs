using UnityEngine;

namespace _TW_Framework
{
    public interface IDamageable 
    {
        float CurrentHealth
        {
            get;
            set;
        }

        void TakeDamage(float damage)
        {
            CurrentHealth -= damage;

            if (CurrentHealth > 0f)
            {
                OnDamaged();
            }
            else
            {
                OnDead();
            }
        }

        void OnDamaged();

        void OnDead();
    }
}