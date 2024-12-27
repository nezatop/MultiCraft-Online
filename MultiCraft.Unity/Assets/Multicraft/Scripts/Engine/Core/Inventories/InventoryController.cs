using System.Collections.Generic;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Inventories
{
    public class InventoryController : MonoBehaviour
    {
        public Slot[,] MainSlots;
        public Slot[,] HotBarSlots;
        public Slot[,] AdditionalSlots;

        [SerializeField] private GameObject slotPref;
        [SerializeField] private Transform mainSlotsGrid;
        [SerializeField] private Transform hotBarSlotsGrid;
        [SerializeField] private Transform additionalSlotsGrid;

        public void Init()
        {
            MainSlots = new Slot[1, 9];
            HotBarSlots = new Slot[1, 9];
            AdditionalSlots = new Slot[3, 9];

            CreateSlotsPrefabs();
        }

        public List<ItemInSlot> GetInv()
        {
            var slots = new List<ItemInSlot>();

            for (int i = 0; i < 9; i++)
            {
                slots.Add(MainSlots[0, i].Item);
            }

            for (int i = 0; i < AdditionalSlots.GetLength(0); i++)
            {
                for (int j = 0; j < AdditionalSlots.GetLength(1); j++)
                {
                    slots.Add(AdditionalSlots[i, j].Item);
                }
            }

            return slots;
        }

        public void UpdateUI(List<ItemInSlot> slots)
        {
            for (int i = 0; i < 9; i++)
            {
                if (slots[i] != null)
                    if (slots[i].Item != null)
                        MainSlots[0, i].SetItem(slots[i]);
                    else
                        MainSlots[0, i].ResetItem();
                else
                    MainSlots[0, i].ResetItem();
            }

            for (int i = 0; i < 9; i++)
            {
                if (slots[i] != null)
                    if (slots[i].Item != null)
                        HotBarSlots[0, i].SetItem(slots[i]);
                    else
                        HotBarSlots[0, i].ResetItem();
                else
                    HotBarSlots[0, i].ResetItem();
            }

            for (int index = 9, i = 0; i < AdditionalSlots.GetLength(0); i++)
            {
                for (int j = 0; j < AdditionalSlots.GetLength(1); j++)
                {
                    if (slots[index] != null)
                        if (slots[index].Item != null)
                            AdditionalSlots[i, j].SetItem(slots[index]);
                        else
                            AdditionalSlots[i, j].ResetItem();
                    else
                        AdditionalSlots[i, j].ResetItem();
                    index++;
                }
            }
        }

        private void CreateSlotsPrefabs()
        {
            for (int i = 0; i < MainSlots.GetLength(1); i++)
            {
                var slot = Instantiate(slotPref, mainSlotsGrid, false);
                MainSlots[0, i] = slot.AddComponent<Slot>();
            }

            for (int i = 0; i < HotBarSlots.GetLength(1); i++)
            {
                var slot = Instantiate(slotPref, hotBarSlotsGrid, false);
                HotBarSlots[0, i] = slot.AddComponent<Slot>();
            }

            for (int i = 0; i < AdditionalSlots.GetLength(0); i++)
            {
                for (int j = 0; j < AdditionalSlots.GetLength(1); j++)
                {
                    var slot = Instantiate(slotPref, additionalSlotsGrid, false);
                    AdditionalSlots[i, j] = slot.AddComponent<Slot>();
                }
            }
        }

        public void UpdateHotBar(int hotBarSelectedSlot)
        {
            for (int i = 0; i < 9; i++)
            {
                HotBarSlots[0, i].SetDefaultColor();
            }

            HotBarSlots[0, hotBarSelectedSlot].SetHighLight();
        }
    }
}