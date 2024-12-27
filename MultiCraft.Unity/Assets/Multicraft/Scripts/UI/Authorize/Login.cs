using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MultiCraft.Scripts.UI.Authorize
{
    public class Login : MonoBehaviour
    {
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public TMP_Text messageText;
    
        public event Action OnLoginSuccess;
        public void StartLogin(ref UserData userData)
        {
            string username = usernameInput.text.Trim();
            string password = passwordInput.text.Trim();

            if (username.Length < 6 || password.Length < 6)
            {
                messageText.text = "Ник и пароль должны быть длиннее 6 символов.";
                return;
            }

            userData = new UserData(username, password);
            StartCoroutine(SendLoginRequest(userData));
        }

        private IEnumerator SendLoginRequest(UserData userData)
        {
            string jsonData = JsonUtility.ToJson(userData);
            Debug.Log("Отправляем JSON: " + jsonData); // Проверка JSON

            using (UnityWebRequest www = new UnityWebRequest("https://multicraftservices-1.onrender.com/login", "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    messageText.text = "Ошибка: " + www.error;
                }
                else
                {
                    if (www.responseCode == 200)
                    {
                        messageText.text = "Успех! Вы вошли в систему.";
                        LoginSuccess();
                    }
                    else
                    {
                        Debug.LogError($"Ошибка {www.responseCode}: " + www.downloadHandler.text);
                        messageText.text = "Ошибка: " + www.downloadHandler.text;
                    }
                }
            }
        }

        private void LoginSuccess()
        {
            Debug.Log("Пользователь успешно авторизован.");
            OnLoginSuccess?.Invoke();
        }
    }
}