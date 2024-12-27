using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace MultiCraft.Scripts.UI.Authorize
{
    public class ChangeData : MonoBehaviour
    {
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public TMP_Text messageText;

        public void ChangeUsername(ref UserData userData)
        {
            string oldUsername = userData.username;
            string newUsername = usernameInput.text.Trim(); 
            string newPassword = passwordInput.text.Trim();

            if (newUsername.Length < 6 || newUsername.Length > 15 || newPassword.Length < 6 || newPassword.Length > 12)
            {
                messageText.text = "Ник должен быть от 6 до 15 символов, а пароль от 6 до 12 символов.";
                return;
            }
            userData.username = newUsername;
            UserDataUpdate updatedUserData = new UserDataUpdate(oldUsername, newUsername, newPassword);
            StartCoroutine(SendChangeDataRequest(updatedUserData));
        }

        public IEnumerator SendChangeDataRequest(UserDataUpdate updatedUserData)
        {
            string jsonData = JsonUtility.ToJson(updatedUserData);
            Debug.Log("Отправляем JSON: " + jsonData);

            using (UnityWebRequest www =
                   new UnityWebRequest("https://multicraftservices-1.onrender.com/edit-username-password", "PUT"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                
                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    messageText.text = "Ошибка: " + www.error;
                }
                else
                {
                    if (www.responseCode == 200)
                    {
                        messageText.text = "Данные успешно обновлены!";
                    }
                    else
                    {
                        Debug.LogError($"Ошибка {www.responseCode}: " + www.downloadHandler.text);
                        messageText.text = "Ошибка: " + www.downloadHandler.text;
                    }
                }
            }
        }
    }
}