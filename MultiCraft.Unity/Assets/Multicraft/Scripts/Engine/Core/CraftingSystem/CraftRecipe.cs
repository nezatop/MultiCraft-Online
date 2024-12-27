using System.Collections.Generic;
using MultiCraft.Scripts.Engine.Core.Items;
using MultiCraft.Scripts.Engine.Utils;

namespace MultiCraft.Scripts.Engine.Core.CraftingSystem
{
    [System.Serializable]
    public class CraftRecipe
    {
        public List<ItemScriptableObject> Items;
        public int Amount;

        public Item[] GetCraftRecipe()
        {
            var ItemsOreder = new Item[Items.Count];
            for (int orderId = 0, i = 0; i < Items.Count; i++)
            {
                ItemsOreder[orderId++] = ResourceLoader.Instance.GetItem(Items[i]);
            }

            return ItemsOreder;
        }
    }
}