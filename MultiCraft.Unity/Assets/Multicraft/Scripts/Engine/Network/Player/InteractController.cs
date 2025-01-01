using System;
using System.Collections.Generic;
using MultiCraft.Scripts.Engine.Core.Player;
using MultiCraft.Scripts.Engine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MultiCraft.Scripts.Engine.Network.Player
{
    public class InteractController : MonoBehaviour
    {
        
        public List<MonoBehaviour> ScriptsToDisable = null;
        public Camera mainCamera;

        private InputSystem_Actions _inputSystem;

        private bool _activeInventory = false;
        private bool _activeChat = false;
        
        private void Awake()
        {
            _inputSystem = new InputSystem_Actions();
            _inputSystem.Enable();
            
            
            _inputSystem.Player.Enable();
            _inputSystem.UI.Enable();
        }

        private void OnEnable()
        {
            _inputSystem.Player.OpenChat.performed += OpenChat;
            _inputSystem.Player.OpenInventory.performed += OpenInventory;
        }

        private void OnDisable()
        {
            _inputSystem.Player.OpenChat.performed -= OpenChat;
            _inputSystem.Player.OpenInventory.performed -= OpenInventory;  
        }

        private void OnDestroy()
        {
            _inputSystem.Player.Disable();
            _inputSystem.UI.Disable();
            
            _inputSystem.Disable();
        }

        public void EnableScripts()
        {
            foreach (var script in ScriptsToDisable)
            {
                script.enabled = true;
            }

            mainCamera.GetComponent<CameraController>().enabled = true;
            mainCamera.GetComponent<DestroyAndPlaceBlockController>().enabled = true;
            //mainCamera.GetComponent<HighLightController>().enabled = true;
        }

        public void DisableScripts()
        {
            foreach (var script in ScriptsToDisable)
            {
                script.enabled = false;
            }

            mainCamera.GetComponent<CameraController>().enabled = false;
            mainCamera.GetComponent<DestroyAndPlaceBlockController>().enabled = false;
            //mainCamera.GetComponent<HighLightController>().enabled = false;
        }
        
        private void OpenInventory(InputAction.CallbackContext obj)
        {
            if(_activeChat) return;
            _activeInventory = !_activeInventory;
            foreach (var script in ScriptsToDisable)
            {
                script.enabled = !script.enabled;
            }

            mainCamera.GetComponent<CameraController>().enabled = !mainCamera.GetComponent<CameraController>().enabled;
            mainCamera.GetComponent<DestroyAndPlaceBlockController>().enabled =
                !mainCamera.GetComponent<DestroyAndPlaceBlockController>().enabled;
            //mainCamera.GetComponent<HighLightController>().enabled =
             //   !mainCamera.GetComponent<HighLightController>().enabled;

            UiManager.Instance.OpenCloseInventory();
        }
        
        private void OpenChat(InputAction.CallbackContext obj)
        {
            if(_activeInventory) return;
            _activeChat = !_activeChat;
            foreach (var script in ScriptsToDisable)
            {
                script.enabled = !script.enabled;
            }

            mainCamera.GetComponent<CameraController>().enabled = !mainCamera.GetComponent<CameraController>().enabled;
            mainCamera.GetComponent<DestroyAndPlaceBlockController>().enabled =
                !mainCamera.GetComponent<DestroyAndPlaceBlockController>().enabled;
            //mainCamera.GetComponent<HighLightController>().enabled =
            //   !mainCamera.GetComponent<HighLightController>().enabled;

            UiManager.Instance.OpenCloseChat();
        }
    }
}