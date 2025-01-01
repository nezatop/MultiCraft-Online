using System;
using MultiCraft.Scripts.UI.Authorize;
using TMPro;
using Unity.Collections;
using UnityEngine;

namespace MultiCraft.Scripts.UI
{
    public class ProfileUI : MonoBehaviour
    {
        public GameObject RegisterWindow;
        public GameObject LoginWindow;
        public GameObject Authorized;
        public GameObject ChangeData;

        public TMP_Text Username;
        public TMP_Text MainMenuUsername;

        public Login login;
        public Registration registration;
        public ChangeData changeData;

        public bool Auth = false;
        private UserData userData;

        private void OnEnable()
        {
            login.OnLoginSuccess += OpenAuthorizedWindow;
            login.OnLoginSuccess += SuccessfullyAuthorized;
            registration.OnRegisterSuccess += OpenAuthorizedWindow;
            registration.OnRegisterSuccess += SuccessfullyAuthorized;

            if (!Auth)
                OpenLoginWindow();
            else
                OpenAuthorizedWindow();
        }

        private void SuccessfullyAuthorized()
        {
            string jsonData = JsonUtility.ToJson(userData);

            // Сохраняем JSON-строку в PlayerPrefs
            PlayerPrefs.SetString("UserData", jsonData);
            PlayerPrefs.Save();

            // Выводим для проверки
            Debug.Log($"UserData сохранено: {jsonData}");
            Auth = true;
        }

        private void OnDisable()
        {
            login.OnLoginSuccess -= OpenAuthorizedWindow;
            registration.OnRegisterSuccess -= OpenAuthorizedWindow;
            login.OnLoginSuccess -= SuccessfullyAuthorized;
            registration.OnRegisterSuccess -= SuccessfullyAuthorized;
        }

        public void OpenRegisterWindow()
        {
            RegisterWindow.SetActive(true);
            LoginWindow.SetActive(false);
            Authorized.SetActive(false);
            ChangeData.SetActive(false);
        }

        public void OpenLoginWindow()
        {
            RegisterWindow.SetActive(false);
            LoginWindow.SetActive(true);
            Authorized.SetActive(false);
            ChangeData.SetActive(false);
        }

        public void OpenAuthorizedWindow()
        {
            Username.text = userData.username;
            MainMenuUsername.text = userData.username;
            RegisterWindow.SetActive(false);
            LoginWindow.SetActive(false);
            Authorized.SetActive(true);
            ChangeData.SetActive(false);
        }

        public void OpenChangeDataWindow()
        {
            RegisterWindow.SetActive(false);
            LoginWindow.SetActive(false);
            Authorized.SetActive(false);
            ChangeData.SetActive(true);
        }

        public void Change()
        {
            changeData.ChangeUsername(ref userData);
        }

        public void Register()
        {
            registration.Register(ref userData);
        }

        public void Login()
        {
            login.StartLogin(ref userData);
        }
    }
}