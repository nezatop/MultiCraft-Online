using MultiCraft.Scripts.Engine.Core.HealthSystem;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.UI
{
    public class HealthView : MonoBehaviour
    {
        public Transform fullHealthBar; // Полоса здоровья (не используется для визуального изменения)
        public Transform emptyHealthBar; // Контейнер для "пустых" элементов здоровья
        
        private int maxHealth;
        private Health healthComponent;

        public void InitializeHealth(Health health)
        {
            healthComponent = health;
            if (healthComponent == null)
            {
                Debug.LogError("Health component not found on the object.");
                return;
            }

            maxHealth = (int)healthComponent.maxHealth;
            healthComponent.OnDamage += UpdateHealthBar;

            UpdateHealthBar((int)healthComponent.maxHealth);
        }

        public void UpdateHealthBar(int currentHealth)
        {
            if (emptyHealthBar.childCount != maxHealth)
            {
                Debug.LogError("Child count of emptyHealthBar does not match maxHealth.");
                return;
            }

            for (int i = 0; i < maxHealth; i++)
            {
                emptyHealthBar.GetChild(i).gameObject.SetActive(i >= currentHealth);
            }
        }

        void OnDestroy()
        {
            if (healthComponent != null)
            {
                healthComponent.OnDamage -= UpdateHealthBar;
            }
        }

    }
}