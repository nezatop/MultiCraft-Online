using MultiCraft.Scripts.Engine.Core.Inventories;
using MultiCraft.Scripts.Engine.UI;

namespace MultiCraft.Scripts.Engine.Core.CraftingSystem
{
    public class CraftResultSlot : Slot
    {
        public override void LeftClick()
        {
            if(InventoryWindow.Instance.HasCurrentItem || !InventoryWindow.Instance.CraftController.HasrResultItem)
                return;
            
            InventoryWindow.Instance.SetCurrentItem(Item);
            ResetItem();
            
            InventoryWindow.Instance.CraftController.CraftItem();
        }
    }
}