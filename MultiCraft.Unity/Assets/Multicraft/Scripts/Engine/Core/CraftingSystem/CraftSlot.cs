using MultiCraft.Scripts.Engine.Core.Inventories;
using MultiCraft.Scripts.Engine.UI;

namespace MultiCraft.Scripts.Engine.Core.CraftingSystem
{
    public class CraftSlot : Slot
    {
        public override void LeftClick()
        {
            base.LeftClick();
            InventoryWindow.Instance.CraftController.CheckCraft();
        }

        public override void RightClick()
        {
            base.RightClick();
            InventoryWindow.Instance.CraftController.CheckCraft();
        }

        public void DecreaseItemAmount(int amount)
        {
            Item.Amount -= amount;

            if (Item.Amount < 1)
            {
                ResetItem();
            }
            else
            {
                RefreshUI();
            }
        }
    }
}