using System.Collections.Generic;
using MultiCraft.Scripts.Engine.Core.Biomes;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Worlds
{
    [CreateAssetMenu(fileName = "WorldGenerator", menuName = "MultiCraft/WorldGenerator")]
    public class WorldGenerator : ScriptableObject
    {
        [Header("Settings")] public int BaseHeight = 64;
        public int WaterLevel = 63;
        public float RiverChance = 0f;
        public float TreeFrequency = 0.5f;

        [Header("Biomes")] public Biome[] Biomes;
        private Biome[] _biomes;
        private float _biomesChance = -1f;

        [Header("Noises Settings")] public NoiseOctaveSetting BiomeNoiseOctaves;
        private FastNoiseLite _biomeNoise;


        public NoiseOctaveSetting FloraNoiseOctaves;
        private FastNoiseLite _floraNoise;

        public NoiseOctaveSetting[] TreeNoiseOctaves;
        private FastNoiseLite[] _treeNoise;


        public NoiseOctaveSetting WaterNoiseOctaves;
        private FastNoiseLite _waterNoise;

        public NoiseOctaveSetting[] SurfaceNoiseOctaves;
        private FastNoiseLite[] _surfaceNoise;


        public void Initialize(int seed)
        {
            InitializeNoises(seed);
            InitializeBiomes();
        }

        private void InitializeBiomes()
        {
            _biomesChance = 0f;
            _biomes = new Biome[Biomes.Length];
            for (int i = 0; i < _biomes.Length; i++)
            {
                _biomes[i] = new Biome(Biomes[i]);
                _biomesChance += _biomes[i].biomeChance;
                _biomes[i].biomeChance = _biomesChance;
            }

            for (int i = 0; i < _biomes.Length; i++)
            {
                _biomes[i].biomeChance /= _biomesChance;
            }
        }

        public void InitializeNoises(int seed)
        {
            _surfaceNoise = new FastNoiseLite[SurfaceNoiseOctaves.Length];
            for (var i = 0; i < SurfaceNoiseOctaves.Length; i++)
            {
                _surfaceNoise[i] = InitializeNoise(SurfaceNoiseOctaves[i], seed);
            }

            _biomeNoise = InitializeNoise(BiomeNoiseOctaves, seed);
            _waterNoise = InitializeNoise(WaterNoiseOctaves, seed);
            _floraNoise = InitializeNoise(FloraNoiseOctaves, seed);

            _treeNoise = new FastNoiseLite[TreeNoiseOctaves.Length];
            for (var i = 0; i < TreeNoiseOctaves.Length; i++)
            {
                _treeNoise[i] = InitializeNoise(TreeNoiseOctaves[i], seed);
            }
        }

        private FastNoiseLite InitializeNoise(NoiseOctaveSetting setting, int seed)
        {
            FastNoiseLite noise = new FastNoiseLite(seed);
            noise.SetNoiseType(setting.NoiseType);
            noise.SetFrequency(setting.Frequency);

            noise.SetFractalType(setting.FractalType);
            noise.SetFractalOctaves(setting.FractalOctaves);
            noise.SetFractalGain(setting.FractalGain);

            noise.SetCellularDistanceFunction(setting.CellularDistanceFunction);
            noise.SetCellularReturnType(setting.CellularReturnType);
            noise.SetCellularJitter(setting.CellularJitter);

            noise.SetDomainWarpType(setting.DomainWarpType);
            noise.SetDomainWarpAmp(setting.DomainWarpAmplitude);
            return noise;
        }

        public int[,,] Generate(int xOffset, int yOffset, int zOffset, ref int[,] surface)
        {
            var blocks = new int[World.ChunkWidth, World.ChunkHeight, World.ChunkWidth];

            GenerateSurface(ref blocks, out var surfaceHeight, xOffset, yOffset, zOffset);
            GenerateTrees(ref blocks, surfaceHeight, xOffset, yOffset, zOffset);
            surface = surfaceHeight;

            return blocks;
        }

        private void GenerateTrees(ref int[,,] blocks, int[,] surfaceHeight, int xOffset, int yOffset, int zOffset)
        {
            Biome biome = new Biome(_biomes[0]);

            for (int x = 0; x < World.ChunkWidth; x++)
            {
                for (int z = 0; z < World.ChunkWidth; z++)
                {
                    int height = surfaceHeight[x, z];

                    float treeNoiseValue = GetTreeNoise(x + xOffset, z + zOffset);

                    if (treeNoiseValue > TreeFrequency && height > BaseHeight - 1 && height != -1)
                    {
                        var biomeType = (GetBiomeNoise(x + xOffset, z + zOffset) + 1f) / 2f;
                        for (int b = 0; b < _biomes.Length; b++)
                        {
                            if (biomeType <= _biomes[b].biomeChance)
                            {
                                biome = new Biome(_biomes[b]);
                                break;
                            }
                        }

                        var treeType = Random.Range(0, biome.treeType.Length);
                        var leavesType = treeType;

                        if (biome.treeType.Length == 0) break;

                        int treeHeight = Random.Range(biome.minTreeHeight[treeType], biome.maxTreeHeight[treeType] + 1);
                        int treeChance = Random.Range(1, 100);
                        int foliageRadius;
                        if (biome.treeType[treeType] != 99)
                        {
                            foliageRadius = 3;
                            if (x + foliageRadius is >= 0 and < World.ChunkWidth &&
                                x - foliageRadius is >= 0 and < World.ChunkWidth &&
                                z + foliageRadius is >= 0 and < World.ChunkWidth &&
                                z - foliageRadius is >= 0 and < World.ChunkWidth &&
                                blocks[x, height - 1, z] != 0 &&
                                treeChance <= biome.treeChance[treeType])
                            {
                                int foliageY = height + treeHeight - 1;
                                for (int dy = 0; dy <= foliageRadius; dy++)
                                {
                                    for (int dx = -foliageRadius; dx <= foliageRadius; dx++)
                                    {
                                        for (int dz = -foliageRadius; dz <= foliageRadius; dz++)
                                        {
                                            int distance = dx * dx + dy * dy + dz * dz;
                                            if (distance <= foliageRadius * foliageRadius - 1 &&
                                                foliageY < World.ChunkHeight)
                                            {
                                                if (biome.leavesType[leavesType] != -1)
                                                    blocks[x + dx, foliageY + dy, z + dz] =
                                                        biome.leavesType[leavesType];
                                            }
                                        }
                                    }
                                }

                                for (int dy = 1; dy <= treeHeight; dy++)
                                    blocks[x, height + dy, z] = biome.treeType[treeType];
                            }
                        }
                        else
                        {
                            foliageRadius = 5;
                            if (x + foliageRadius is >= 0 and < World.ChunkWidth &&
                                x - foliageRadius is >= 0 and < World.ChunkWidth &&
                                z + foliageRadius is >= 0 and < World.ChunkWidth &&
                                z - foliageRadius is >= 0 and < World.ChunkWidth &&
                                blocks[x, height - 1, z] != 0 &&
                                treeChance <= biome.treeChance[treeType])
                            {
                                int oddFoliageRadius = 0;
                                int evenFoliageRadius = 0;
                                int ddy = 0;
                                for (int dy = height + treeHeight - 1;
                                     dy >= height + 1 + Random.Range(1, 3);
                                     ddy++, dy -= 1)
                                {
                                    if (ddy % 2 == 1)
                                    {
                                        oddFoliageRadius += 1;
                                        foliageRadius = Mathf.Clamp(oddFoliageRadius, 1, 4);
                                    }
                                    else
                                    {
                                        evenFoliageRadius += 1;
                                        foliageRadius = Mathf.Clamp(evenFoliageRadius + 1, 1, 5);
                                    }

                                    for (int dx = -foliageRadius; dx <= foliageRadius; dx++)
                                    {
                                        for (int dz = -foliageRadius; dz <= foliageRadius; dz++)
                                        {
                                            int distance = dx * dx + dz * dz;
                                            if (distance <= foliageRadius * foliageRadius)
                                            {
                                                if (biome.leavesType[leavesType] != -1)
                                                    blocks[x + dx, dy, z + dz] =
                                                        biome.leavesType[leavesType];
                                            }
                                        }
                                    }
                                }

                                if (biome.leavesType[leavesType] != -1)
                                {
                                    blocks[x, height + treeHeight + 1, z] = biome.leavesType[leavesType];
                                    blocks[x + 1, height + treeHeight, z] = biome.leavesType[leavesType];
                                    blocks[x - 1, height + treeHeight, z] = biome.leavesType[leavesType];
                                    blocks[x, height + treeHeight, z + 1] = biome.leavesType[leavesType];
                                    blocks[x, height + treeHeight, z - 1] = biome.leavesType[leavesType];
                                }

                                for (int dy = 1; dy <= treeHeight; dy++)
                                    blocks[x, height + dy, z] = biome.treeType[treeType];
                            }
                        }
                    }
                }
            }
        }

        private void GenerateSurface(ref int[,,] blocks, out int[,] surfaceHeight, int xOffset, int yOffset,
            int zOffset)
        {
            surfaceHeight = new int[World.ChunkWidth, World.ChunkWidth];

            Biome biome = new Biome(_biomes[0]);

            for (int i = 0; i < World.ChunkWidth; i++)

            for (var x = 0; x < World.ChunkWidth; x++)
            {
                for (var z = 0; z < World.ChunkWidth; z++)
                {
                    var biomeType = (GetBiomeNoise(x + xOffset, z + zOffset) + 1f) / 2f;
                    for (int b = 0; b < _biomes.Length; b++)
                    {
                        if (biomeType <= _biomes[b].biomeChance)
                        {
                            biome = new Biome(_biomes[b]);
                            break;
                        }
                    }

                    var waterChance = _waterNoise.GetNoise(x + xOffset, z + zOffset);
                    var height = GetHeight(x + xOffset, z + zOffset);
                    if (waterChance > RiverChance)
                        height -= waterChance * WaterNoiseOctaves.Amplitude;

                    for (var y = 0; y <= (int)height && y < World.ChunkHeight; y++)
                    {
                        if (y < 1) blocks[x, y, z] = 1;
                        if (y >= 1 && y < height - 3) blocks[x, y, z] = 2;
                        if (y >= height - 3 && y < height - 1)
                            blocks[x, y, z] =
                                blocks[x, y, z] = (waterChance > RiverChance && height <= WaterLevel)
                                    ? 3
                                    : biome.subSurfaceBlock;
                        if (y >= height - 1 && y <= height)
                            blocks[x, y, z] = (waterChance > RiverChance && height <= WaterLevel)
                                ? 3
                                : biome.surfaceBlock;
                    }

                    surfaceHeight[x, z] = waterChance > RiverChance && height <= WaterLevel ? -1 : (int)height;
                }
            }
        }

        public int[,,] GenerateFlora(int xOffset, int yOffset, int zOffset)
        {
            var blocks = new int[World.ChunkWidth, World.ChunkHeight, World.ChunkWidth];
            
            Biome biome = new Biome(_biomes[0]);

            for (var x = 0; x < World.ChunkWidth; x++)
            {
                for (var z = 0; z < World.ChunkWidth; z++)
                {
                    var biomeType = (GetBiomeNoise(x + xOffset, z + zOffset) + 1f) / 2f;
                    for (int b = 0; b < _biomes.Length; b++)
                    {
                        if (biomeType <= _biomes[b].biomeChance)
                        {
                            biome = new Biome(_biomes[b]);
                            break;
                        }
                    }

                    var waterChance = _waterNoise.GetNoise(x + xOffset, z + zOffset);
                    var height = GetHeight(x + xOffset, z + zOffset);
                    if (waterChance > RiverChance)
                        height -= waterChance * WaterNoiseOctaves.Amplitude;
                    var floraChance = _floraNoise.GetNoise(x + xOffset, z + zOffset);
                    if (floraChance <= 0 && biome.placeFlora && (int)height > WaterLevel)
                    {
                        int floraIndex = 27;
                        var floraChances = biome.chances;
                        var fc = 0;
                        var currentFloraChance = Random.Range(0, floraChances);
                        var currentFloraChance2 = Random.Range(0, floraChances);
                        for (int i = 0; i < biome.floraId.Length; i++)
                        {
                            fc += biome.floraChances[i];
                            if (fc >= currentFloraChance)
                            {
                                floraIndex = biome.floraId[i];
                                break;
                            }
                        }

                        if (currentFloraChance2 <= biome.fullFloraChance)
                            blocks[x, (int)height + 1, z] = biome.placeFlora ? floraIndex : 0;
                    }
                }
            }

            return blocks;
        }

        public int[,,] GenerateWater(int xOffset, int yOffset, int zOffset)
        {
            var blocks = new int[World.ChunkWidth, World.ChunkHeight, World.ChunkWidth];
            for (var x = 0; x < World.ChunkWidth; x++)
            {
                for (var z = 0; z < World.ChunkWidth; z++)
                {
                    var waterChance = _waterNoise.GetNoise(x + xOffset, z + zOffset);
                    var height = GetHeight(x + xOffset, z + zOffset);
                    if (waterChance > RiverChance)
                        height -= waterChance * WaterNoiseOctaves.Amplitude;
                    for (var y = (int)height + 1; y <= WaterLevel; y++)
                    {
                        blocks[x, y, z] = -1;
                    }
                }
            }

            return blocks;
        }

        private float GetHeight(float x, float z)
        {
            float result = BaseHeight;

            for (int i = 0; i < _surfaceNoise.Length; i++)
            {
                float noise = _surfaceNoise[i].GetNoise(x, z);
                if (noise > 0)
                    result += noise * SurfaceNoiseOctaves[i].Amplitude / 2;
            }

            return result;
        }

        private float GetBiomeNoise(float x, float z)
        {
            _biomeNoise.DomainWarp(ref x, ref z);

            return _biomeNoise.GetNoise(x, z);
        }

        private float GetTreeNoise(float x, float z)
        {
            float result = 0;

            for (int i = 0; i < _treeNoise.Length; i++)
            {
                float noise = _treeNoise[i].GetNoise(x, z);
                result += noise * TreeNoiseOctaves[i].Amplitude / 2;
            }

            return result;
        }
    }
}