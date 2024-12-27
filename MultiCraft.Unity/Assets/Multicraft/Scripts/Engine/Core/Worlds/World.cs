using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiCraft.Scripts.Engine.Core.Biomes;
using MultiCraft.Scripts.Engine.Core.Blocks;
using MultiCraft.Scripts.Engine.Core.Chunks;
using MultiCraft.Scripts.Engine.Core.Entities;
using MultiCraft.Scripts.Engine.Core.HealthSystem;
using Multicraft.Scripts.Engine.Core.Hunger;
using MultiCraft.Scripts.Engine.Core.Inventories;
using MultiCraft.Scripts.Engine.Core.Items;
using MultiCraft.Scripts.Engine.Core.MeshBuilders;
using MultiCraft.Scripts.Engine.Core.Player;
using MultiCraft.Scripts.Engine.UI;
using MultiCraft.Scripts.Engine.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MultiCraft.Scripts.Engine.Core.Worlds
{
    public class World : MonoBehaviour
    {
        private static World _instance;
        public static World Instance => _instance;

        [Header("User settings")] 
        public UserSettings settings;

        [Header("World")] 
        public string worldName;
        public Texture2D worldIcon;

        [Header("World Settings")] public int seed;

        public const int WorldSize = 2048;
        public const int WorldHeight = 1;

        [Header("Chunk Settings")] public const int ChunkWidth = 16;
        public const int ChunkHeight = 256;


        [Header("Chunks Dictionary")] 
        public Dictionary<Vector3Int, Chunk> Chunks;
        public Dictionary<Vector3Int, Chunk> WaterChunks;
        public Dictionary<Vector3Int, Chunk> FloraChunks;
        private readonly ConcurrentQueue<GeneratedMesh> _meshingResults = new ConcurrentQueue<GeneratedMesh>();
        private readonly ConcurrentQueue<GeneratedMesh> _meshingWaterResults = new ConcurrentQueue<GeneratedMesh>();
        private readonly ConcurrentQueue<GeneratedMesh> _meshingFloraResults = new ConcurrentQueue<GeneratedMesh>();

        private List<GameObject> _players;
        private List<Vector3Int> _currentPlayersPosition;
        private Dictionary<Vector3Int, List<ItemInSlot>> _inventories;

        [Header("Prefabs")]
        public ChunkRenderer chunkPrefab;
        public ChunkRenderer WaterChunkPrefab;
        public ChunkRenderer FloraChunkPrefab;
        public GameObject PlayerPrefab;
        public GameObject mobPrefab;
        public DroppedItem DroppedItemPrefab;

        public WorldGenerator worldGenerator;
        public ResourceLoader resourceLoader;

        public event Action<Vector3Int> OnPlayerMove;

        private HashSet<Vector3Int> _updateChunksPoll = new HashSet<Vector3Int>();
        
        private void Update()
        {
            foreach (var inv in _inventories.Values)
            {
                foreach (var slot in inv)
                {
                    if (slot == null)
                    {
                        break;
                    }
                }
            }
        }

        private void Awake()
        {
            _instance = this;
            StartCoroutine(InitializeWorld());
        }

        private IEnumerator InitializeWorld()
        {

            if (seed == 0)
                seed = Random.Range(int.MinValue, int.MaxValue);

            Chunks = new Dictionary<Vector3Int, Chunk>();
            WaterChunks = new Dictionary<Vector3Int, Chunk>();
            FloraChunks = new Dictionary<Vector3Int, Chunk>();
            _players = new List<GameObject>();
            _currentPlayersPosition = new List<Vector3Int>();
            _inventories = new Dictionary<Vector3Int, List<ItemInSlot>>();

            ChunkRenderer.InitializeTriangles();
            DropItemRenderer.InitializeTriangles();
            HandRenderer.InitializeTriangles();
            worldGenerator.Initialize(seed);
            yield return StartCoroutine(resourceLoader.Initialize());

            _players.AddRange(GameObject.FindGameObjectsWithTag("Player"));


            yield return StartCoroutine(Generate(Vector2Int.zero));
            yield return StartCoroutine(SpawnMobs(Vector2Int.zero));

            SpawnChunks();
            SpawnWaterChunks();
            SpawnFloraChunks();

            var player = Instantiate(PlayerPrefab, new Vector3Int(0, 68, 0), Quaternion.identity);
            UiManager.Instance.PlayerController = player.GetComponent<PlayerController>();
            UiManager.Instance.Initialize();

            _players.Add(player);
            _currentPlayersPosition = _players
                .Select(t => GetChunkContainBlock(Vector3Int.FloorToInt(t.transform.position))).ToList();

            player.GetComponent<Health>().OnDeath += RespawnPlayers;

            UiManager.Instance.CloseLoadingScreen();
        }

        private void FixedUpdate()
        {
            StartCoroutine(CheckPlayers());

            SpawnChunks();
            SpawnWaterChunks();
            SpawnFloraChunks();
        }

        public void RespawnPlayers()
        {
            var inv = _players[0].GetComponent<Inventory>();
            foreach (var itemInSlot in inv.Slots)
            {
                if (itemInSlot != null)
                {
                    if (itemInSlot.Item != null)
                        DropItem(_players[0].transform.position, itemInSlot.Item, itemInSlot.Amount);
                }
            }

            inv.Clear();

            _players[0].GetComponent<PlayerController>().Teleport(new Vector3Int(0, 68, 0));
            var playerHealth = _players[0].GetComponent<Health>();
            var playerHungerSystem = _players[0].GetComponent<HungerSystem>();
            playerHungerSystem.hunger = playerHungerSystem.maxHunger - 3;
            playerHealth.health = playerHealth.maxHealth;
        }

        private void SpawnChunks()
        {
            int chunksAmount = 0;
            while (chunksAmount < 4)
            {
                if (_meshingResults.TryDequeue(out var mesh))
                {
                    var xPos = mesh.Chunk.Position.x * ChunkWidth;
                    var yPos = mesh.Chunk.Position.y * ChunkHeight;
                    var zPos = mesh.Chunk.Position.z * ChunkWidth;

                    var chunkObject = Instantiate(chunkPrefab, new Vector3(xPos, yPos, zPos), Quaternion.identity,
                        transform);
                    chunkObject.Chunk = mesh.Chunk;

                    chunkObject.SetMesh(mesh);

                    mesh.Chunk.Renderer = chunkObject;
                    mesh.Chunk.State = ChunkState.Active;
                }

                chunksAmount++;
            }
        }

        private void SpawnWaterChunks()
        {
            int chunksAmount = 0;
            while (chunksAmount < 4)
            {
                if (_meshingWaterResults.TryDequeue(out var mesh))
                {
                    var xPos = mesh.Chunk.Position.x * ChunkWidth;
                    var yPos = mesh.Chunk.Position.y * ChunkHeight;
                    var zPos = mesh.Chunk.Position.z * ChunkWidth;

                    var chunkObject = Instantiate(WaterChunkPrefab, new Vector3(xPos, yPos, zPos), Quaternion.identity,
                        transform);
                    chunkObject.Chunk = mesh.Chunk;

                    chunkObject.SetMesh(mesh);

                    mesh.Chunk.Renderer = chunkObject;
                    mesh.Chunk.State = ChunkState.Active;
                }

                chunksAmount++;
            }
        }

        private void SpawnFloraChunks()
        {
            int chunksAmount = 0;
            while (chunksAmount < 4)
            {
                if (_meshingFloraResults.TryDequeue(out var mesh))
                {
                    var xPos = mesh.Chunk.Position.x * ChunkWidth;
                    var yPos = mesh.Chunk.Position.y * ChunkHeight;
                    var zPos = mesh.Chunk.Position.z * ChunkWidth;

                    var chunkObject = Instantiate(FloraChunkPrefab, new Vector3(xPos, yPos, zPos), Quaternion.identity,
                        transform);
                    chunkObject.Chunk = mesh.Chunk;

                    chunkObject.SetMesh(mesh);

                    mesh.Chunk.Renderer = chunkObject;
                    mesh.Chunk.State = ChunkState.Active;
                }

                chunksAmount++;
            }
        }

        private IEnumerator CheckPlayers()
        {
            var playerMoved = false;

            var playersPosition = _players
                .Select(t => GetChunkContainBlock(Vector3Int.FloorToInt(t.transform.position))).ToList();

            if (_currentPlayersPosition.Count == 0)
            {
                _currentPlayersPosition = playersPosition;
                playerMoved = true;
            }

            for (var i = 0; i < _players.Count; i++)
            {
                var prevPlayer = _currentPlayersPosition[i];
                var player = playersPosition[i];
                if (prevPlayer.x != player.x || prevPlayer.z != player.z)
                    playerMoved = true;
            }

            if (playerMoved)
            {
                _currentPlayersPosition = playersPosition;
                foreach (var pos in _currentPlayersPosition)
                {
                    yield return StartCoroutine(Generate(new Vector2Int(pos.x, pos.z)));
                    yield return StartCoroutine(SpawnMobs(new Vector2Int(pos.x, pos.z)));
                }
            }
            
            OnPlayerMove?.Invoke(Vector3Int.FloorToInt(_players[0].transform.position));

            yield return null;
        }

        private IEnumerator DisableChunks(Vector2Int center)
        {
            for (int i = 0; i < Chunks.Count; i++)
            {
                var chunk = Chunks.ElementAt(i);
                var chunkPosition = chunk.Key;
                var chunkObject = chunk.Value;
                if (center.x - settings.viewDistanceInChunks > chunkPosition.x ||
                    center.x + settings.viewDistanceInChunks < chunkPosition.x ||
                    center.y - settings.viewDistanceInChunks > chunkPosition.y ||
                    center.y + settings.viewDistanceInChunks < chunkPosition.y)
                {
                    if (chunkObject.State == ChunkState.Active)
                    {
                        chunkObject.Renderer.meshFilter.sharedMesh = null;
                        chunkObject.State = ChunkState.Inactive;
                    }
                }
            }

            yield return null;
        }

        private IEnumerator DisableWaterChunks(Vector2Int center)
        {
            for (int i = 0; i < WaterChunks.Count; i++)
            {
                var chunk = WaterChunks.ElementAt(i);
                var chunkPosition = chunk.Key;
                var chunkObject = chunk.Value;
                if (center.x - settings.viewDistanceInChunks > chunkPosition.x ||
                    center.x + settings.viewDistanceInChunks < chunkPosition.x ||
                    center.y - settings.viewDistanceInChunks > chunkPosition.y ||
                    center.y + settings.viewDistanceInChunks < chunkPosition.y)
                {
                    if (chunkObject.State == ChunkState.Active)
                    {
                        chunkObject.Renderer.meshFilter.sharedMesh = null;
                        chunkObject.State = ChunkState.Inactive;
                    }
                }
            }

            yield return null;
        }

        private IEnumerator Generate(Vector2Int center)
        {
            List<Chunk> loadingChunks = new List<Chunk>();
            List<Chunk> loadingWaterChunks = new List<Chunk>();
            List<Chunk> loadingFloraChunks = new List<Chunk>();

            for (var x = center.x - settings.loadDistance; x <= center.x + settings.loadDistance; x++)
            {
                for (var z = center.y - settings.loadDistance; z <= center.y + settings.loadDistance; z++)
                {
                    for (var y = 0; y < WorldHeight; y++)
                    {
                        var chunkPosition = new Vector3Int(x, y, z);
                        if (Mathf.Abs(x) > WorldSize || Mathf.Abs(z) > WorldSize)
                            continue;

                        if (Chunks.ContainsKey(chunkPosition)) continue;
                        if (WaterChunks.ContainsKey(chunkPosition)) continue;
                        if (FloraChunks.ContainsKey(chunkPosition)) continue;

                        var chunk = GenerateChunk(chunkPosition);
                        var waterChunk = GenerateWaterChunk(chunkPosition);
                        var floraChunk = GenerateFloraChunk(chunkPosition);

                        loadingChunks.Add(chunk);
                        loadingWaterChunks.Add(waterChunk);
                        loadingFloraChunks.Add(floraChunk);

                        yield return null;
                    }
                }
            }

            while (loadingChunks.Any(c => c.State == ChunkState.Generating))
                yield return null;

            while (loadingWaterChunks.Any(c => c.State == ChunkState.Generating))
                yield return null;

            while (loadingFloraChunks.Any(c => c.State == ChunkState.Generating))
                yield return null;

            for (var x = center.x - settings.viewDistanceInChunks;
                 x <= center.x + settings.viewDistanceInChunks;
                 x++)
            {
                for (var z = center.y - settings.viewDistanceInChunks;
                     z <= center.y + settings.viewDistanceInChunks;
                     z++)
                {
                    for (var y = 0; y < WorldHeight; y++)
                    {
                        var chunkPosition = new Vector3Int(x, y, z);

                        if (Mathf.Abs(x) > WorldSize || Mathf.Abs(z) > WorldSize)
                            continue;

                        var chunk = Chunks[chunkPosition];
                        var waterChunk = WaterChunks[chunkPosition];
                        var floraChunk = FloraChunks[chunkPosition];

                        if (chunk.State == ChunkState.Inactive)
                        {
                            chunk.Renderer.meshFilter.sharedMesh = chunk.Renderer.mesh;
                            chunk.State = ChunkState.Active;
                        }

                        if (waterChunk.State == ChunkState.Inactive)
                        {
                            waterChunk.Renderer.meshFilter.sharedMesh = waterChunk.Renderer.mesh;
                            waterChunk.State = ChunkState.Active;
                        }

                        if (chunk.Renderer) continue;
                        if (waterChunk.Renderer) continue;
                        if (floraChunk.Renderer) continue;

                        SpawnChunk(chunk);
                        SpawnWaterChunk(waterChunk);
                        SpawnFloraChunk(floraChunk);

                        yield return null;
                    }
                }
            }
        }

        private void SpawnChunk(Chunk chunk)
        {
            Chunks.TryGetValue(chunk.Position + Vector3Int.left, out chunk.LeftChunk);
            Chunks.TryGetValue(chunk.Position + Vector3Int.right, out chunk.RightChunk);
            Chunks.TryGetValue(chunk.Position + Vector3Int.up, out chunk.UpChunk);
            Chunks.TryGetValue(chunk.Position + Vector3Int.down, out chunk.DownChunk);
            Chunks.TryGetValue(chunk.Position + Vector3Int.forward, out chunk.FrontChunk);
            Chunks.TryGetValue(chunk.Position + Vector3Int.back, out chunk.BackChunk);

            //Task.Factory.StartNew(() =>
            //          {
            chunk.State = ChunkState.MeshBuilding;
            var mesh = MeshBuilder.GenerateMesh(chunk);
            _meshingResults.Enqueue(mesh);
            chunk.State = ChunkState.Loaded;
            //});
        }

        private void SpawnWaterChunk(Chunk chunk)
        {
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.left, out chunk.LeftChunk);
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.right, out chunk.RightChunk);
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.up, out chunk.UpChunk);
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.down, out chunk.DownChunk);
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.forward, out chunk.FrontChunk);
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.back, out chunk.BackChunk);

            //Task.Factory.StartNew(() =>
            //          {
            chunk.State = ChunkState.MeshBuilding;
            var mesh = MeshBuilder.GenerateMesh(chunk);
            _meshingWaterResults.Enqueue(mesh);
            chunk.State = ChunkState.Loaded;
            //});
        }

        private void SpawnFloraChunk(Chunk chunk)
        {
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.left, out chunk.LeftChunk);
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.right, out chunk.RightChunk);
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.up, out chunk.UpChunk);
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.down, out chunk.DownChunk);
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.forward, out chunk.FrontChunk);
            WaterChunks.TryGetValue(chunk.Position + Vector3Int.back, out chunk.BackChunk);

            //Task.Factory.StartNew(() =>
            //          {
            chunk.State = ChunkState.MeshBuilding;
            var mesh = MeshBuilder.GenerateMesh(chunk);
            _meshingFloraResults.Enqueue(mesh);
            chunk.State = ChunkState.Loaded;
            //});
        }

        private Chunk GenerateChunk(Vector3Int chunkPosition)
        {
            var xPos = chunkPosition.x * ChunkWidth;
            var yPos = chunkPosition.y * ChunkHeight;
            var zPos = chunkPosition.z * ChunkWidth;

            var chunk = new Chunk
            {
                Position = chunkPosition,
                State = ChunkState.Generating
            };

            Chunks.Add(chunkPosition, chunk);
            //Task.Factory.StartNew(() =>
            //          {
            chunk.Blocks = worldGenerator.Generate(xPos, yPos, zPos, ref chunk.SurfaceHeight);
            chunk.State = ChunkState.Generated;
            //});

            return chunk;
        }

        private Chunk GenerateWaterChunk(Vector3Int chunkPosition)
        {
            var xPos = chunkPosition.x * ChunkWidth;
            var yPos = chunkPosition.y * ChunkHeight;
            var zPos = chunkPosition.z * ChunkWidth;

            var chunk = new Chunk
            {
                Position = chunkPosition,
                State = ChunkState.Generating
            };

            WaterChunks.Add(chunkPosition, chunk);
            //Task.Factory.StartNew(() =>
            //          {
            chunk.Blocks = worldGenerator.GenerateWater(xPos, yPos, zPos);
            chunk.State = ChunkState.Generated;
            //});

            return chunk;
        }

        private Chunk GenerateFloraChunk(Vector3Int chunkPosition)
        {
            var xPos = chunkPosition.x * ChunkWidth;
            var yPos = chunkPosition.y * ChunkHeight;
            var zPos = chunkPosition.z * ChunkWidth;

            var chunk = new Chunk
            {
                Position = chunkPosition,
                State = ChunkState.Generating
            };

            FloraChunks.Add(chunkPosition, chunk);
            //Task.Factory.StartNew(() =>
            //          {
            chunk.Blocks = worldGenerator.GenerateFlora(xPos, yPos, zPos);
            chunk.State = ChunkState.Generated;
            //});

            return chunk;
        }

        private Vector3Int GetChunkContainBlock(Vector3Int blockWorldPosition)
        {
            var chunkPosition = new Vector3Int(
                blockWorldPosition.x / ChunkWidth,
                blockWorldPosition.y / ChunkHeight,
                blockWorldPosition.z / ChunkWidth);

            if (blockWorldPosition.x < 0)
                if (blockWorldPosition.x % ChunkWidth != 0)
                    chunkPosition.x--;
            if (blockWorldPosition.z < 0)
                if (blockWorldPosition.z % ChunkWidth != 0)
                    chunkPosition.z--;

            return chunkPosition;
        }

        public Block GetBlockAtPosition(Vector3 blockPosition)
        {
            var blockWorldPosition = Vector3Int.FloorToInt(blockPosition);
            var chunkPosition = GetChunkContainBlock(Vector3Int.FloorToInt(blockPosition));

            int blockId = 0;
            if (Chunks.TryGetValue(chunkPosition, out var chunk))
            {
                var chunkOrigin = new Vector3Int(chunkPosition.x, chunkPosition.y, chunkPosition.z) * ChunkWidth;
                var blockChunkPosition = blockWorldPosition - chunkOrigin;
                blockId = chunk.Blocks[blockChunkPosition.x, blockChunkPosition.y, blockChunkPosition.z];
            }

            return ResourceLoader.Instance.GetBlock(blockId);
        }
        
        public int GetBlockTypeAtPosition(Vector3 blockPosition)
        {
            var blockWorldPosition = Vector3Int.FloorToInt(blockPosition);
            var chunkPosition = GetChunkContainBlock(Vector3Int.FloorToInt(blockPosition));

            int blockId = 0;
            if (Chunks.TryGetValue(chunkPosition, out var chunk))
            {
                var chunkOrigin = new Vector3Int(chunkPosition.x, chunkPosition.y, chunkPosition.z) * ChunkWidth;
                var blockChunkPosition = blockWorldPosition - chunkOrigin;
                blockId = chunk.Blocks[blockChunkPosition.x, blockChunkPosition.y, blockChunkPosition.z];
            }

            return blockId;
        }


        public void SpawnBlock(Vector3 blockPosition, int blockType)
        {
            if(blockType == 0) return;
            var blockWorldPosition = Vector3Int.FloorToInt(blockPosition);
            var chunkPosition = GetChunkContainBlock(Vector3Int.FloorToInt(blockWorldPosition));

            if (Chunks.TryGetValue(chunkPosition, out var chunk))
            {
                var chunkOrigin = new Vector3Int(chunkPosition.x, chunkPosition.y, chunkPosition.z) * ChunkWidth;
                var blockChunkPosition = blockWorldPosition - chunkOrigin;
                chunk.Renderer.SpawnBlock(blockChunkPosition, blockType);
                if (ResourceLoader.Instance.GetBlock(blockType).HaveInventory)
                {
                    _inventories.Add(blockWorldPosition, Enumerable.Repeat(new ItemInSlot(), 36).ToList());
                }
            }
        }
        
        public void SpawnBlockNoUpdate(Vector3 blockPosition, int blockType)
        {
            if(blockType == 0) return;
            var blockWorldPosition = Vector3Int.FloorToInt(blockPosition);
            var chunkPosition = GetChunkContainBlock(Vector3Int.FloorToInt(blockWorldPosition));

            if (Chunks.TryGetValue(chunkPosition, out var chunk))
            {
                var chunkOrigin = new Vector3Int(chunkPosition.x, chunkPosition.y, chunkPosition.z) * ChunkWidth;
                var blockChunkPosition = blockWorldPosition - chunkOrigin;
                chunk.Renderer.SpawnBlockWithoutUpdate(blockChunkPosition, blockType);
                _updateChunksPoll.Add(chunkPosition);
                if (ResourceLoader.Instance.GetBlock(blockType).HaveInventory)
                {
                    _inventories.Add(blockWorldPosition, Enumerable.Repeat(new ItemInSlot(), 36).ToList());
                }
            }
        }

        public void RerenderChunks()
        {
            foreach (var chunkPosition in _updateChunksPoll)
            {
                Chunks.TryGetValue(chunkPosition, out var chunk);
                if (chunk != null) chunk.Renderer.RegenerateMeshAndNearChunks();
            }
            _updateChunksPoll.Clear();
        }
        
        public void UpdateChest(Vector3Int blockPosition, List<ItemInSlot> slots)
        {
            _inventories[blockPosition] = slots;
        }


        public int DestroyBlock(Vector3 blockPosition)
        {
            var blockWorldPosition = Vector3Int.FloorToInt(blockPosition);
            var chunkPosition = GetChunkContainBlock(Vector3Int.FloorToInt(blockWorldPosition));

            if (Chunks.TryGetValue(chunkPosition, out var chunk))
            {
                var chunkOrigin = new Vector3Int(chunkPosition.x, chunkPosition.y, chunkPosition.z) * ChunkWidth;
                var blockChunkPosition = blockWorldPosition - chunkOrigin;

                var destroyedBlock = chunk.Renderer.DestroyBlock(blockChunkPosition);

                DropItem(blockWorldPosition, ResourceLoader.Instance.GetItem(destroyedBlock), 1);

                return destroyedBlock;
            }

            return -1;
        }

        public IEnumerator SpawnMobs(Vector2Int center)
        {
            for (var x = center.x - settings.viewDistanceInChunks;
                 x <= center.x + settings.viewDistanceInChunks;
                 x++)
            {
                for (var z = center.y - settings.viewDistanceInChunks;
                     z <= center.y + settings.viewDistanceInChunks;
                     z++)
                {
                    var chunkSpawnChance = Random.Range(0, 100);
                    var chunk = Chunks[new Vector3Int(x, 0, z)];
                    if (chunkSpawnChance <= 5)
                    {
                        for (int chunkX = 0; chunkX < ChunkWidth; chunkX++)
                        {
                            for (int chunkZ = 0; chunkZ < ChunkWidth; chunkZ++)
                            {
                                var spawnChance = Random.Range(0, 256);
                                if (spawnChance == 1 && chunk.SpawnedAnimals == false)
                                {
                                    var y = chunk.SurfaceHeight[chunkX, chunkZ] + 3;
                                    SpawnMob(new Vector3Int(chunkX + x * ChunkWidth, y, chunkZ + z * ChunkWidth));
                                }
                            }
                        }
                    }

                    chunk.SpawnedAnimals = true;
                }
            }

            yield return null;
        }

        public void SpawnMob(Vector3Int position)
        {
            Instantiate(mobPrefab, position + new Vector3(0.5f, 0, 0.5f), Quaternion.Euler(-90, 0, 0));
        }

        public void DropItem(Vector3 position, Item item, int amount)
        {
            var droppedItem = Instantiate(DroppedItemPrefab, position + new Vector3(0.5f, 0.5f, 0.5f),
                Quaternion.identity);
            droppedItem.Item = new ItemInSlot(item, amount);
            droppedItem.Init();
        }

        public List<ItemInSlot> GetInventory(Vector3 blockPosition)
        {
            var blockWorldPosition = Vector3Int.FloorToInt(blockPosition);
            return _inventories.GetValueOrDefault(blockWorldPosition);
        }


        public int[,,] CopyStructure(Vector3Int start, Vector3Int end)
        {
            int[,,] structure = new int[end.x - start.x + 1, end.y - start.y + 1, end.z - start.z + 1];
            for (int x = Mathf.Min(start.x, end.x); x <=  Mathf.Max(start.x, end.x); x++)
            {
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    for (int z = Mathf.Min(start.z, end.z); z <= Mathf.Max(start.z, end.z); z++)
                    {
                        structure[x - start.x, y - start.y, z - start.z] = GetBlockTypeAtPosition(new Vector3Int(x,y,z));
                    }
                }
            }
            return structure;
        }

        public string SaveStructure(int[,,] block, string structureName)
        {
            var saver = new StructureSaver();
            return saver.SaveStructure(block, structureName);
        }
        public void SpawnStructure(string structureName, Vector3Int structurePosition)
        {
            StructureSaver loader = new StructureSaver();
            int[,,] blocks = loader.LoadStructure(structureName);
            
            for (int x = 0; x < blocks.GetLength(0); x++)
            {
                for (int y = 0; y < blocks.GetLength(1); y++)
                {
                    for (int z = 0; z < blocks.GetLength(2); z++)
                    {
                        SpawnBlockNoUpdate(new Vector3Int(x, y, z) + structurePosition, blocks[x, y, z]);
                    }
                }
            }
            RerenderChunks();
        }


        public void UpdateChestItems(List<ItemInSlot> items, Vector3Int position)
        {
            _inventories[position] = items;
        }
    }

    [System.Serializable]
    public class UserSettings
    {
        [Header("Performance")] [Range(1, 16)] public int viewDistanceInChunks = 8;
        [Range(1, 32)] public int loadDistance = 16;
    }
}