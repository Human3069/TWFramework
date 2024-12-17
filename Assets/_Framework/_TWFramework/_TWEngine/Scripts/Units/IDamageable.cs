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

        void TakeDamage(float damage, DieType dieType)
        {
            CurrentHealth -= damage;

            if (CurrentHealth > 0f)
            {
                OnDamaged();
            }
            else
            {
                OnDead(dieType);
            }
        }

        void OnDamaged();

        void OnDead(DieType dieType);
    }
}