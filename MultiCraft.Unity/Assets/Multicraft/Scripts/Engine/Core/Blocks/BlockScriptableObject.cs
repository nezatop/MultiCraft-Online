using System.Collections.Generic;
using MultiCraft.Scripts.Engine.Core.Items;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Blocks
{
    [CreateAssetMenu(fileName = "Block", menuName = "MultiCraft/Block")]
    public class BlockScriptableObject : ScriptableObject
    {
        public int Id;
        public string Name;
        
        public bool IsFlora = false;
        
        public ToolType ToolToDig;
        public int MinToolLevelToDig = 0;
        public int Durability = -1;
        
        public bool IsTransparent = false;
        
        public bool HaveInventory = false;
        
        public ItemScriptableObject DropItem;

        public Texture2D[] Textures = new Texture2D[6];//top left right forward back down
        
        
        public AudioClip StepSound;
        public AudioClip DigSound;
        public AudioClip PlaceSound;
    }
}