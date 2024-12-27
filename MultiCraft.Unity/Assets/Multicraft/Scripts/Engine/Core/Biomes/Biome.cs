using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Biomes
{
    [System.Serializable]
    public class Biome
    {
        [Header("General Biome Values")] public string biomeName;
        public float offset;
        public float scale;

        public int terrainHeight;
        public float terrainScale;

        public int surfaceBlock;
        public int subSurfaceBlock;
        
        public int[] treeType;
        public int[] leavesType;
        public int[] maxTreeHeight;
        public int[] minTreeHeight;
        public int[] treeChance;

        [Range(0.0f, 1.0f)] public float biomeChance;
        
        [Header("Flora")] 
        public int fullFloraChance;
        public int floraIndex;
        public float floraZoneScale = 1.3f;
        [Range(0.1f, 1f)] public float floraThreshold = 0.7f;
        public float floraPlacementScale = 30f;
        [Range(0.1f, 1f)] public float floraPlacementThreshold = 0.8f;
        public bool placeFlora = true;
        public int[] floraId;
        public int[] floraChances;

        public int HeightMax = 12;
        public int HeightMin = 4;

        public Lode[] lodes;

        public int chances = 0;

        public Biome(Biome biome)
        {
            this.fullFloraChance = biome.fullFloraChance;
            this.biomeName = biome.biomeName;
            this.offset = biome.offset;
            this.scale = biome.scale;
            this.terrainHeight = biome.terrainHeight;
            this.terrainScale = biome.terrainScale;
            this.surfaceBlock = biome.surfaceBlock;
            this.subSurfaceBlock = biome.subSurfaceBlock;
            this.biomeChance = biome.biomeChance;
            this.floraIndex = biome.floraIndex;
            this.floraZoneScale = biome.floraZoneScale;
            this.floraThreshold = biome.floraThreshold;
            this.floraPlacementScale = biome.floraPlacementScale;
            this.floraPlacementThreshold = biome.floraPlacementThreshold;
            this.placeFlora = biome.placeFlora;
            this.HeightMax = biome.HeightMax;
            this.HeightMin = biome.HeightMin;

            // Копирование массивов, чтобы избежать передачи ссылок
            this.treeType = new int[biome.treeType.Length];
            biome.treeType.CopyTo(this.treeType, 0);
            
            this.maxTreeHeight = new int[biome.maxTreeHeight.Length];
            biome.maxTreeHeight.CopyTo(this.maxTreeHeight, 0);
            
            this.minTreeHeight = new int[biome.minTreeHeight.Length];
            biome.minTreeHeight.CopyTo(this.minTreeHeight, 0);
            
            this.treeChance = new int[biome.treeChance.Length];
            biome.treeChance.CopyTo(this.treeChance, 0);

            this.floraId = new int[biome.floraId.Length];
            biome.floraId.CopyTo(this.floraId, 0);
            
            this.floraChances = new int[biome.floraChances.Length];
            biome.floraChances.CopyTo(this.floraChances, 0);
            
            foreach (var t in biome.floraChances)
                chances += t;

            this.leavesType = new int[biome.leavesType.Length];
            biome.leavesType.CopyTo(this.leavesType, 0);

            // Копирование массива lodes
            this.lodes = new Lode[biome.lodes.Length];
            for (int i = 0; i < biome.lodes.Length; i++)
            {
                // Если Lode имеет конструктор копирования, создайте новый экземпляр
                this.lodes[i] = new Lode(biome.lodes[i]);
            }
        }
    }

    [System.Serializable]
    public class Lode
    {
        public string nodeName;
        public byte blockID;
        public int minHeight;
        public int maxHeight;
        public float scale;
        public float threshold;
        public float noiseOffset;
        
        public Lode(Lode lode)
        {
            this.nodeName = lode.nodeName;
            this.blockID = lode.blockID;
            this.minHeight = lode.minHeight;
            this.maxHeight = lode.maxHeight;
            this.scale = lode.scale;
            this.threshold = lode.threshold;
            this.noiseOffset = lode.noiseOffset;
        }
    }
}