using System;
using Multicraft.Scripts.Engine.Core.Hunger;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.HealthSystem
{
    public class Health : MonoBehaviour
    {
        public float health = 10;

        public float maxHealth = 10;

        public event Action OnDeath;
        public event Action<int> OnDamage;

        private void Awake()
        {
            gameObject.GetComponent<HungerSystem>().onHungerZero += TakeDamage;
        }

        private void OnDisable()
        {
            gameObject.GetComponent<HungerSystem>().onHungerZero -= TakeDamage;
        }

        public void TakeDamage(int damage)
        {
            health = Mathf.Clamp(health - damage, 1, maxHealth);
            if (health <= 0) OnDeath?.Invoke();
            OnDamage?.Invoke((int)health);
        }

        public void TakeDamage()
        {
            var damage = 0.001f * Time.deltaTime;
            health = Mathf.Clamp((health - damage), 1f, maxHealth);
            if (health <= 0) OnDeath?.Invoke();
            OnDamage?.Invoke((int)health);
        }
    }
}