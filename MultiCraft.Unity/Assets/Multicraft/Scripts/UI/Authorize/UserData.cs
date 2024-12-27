using System;

namespace MultiCraft.Scripts.UI.Authorize
{
    [Serializable]
    public class UserData
    {
        public string username;
        public string password;

        public UserData(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
}