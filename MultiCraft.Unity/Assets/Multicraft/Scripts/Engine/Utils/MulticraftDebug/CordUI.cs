using System;
using MultiCraft.Scripts.Engine.Core.Worlds;
using TMPro;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Utils.MulticraftDebug
{
    public class CordUI : MonoBehaviour
    {
        public TMP_Text text;

        private void OnEnable()
        {
            World.Instance.OnPlayerMove += SetText;
        }

        private void OnDisable()
        {
            World.Instance.OnPlayerMove -= SetText;
        }

        private void SetText(Vector3Int position)
        { 
            text.text = $"X:{Mathf.RoundToInt(position.x)} Y:{Mathf.RoundToInt(position.y)} Z:{Mathf.RoundToInt(position.z)}";
        }
    }
}