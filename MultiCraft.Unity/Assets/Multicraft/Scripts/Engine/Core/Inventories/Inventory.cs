using System.Collections.Generic;
using System.Linq;
using MultiCraft.Scripts.Engine.Core.Items;
using MultiCraft.Scripts.Engine.UI;
using MultiCraft.Scripts.Engine.Utils;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Inventories
{
    public class Inventory : MonoBehaviour
    {
        public List<ItemInSlot> Slots;
        public int HotBarSelectedSlot = 0;
        private bool _init = false;

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (UiManager.Instance.inventoryUpdated == true)
            {
                Slots = InventoryWindow.Instance.InventoryController.GetInv();

                UiManager.Instance.inventoryUpdated = false;
                UiManager.Instance.UpdateInventory(Slots);
            }
        }

        private void Initialize()
        {
            Slots = new List<ItemInSlot>();
            for (int i = 0; i < 9 * 4; i++)
            {
                Slots.Add(new ItemInSlot());
                Slots.Add(new ItemInSlot());
            }

            _init = true;

            foreach (var item in ResourceLoader.Instance.getItems.Values.Where(item => item.Type == ItemType.Block))
            {
                AddItem(item, 64);
            }
        }

        public Item UpdateHotBarSelectedSlot(int index)
        {
            HotBarSelectedSlot = index;
            UiManager.Instance.InventoryWindow.InventoryController.UpdateHotBar(HotBarSelectedSlot);
            return Slots[index] == null ? null : Slots[index].Item;
        }

        public bool AddItem(Item item, int amount)
        {
            if (item == null) return true;

            foreach (var slot in Slots)
            {
                if (slot != null)
                    if (slot.CanAdd(item))
                    {
                        int spaceInSlot = slot.Item.MaxStackSize - slot.Amount;
                        int amountToAdd = Mathf.Min(spaceInSlot, amount);

                        slot.Amount += amountToAdd;
                        amount -= amountToAdd;

                        if (amount == 0)
                        {
                            UiManager.Instance.UpdateInventory(Slots);
                            return true;
                        }
                    }
            }

            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i] == null)
                {
                    var addSlot = new ItemInSlot();
                    int spaceInSlot = item.MaxStackSize - addSlot.Amount;
                    int amountToAdd = Mathf.Min(spaceInSlot, amount);
                    addSlot.Item = item;

                    addSlot.Amount += amountToAdd;
                    amount -= amountToAdd;
                    Slots[i] = addSlot;

                    if (amount == 0)
                    {
                        UiManager.Instance.UpdateInventory(Slots);
                        return true;
                    }
                }
            }

            foreach (var slot in Slots)
            {
                if (slot.CanAdd())
                {
                    int spaceInSlot = item.MaxStackSize - slot.Amount;
                    int amountToAdd = Mathf.Min(spaceInSlot, amount);
                    slot.Item = item;

                    slot.Amount += amountToAdd;
                    amount -= amountToAdd;

                    if (amount == 0)
                    {
                        UiManager.Instance.UpdateInventory(Slots);
                        return true;
                    }
                }
            }

            UiManager.Instance.UpdateInventory(Slots);
            return false;
        }

        public int GetSelectedItemId()
        {
            if (_init == false) return 0;
            var item = Slots[HotBarSelectedSlot].Item;
            if (item == null) return 0;
            if (!item.BlockType) return 0;
            return item.BlockType.Id;
        }

        public ItemInSlot GetSelectedItem()
        {
            if (_init == false) return null;
            var item = Slots[HotBarSelectedSlot];
            return item;
        }

        public ItemInSlot RemoveSelectedItem()
        {
            var item = Slots[HotBarSelectedSlot];
            Slots[HotBarSelectedSlot].Amount--;
            if (Slots[HotBarSelectedSlot].Amount == 0)
            {
                Slots[HotBarSelectedSlot].Item = null;
            }

            UiManager.Instance.UpdateInventory(Slots);
            return item;
        }

        public void RemoveDurability()
        {
            if (Slots[HotBarSelectedSlot].Item != null)
            {
                if (Slots[HotBarSelectedSlot].Item.Type == ItemType.Tool)
                {
                    Slots[HotBarSelectedSlot].Durability--;
                    if (Slots[HotBarSelectedSlot].Durability <= 0)
                    {
                        RemoveSelectedItem();
                    }
                }
            }
        }

        public bool AddItem(ItemInSlot item)
        {
            return AddItem(item.Item, item.Amount);
        }

        public void Clear()
        {
            foreach (var slot in Slots)
            {
                slot.Item = null;
                slot.Amount = 0;
                slot.Durability = 0;
            }
            
            UiManager.Instance.UpdateInventory(Slots);
        }
    }
}