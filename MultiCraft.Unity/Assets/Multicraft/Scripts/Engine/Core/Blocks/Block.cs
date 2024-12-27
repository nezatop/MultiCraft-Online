using System.Collections.Generic;
using MultiCraft.Scripts.Engine.Core.Items;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Blocks
{
    public class Block
    {
        public int Id = -1;
        public string Name = "Empty";
        
        public bool IsFlora = false;
        
        public ToolType ToolToDig;
        public int MinToolLevelToDig = 0;
        public int Durability = -1;
        
        public bool IsTransparent = false;
        
        public bool HaveInventory = false;
        
        public ItemScriptableObject DropItem;

        private List<Vector2> _uvs = new List<Vector2>();
        
        public AudioClip StepSound;
        public AudioClip DigSound;
        public AudioClip PlaceSound;
        
        public Vector2 GetUvsPixelsOffset(Vector3Int normal)
        {
            if (normal == Vector3Int.left) return _uvs[1];
            if (normal == Vector3Int.right) return _uvs[2];
            if (normal == Vector3Int.forward) return _uvs[3];
            if (normal == Vector3Int.back) return _uvs[4];
            if (normal == Vector3Int.down) return _uvs[5];
            return _uvs[0];
        }

        public void SetUvs(List<Vector2> uv)
        {
            _uvs = uv;
        }
    }
}