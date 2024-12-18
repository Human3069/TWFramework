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

        void TakeDamage(float damage, DieType dieType, Vector3 thrownPower)
        {
            CurrentHealth -= damage;

            if (CurrentHealth > 0f)
            {
                OnDamaged();
            }
            else
            {
                OnDead(dieType, thrownPower);
            }
        }

        void OnDamaged();

        void OnDead(DieType dieType, Vector3 thrownPower);
    }
}