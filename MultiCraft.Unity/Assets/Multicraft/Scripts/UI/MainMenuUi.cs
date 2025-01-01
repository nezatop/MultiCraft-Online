using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiCraft.Scripts.UI
{
    public class MainMenuUi : MonoBehaviour
    {
        public ProfileUI ProfilePanel;
        public GameObject SettingsPanel;

        private void Awake()
        {
            CloseAllPanels();
        }

        public void StartSingleGame()
        {
            SceneManager.LoadScene("Boot");
            SceneManager.LoadScene("Gameplay");
        }
        
        public void StartMultiGame()
        {
            if (ProfilePanel.Auth)
            {
                SceneManager.LoadScene("Boot");
                SceneManager.LoadScene("Multiplayer");
            }
            else
            {
                ToggleProfileHandler();
                ProfilePanel.OpenLoginWindow();
            }
        }

        public void ToggleProfileHandler()
        {
            ProfilePanel.gameObject.SetActive(!ProfilePanel.gameObject.activeSelf);
        }

        public void ToggleSettingsHandler()
        {
            SettingsPanel.SetActive(!SettingsPanel.activeSelf);
        }

        public void CloseAllPanels()
        {
            SettingsPanel.SetActive(false);
            ProfilePanel.gameObject.SetActive(false);
        }
    }
}