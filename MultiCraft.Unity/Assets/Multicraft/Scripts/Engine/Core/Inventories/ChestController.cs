using System;
using System.Collections.Generic;
using System.Linq;
using MultiCraft.Scripts.Engine.Core.Worlds;
using MultiCraft.Scripts.Engine.Network.Worlds;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Inventories
{
    public class ChestController : MonoBehaviour
    {
        public Slot[,] ChestSlots;

        [SerializeField] private GameObject slotPref;
        [SerializeField] private Transform chestSlotsGrid;

        private Vector3Int _chestPosition = new Vector3Int(0, 0, 0);

        public void Init()
        {
            ChestSlots = new Slot[4, 9];

            CreateSlotsPrefabs();
            
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            var chestSlots = new List<ItemInSlot>();
            for (int i = 0; i < ChestSlots.GetLength(0); i++)
            {
                for (int j = 0; j < ChestSlots.GetLength(1); j++)
                {
                    if (ChestSlots[i, j] != null)
                        chestSlots.Add(ChestSlots[i, j].Item);
                    else
                        chestSlots.Add(null);
                }
            }
            if(World.Instance != null)
                World.Instance.UpdateChest(_chestPosition, chestSlots);
            else
                NetworkWorld.instance.UpdateChest(_chestPosition, chestSlots);
            _chestPosition = new Vector3Int(0, 0, 0);
        }

        private void CreateSlotsPrefabs()
        {
            for (int i = 0; i < ChestSlots.GetLength(0); i++)
            {
                for (int j = 0; j < ChestSlots.GetLength(1); j++)
                {
                    var slot = Instantiate(slotPref, chestSlotsGrid, false);
                    ChestSlots[i, j] = slot.AddComponent<Slot>();
                }
            }
        }

        public void SetItems(List<ItemInSlot> slots, Vector3Int chestPosition)
        {
            _chestPosition = chestPosition;
            for (int i = 0; i < ChestSlots.GetLength(0); i++)
            {
                for (int j = 0; j < ChestSlots.GetLength(1); j++)
                {
                    ChestSlots[i, j].ResetItem();
                    if (slots[i * 9 + j] != null)
                        if(slots[i * 9 + j].Item != null)
                            ChestSlots[i, j].SetItem(slots[i * 9 + j]);
                }
            }
        }
    }
}