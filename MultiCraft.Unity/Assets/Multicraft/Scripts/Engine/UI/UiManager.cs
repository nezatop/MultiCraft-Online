using System;
using System.Collections.Generic;
using System.Linq;
using MultiCraft.Scripts.Engine.Core.HealthSystem;
using Multicraft.Scripts.Engine.Core.Hunger;
using MultiCraft.Scripts.Engine.Core.Inventories;
using MultiCraft.Scripts.Engine.Core.Player;
using MultiCraft.Scripts.Engine.Core.Worlds;
using MultiCraft.Scripts.Engine.Utils.Commands;
using MultiCraft.Scripts.Engine.Utils.MulticraftDebug;
using Unity.VisualScripting;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.UI
{
    public class UiManager : MonoBehaviour
    {
        public static UiManager Instance;

        public InventoryWindow InventoryWindow;
        public ChatWindow ChatWindow;
        public GameObject LoadingScreen;

        public HealthView HealthView;
        public HungerUI hungerUI;
        
        public bool inventoryUpdated = false;
        
        public PlayerController PlayerController;

        private Vector3Int _chestPosition;

        public CordUI cordUI;
        
        public bool chatWindowOpen = false;
        
        private void Awake()
        {
            Instance = this;
        }

        public void Initialize()
        {
            InventoryWindow.CraftController.Init();
            InventoryWindow.InventoryController.Init();
            InventoryWindow.ChestController.Init();

            LoadingScreen.SetActive(true);

            OpenInventory();
            CloseInventory();
            OpenChat();
            CloseChat();
            HealthView.InitializeHealth(PlayerController.gameObject.GetComponent<Health>());
            hungerUI.InitializeHealth(PlayerController.gameObject.GetComponent<HungerSystem>());
            
            cordUI.gameObject.SetActive(true);
        }
        
        public void OpenCloseInventory()
        {
            if (InventoryWindow.gameObject.activeSelf)
            {
                CloseInventory();
                CloseChest();
                inventoryUpdated = true;
            }
            else
            {
                OpenInventory();
            }
        }
        
        public void OpenCloseChat()
        {
            if (ChatWindow.gameObject.activeSelf)
            {
                CloseChat();
                inventoryUpdated = true;
            }
            else
            {
                OpenChat();
            }
        }
        
        private void OpenChat()
        {
            chatWindowOpen = true;
            ChatWindow.Open();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        private void CloseChat()
        {
            chatWindowOpen = false;
            ChatWindow.Close();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        
        private void OpenInventory()
        {
            InventoryWindow.Open();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        private void CloseInventory()
        {
            InventoryWindow.Close();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OpenCloseChest(List<ItemInSlot> slots, Vector3Int position)
        {
            InventoryWindow.gameObject.SetActive(true);
            OpenChest(slots, position);
            inventoryUpdated = true;
        }

        public void OpenChest(List<ItemInSlot> slots,Vector3Int position)
        {
            InventoryWindow.OpenChest(slots, position);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        public void CloseChest()
        {
            InventoryWindow.CloseChest();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UpdateInventory(List<ItemInSlot> slots)
        {
            InventoryWindow.InventoryController.UpdateUI(slots);
        }

        public void CloseLoadingScreen()
        {
            LoadingScreen.SetActive(false);
        }
    }
}