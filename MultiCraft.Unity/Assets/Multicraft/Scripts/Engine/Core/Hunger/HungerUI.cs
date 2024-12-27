using UnityEngine;

namespace Multicraft.Scripts.Engine.Core.Hunger
{
    public class HungerUI: MonoBehaviour
    {
       public Transform emptyHungerBar; // Контейнер для "пустых" элементов здоровья
        
        private int maxHunger;
        private HungerSystem hungerSystem;

        public void InitializeHealth(HungerSystem hunger)
        {
            hungerSystem = hunger;
            if (hungerSystem == null)
            {
                Debug.LogError("Health component not found on the object.");
                return;
            }

            maxHunger = (int)hungerSystem.maxHunger;
            hungerSystem.onHungerChanged += UpdateHugnerBar;

            UpdateHugnerBar((int)hungerSystem.maxHunger);
        }

        public void UpdateHugnerBar(int currentHealth)
        {
            if (emptyHungerBar.childCount != maxHunger)
            {
                Debug.LogError("Child count of emptyHealthBar does not match maxHealth.");
                return;
            }

            for (int i = 0; i < maxHunger; i++)
            {
                emptyHungerBar.GetChild(i).gameObject.SetActive(i >= currentHealth);
            }
        }

        void OnDisable()
        {
            if (hungerSystem != null)
            {
                hungerSystem.onHungerChanged -= UpdateHugnerBar;
            }
        }

    }
}