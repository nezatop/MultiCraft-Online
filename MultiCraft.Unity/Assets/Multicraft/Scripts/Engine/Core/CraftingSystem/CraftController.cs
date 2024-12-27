using System.Linq;
using MultiCraft.Scripts.Engine.Core.Inventories;
using MultiCraft.Scripts.Engine.Core.Items;
using MultiCraft.Scripts.Engine.Utils;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.CraftingSystem
{
    public class CraftController : MonoBehaviour
    {
        [SerializeField] private GameObject slotPref;
        [SerializeField] private Transform craftGrid;

        public CraftSlot[,] CraftTable;

        public CraftResultSlot ResultSlot;

        public bool HasrResultItem => ResultSlot.Item != null;

        public void Init()
        {
            CraftTable = new CraftSlot[3, 3];

            CreateSlotsPrefabs();
        }

        private void CreateSlotsPrefabs()
        {
            for (int i = 0; i < CraftTable.GetLength(0); i++)
            for (int j = 0; j < CraftTable.GetLength(1); j++)
            {
                var slot = Instantiate(slotPref, craftGrid, false);
                CraftTable[i, j] = slot.AddComponent<CraftSlot>();
            }
        }

        public void CheckCraft()
        {
            ItemInSlot newItem = null;

            int currentRecipeW = 0;
            int currentRecipeH = 0;
            int currentRecipeWStartIndex = -1;
            int currentRecipeHStartIndex = -1;

            for (int i = 0; i < CraftTable.GetLength(0); i++)
            for (int j = 0; j < CraftTable.GetLength(1); j++)
                if (CraftTable[i, j].hasItem)
                {
                    if (currentRecipeHStartIndex == -1)
                        currentRecipeHStartIndex = i;
                    currentRecipeH++;
                    break;
                }

            for (int i = 0; i < CraftTable.GetLength(0); i++)
            for (int j = 0; j < CraftTable.GetLength(1); j++)
                if (CraftTable[j, i].hasItem)
                {
                    if (currentRecipeWStartIndex == -1)
                        currentRecipeWStartIndex = i;
                    currentRecipeW++;
                    break;
                }

            var CraftOrder = new Item[currentRecipeH * currentRecipeW];
            
            for (int orderId = 0, i = currentRecipeHStartIndex; i < currentRecipeHStartIndex + currentRecipeH; i++)
            for (int j = currentRecipeWStartIndex; j < currentRecipeWStartIndex + currentRecipeW; j++)
            {
                CraftOrder[orderId++] = CraftTable[i, j].Item?.Item;
            }

            foreach (var item in ResourceLoader.Instance.GetItems().Values)
            {
                if (item.HasRecipe)
                {
                    if(item.CraftRecipe.GetCraftRecipe().SequenceEqual(CraftOrder))
                    {
                        newItem = new ItemInSlot(item, item.CraftRecipe.Amount);
                        break;
                    }
                }
            }

            if (newItem!=null)
            {
                ResultSlot.SetItem(newItem);
            }
            else
            {
                ResultSlot.ResetItem();
            }
        }

        public void CraftItem()
        {
            for (int i = 0; i < CraftTable.GetLength(0); i++)
            for (int j = 0; j < CraftTable.GetLength(1); j++)
                if (CraftTable[i, j].Item != null)
                    CraftTable[i, j].DecreaseItemAmount(1);

            CheckCraft();
        }
    }
}