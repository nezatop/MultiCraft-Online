using System;

namespace MultiCraft.Scripts.UI.Authorize
{
    [Serializable]
    public class UserDataUpdate
    {
        public string old_username;
        public string new_username;
        public string password;

        public UserDataUpdate(string oldUsername, string newUsername, string password)
        {
            this.old_username = oldUsername;
            this.new_username = newUsername;
            this.password = password;
        }
    }
}