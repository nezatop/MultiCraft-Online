using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiCraft.Scripts.Engine.Core.Blocks;
using MultiCraft.Scripts.Engine.Core.Items;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Utils
{
    public class ResourceLoader : MonoBehaviour
    {
        public static ResourceLoader Instance;

        public List<BlockScriptableObject> BlocksToLoad = new List<BlockScriptableObject>();
        public List<ItemScriptableObject> ItemsToLoad = new List<ItemScriptableObject>();

        private Dictionary<string, Item> _items;
        private Dictionary<int, Block> _blocks;
        public TextureData TextureData = new TextureData();

        public Material worldMaterial;

        public Dictionary<string, Item> getItems => _items;
        public Dictionary<int, Block> getBlocks => _blocks;

        public IEnumerator Initialize()
        {
            Instance = this;

            LoadItems();
            LoadBlocks();
            worldMaterial.mainTexture = TextureData.atlas;

            //SaveAtlasToFile(TextureData.atlas, "Assets/Debug/Blocks.png");
            yield return null;
        }

        private void LoadBlocks()
        {
            _blocks = new Dictionary<int, Block>();

            HashSet<Texture2D> texture2Ds = new HashSet<Texture2D>();
            foreach (var blockScriptableObject in BlocksToLoad)
            {
                texture2Ds.Add(blockScriptableObject.Textures[0]);
                texture2Ds.Add(blockScriptableObject.Textures[1]);
                texture2Ds.Add(blockScriptableObject.Textures[2]);
                texture2Ds.Add(blockScriptableObject.Textures[3]);
                texture2Ds.Add(blockScriptableObject.Textures[4]);
                texture2Ds.Add(blockScriptableObject.Textures[5]);
            }

            TextureData.atlas =
                new Texture2D(TextureData.AtlasSize, TextureData.AtlasSize, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Repeat
                };


            var currentX = 0;
            var currentY = 0;

            foreach (var texture in texture2Ds)
            {
                TextureData.atlas.SetPixels(currentX * TextureData.TextureResolution,
                    currentY * TextureData.TextureResolution,
                    TextureData.TextureResolution, TextureData.TextureResolution,
                    texture.GetPixels());

                TextureData.TexturesPositionInAtlas[texture.name] = new Vector2Int(currentX, currentY);

                currentX++;
                if (currentX >= TextureData.AtlasSize / TextureData.TextureResolution)
                {
                    currentX = 0;
                    currentY++;
                }
            }

            TextureData.atlas.Apply(updateMipmaps: false);

            foreach (var blockScriptableObject in BlocksToLoad)
            {
                var block = new Block
                {
                    Id = blockScriptableObject.Id,
                    Name = blockScriptableObject.name,
                    HaveInventory = blockScriptableObject.HaveInventory,
                    IsTransparent = blockScriptableObject.IsTransparent,
                    IsFlora = blockScriptableObject.IsFlora,
                    ToolToDig = blockScriptableObject.ToolToDig,
                    MinToolLevelToDig = blockScriptableObject.MinToolLevelToDig,
                    Durability = blockScriptableObject.Durability,
                    DropItem = blockScriptableObject.DropItem,
                    StepSound = blockScriptableObject.StepSound,
                    DigSound = blockScriptableObject.DigSound,
                    PlaceSound = blockScriptableObject.PlaceSound
                };

                block.SetUvs(GetUv(blockScriptableObject.Textures));

                _blocks.Add(block.Id, block);
            }
        }

        private List<Vector2> GetUv(Texture2D[] textures)
        {
            return textures.Select(texture => TextureData.TexturesPositionInAtlas[texture.name])
                .Select(dummy => (Vector2)dummy)
                .ToList();
        }

        private void LoadItems()
        {
            _items = new Dictionary<string, Item>();
            foreach (var item in ItemsToLoad)
            {
                var itemToLoad = new Item();
                itemToLoad.Name = item.Name;
                itemToLoad.Icon = item.Icon;
                itemToLoad.CraftRecipe = item.CraftRecipe;
                itemToLoad.Type = item.Type;
                itemToLoad.MaxStackSize = item.MaxStackSize;
                itemToLoad.IsPlaceable = item.IsPlaceable;
                itemToLoad.BlockType = item.BlockType;
                itemToLoad.MaxDurability = item.MaxDurability;
                itemToLoad.Damage = item.Damage;
                itemToLoad.ToolType = item.ToolType;
                itemToLoad.MiningLevel = item.MiningLevel;
                itemToLoad.MiningMultiplier = item.MiningMultiplier;
                itemToLoad.Defense = item.Defense;
                itemToLoad.ArmorSlot = item.ArmorSlot;
                itemToLoad.HungerRestoration = item.HungerRestoration;
                itemToLoad.Saturation = item.Saturation;

                _items.Add(itemToLoad.Name, itemToLoad);
            }
        }

        public Block GetBlock(int id)
        {
            return _blocks.GetValueOrDefault(id);
        }

        public Item GetItem(string id)
        {
            return _items.GetValueOrDefault(id);
        }

        public Item GetItem(ItemScriptableObject itemScriptableObject)
        {
            if (itemScriptableObject == null) return null;
            return GetItem(itemScriptableObject.Name);
        }

        public Dictionary<string, Item> GetItems()
        {
            return _items;
        }


        private void SaveAtlasToFile(Texture2D atlas, string path)
        {
            var bytes = atlas.EncodeToPNG();

            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                if (directory != null) Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(path, bytes);
        }

        public Item GetItem(int blockId)
        {
            return GetItem(GetBlock(blockId).DropItem);
        }
    }

    [Serializable]
    public class TextureData
    {
        public int AtlasSize = 256;
        public int TextureResolution = 16;

        public Texture2D atlas;
        public Dictionary<string, Vector2Int> TexturesPositionInAtlas = new Dictionary<string, Vector2Int>();
    }
}