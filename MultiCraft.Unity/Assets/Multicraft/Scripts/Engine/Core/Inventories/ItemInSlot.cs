using MultiCraft.Scripts.Engine.Core.Items;

namespace MultiCraft.Scripts.Engine.Core.Inventories
{
    public class ItemInSlot
    {
        public Item Item;
        public int Amount;
        public int Durability;

        public ItemInSlot(Item item, int amount)
        {
            Item = item;
            Amount = amount;
            Durability = item.MaxDurability;
        }

        public ItemInSlot()
        {
            Item = null;
            Amount = 0;
            Durability = 0;
        }

        public void ReduceDurability()
        {
            Durability--;
        }
        
        public bool CanAdd(Item item)
        {
            if (Item == null) return false;
            if (Amount < Item.MaxStackSize && Item.Name == item.Name) return true;
            return false;
        }

        public bool CanAdd()
        {
            if (Amount == 0) return true;
            return false;
        }
    }
}