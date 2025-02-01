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

        void TakeDamage(BaseFormationController attackerController, float damage, DieType dieType, Vector3 thrownPower)
        {
            CurrentHealth -= damage;

            if (CurrentHealth > 0f)
            {
                OnDamaged(attackerController);
            }
            else
            {
                OnDead(dieType, thrownPower);
            }
        }

        void OnDamaged(BaseFormationController attackerController);

        void OnDead(DieType dieType, Vector3 thrownPower);
    }
}