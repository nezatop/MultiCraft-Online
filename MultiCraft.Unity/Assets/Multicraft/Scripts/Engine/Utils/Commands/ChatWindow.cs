using UnityEngine;

namespace MultiCraft.Scripts.Engine.Utils.Commands
{
    public class ChatWindow : MonoBehaviour
    {
        public CommandReader commandReader;
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}