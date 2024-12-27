using System.Collections.Generic;
using MultiCraft.Scripts.Engine.Core.CraftingSystem;
using MultiCraft.Scripts.Engine.Core.Inventories;
using UnityEngine;
using UnityEngine.UI;

namespace MultiCraft.Scripts.Engine.UI
{
    public class InventoryWindow : MonoBehaviour   
    {
        public static InventoryWindow Instance;

        public CraftController CraftController;
        public InventoryController InventoryController;
        public ChestController ChestController;

        [SerializeField] private Image currentItemImage;

        public ItemInSlot CurrentItem;
        public bool HasCurrentItem => CurrentItem != null;

        private void Awake()
        {
            Instance = this;
        }

        public void Open()
        {
            gameObject.SetActive(true);
            CraftController.gameObject.SetActive(true);
        }
        
        public void Close()
        {
            gameObject.SetActive(false);
            CraftController.gameObject.SetActive(false);
        }

        public void OpenChest(List<ItemInSlot> slots, Vector3Int position)
        {
            gameObject.SetActive(true);
            ChestController.gameObject.SetActive(true);
            ChestController.SetItems(slots,position);
        }
        
        public void CloseChest()
        {
            ChestController.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public void SetCurrentItem(ItemInSlot item)
        {
            CurrentItem = item;
            currentItemImage.gameObject.SetActive(true);
            currentItemImage.sprite = CurrentItem.Item.Icon;
        }

        public void ResetCurrentItem()
        {
            CurrentItem = null;
            currentItemImage.gameObject.SetActive(false);
        }

        public void CheckCurrentItem()
        {
            if(CurrentItem != null && CurrentItem.Amount < 1)
                ResetCurrentItem();
        }

        private void Update()
        {
            if(CurrentItem == null)
                return;

            currentItemImage.transform.position = Input.mousePosition;
        }
    }
}