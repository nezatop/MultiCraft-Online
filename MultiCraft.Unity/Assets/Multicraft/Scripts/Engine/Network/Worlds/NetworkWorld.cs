using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MultiCraft.Scripts.Engine.Core.Blocks;
using MultiCraft.Scripts.Engine.Core.Chunks;
using MultiCraft.Scripts.Engine.Core.Entities;
using MultiCraft.Scripts.Engine.Core.Inventories;
using MultiCraft.Scripts.Engine.Core.MeshBuilders;
using MultiCraft.Scripts.Engine.Core.Player;
using MultiCraft.Scripts.Engine.Utils;
using Unity.VisualScripting;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Network.Worlds
{
    public class NetworkWorld : MonoBehaviour
    {
        public static NetworkWorld instance { get; private set; }

        [Header("User settings")] public ServerSettings settings;

        [Header("Chunk Settings")] private const int ChunkWidth = 16;
        private const int ChunkHeight = 256;

        [Header("Chunks Dictionary")] private Dictionary<Vector3Int, Chunk> _chunks;
        private Dictionary<Vector3Int, Chunk> _waterChunks;
        private Dictionary<Vector3Int, Chunk> _floraChunks;

        [Header("Prefabs")] public ChunkRenderer chunkPrefab;
        public ChunkRenderer waterChunkPrefab;
        public ChunkRenderer floraChunkPrefab;

        private readonly ConcurrentQueue<GeneratedMesh> _meshingResults = new ConcurrentQueue<GeneratedMesh>();
        private readonly ConcurrentQueue<GeneratedMesh> _meshingWaterResults = new ConcurrentQueue<GeneratedMesh>();
        private readonly ConcurrentQueue<GeneratedMesh> _meshingFloraResults = new ConcurrentQueue<GeneratedMesh>();


        [Header("Resource Loader")] public ResourceLoader resourceLoader;

        public GameObject player;
        public Vector3Int currentPosition;
        public bool canSpawnPlayer;

        public event Action RenderAllChunks;

        private void Awake()
        {
            instance = this;
            StartCoroutine(InitializeWorld());
        }

        private IEnumerator InitializeWorld()
        {
            canSpawnPlayer = false;
            
            _chunks = new Dictionary<Vector3Int, Chunk>();
            _waterChunks = new Dictionary<Vector3Int, Chunk>();
            _floraChunks = new Dictionary<Vector3Int, Chunk>();

            ChunkRenderer.InitializeTriangles();
            DropItemRenderer.InitializeTriangles();
            HandRenderer.InitializeTriangles();

            yield return StartCoroutine(resourceLoader.Initialize());
        }

        public void Update()
        {
            if (player)
                StartCoroutine(CheckPlayer());

            SpawnChunks();
            if (canSpawnPlayer)
                RenderAllChunks?.Invoke();
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

            chunksAmount = 0;
            while (chunksAmount < 4)
            {
                if (_meshingWaterResults.TryDequeue(out var mesh))
                {
                    var xPos = mesh.Chunk.Position.x * ChunkWidth;
                    var yPos = mesh.Chunk.Position.y * ChunkHeight;
                    var zPos = mesh.Chunk.Position.z * ChunkWidth;

                    var chunkObject = Instantiate(waterChunkPrefab, new Vector3(xPos, yPos, zPos), Quaternion.identity,
                        transform);
                    chunkObject.Chunk = mesh.Chunk;

                    chunkObject.SetMesh(mesh);

                    mesh.Chunk.Renderer = chunkObject;
                    mesh.Chunk.State = ChunkState.Active;
                }

                chunksAmount++;
            }

            chunksAmount = 0;
            while (chunksAmount < 4)
            {
                if (_meshingFloraResults.TryDequeue(out var mesh))
                {
                    var xPos = mesh.Chunk.Position.x * ChunkWidth;
                    var yPos = mesh.Chunk.Position.y * ChunkHeight;
                    var zPos = mesh.Chunk.Position.z * ChunkWidth;

                    var chunkObject = Instantiate(floraChunkPrefab, new Vector3(xPos, yPos, zPos), Quaternion.identity,
                        transform);
                    chunkObject.Chunk = mesh.Chunk;

                    chunkObject.SetMesh(mesh);

                    mesh.Chunk.Renderer = chunkObject;
                    mesh.Chunk.State = ChunkState.Active;
                }

                chunksAmount++;
            }
        }


        private IEnumerator CheckPlayer()
        {
            var playersPosition = Vector3Int.FloorToInt(player.transform.position);
            if (currentPosition != playersPosition)
            {
                currentPosition = playersPosition;
                var playerChunkPosition = GetChunkContainBlock(currentPosition);
                for (int x = playerChunkPosition.x - settings.loadDistance;
                     x <= playerChunkPosition.x + settings.loadDistance;
                     x++)
                {
                    for (int z = playerChunkPosition.z - settings.loadDistance;
                         z <= playerChunkPosition.z + settings.loadDistance;
                         z++)
                    {
                        if (_chunks.ContainsKey(new Vector3Int(x, 0, z))) continue;
                        NetworkManager.instance.RequestChunk(new Vector3Int(x, 0, z));
                        yield return null;
                    }
                }
            }
        }

        public bool SpawnChunk(Vector3Int position, int[,,] blocks)
        {
            if (_chunks.ContainsKey(position)) return false;
            Chunk chunk = new Chunk()
            {
                Position = position,
                Blocks = blocks,
                State = ChunkState.Generated
            };

            _chunks.Add(position, chunk);

            return true;
        }

        public bool SpawnWaterChunk(Vector3Int position, int[,,] blocks)
        {
            if (_waterChunks.ContainsKey(position)) return false;
            Chunk chunk = new Chunk()
            {
                Position = position,
                Blocks = blocks,
                State = ChunkState.Generated
            };

            _waterChunks.Add(position, chunk);

            return true;
        }

        public bool SpawnFloraChunk(Vector3Int position, int[,,] blocks)
        {
            if (_floraChunks.ContainsKey(position)) return false;
            Chunk chunk = new Chunk()
            {
                Position = position,
                Blocks = blocks,
                State = ChunkState.Generated
            };

            _floraChunks.Add(position, chunk);

            return true;
        }

        public IEnumerator RenderChunks(Vector3Int position)
        {
            var playerChunkPosition = GetChunkContainBlock(currentPosition);
            if (_chunks.TryGetValue(position, out var chunk))
            {
                if (playerChunkPosition.x - settings.viewDistanceInChunks <= chunk.Position.x &&
                    playerChunkPosition.x + settings.viewDistanceInChunks >= chunk.Position.x &&
                    playerChunkPosition.z - settings.viewDistanceInChunks <= chunk.Position.z &&
                    playerChunkPosition.z + settings.viewDistanceInChunks >= chunk.Position.z)
                {
                    _chunks.TryGetValue(chunk.Position + Vector3Int.left, out chunk.LeftChunk);
                    _chunks.TryGetValue(chunk.Position + Vector3Int.right, out chunk.RightChunk);
                    _chunks.TryGetValue(chunk.Position + Vector3Int.up, out chunk.UpChunk);
                    _chunks.TryGetValue(chunk.Position + Vector3Int.down, out chunk.DownChunk);
                    _chunks.TryGetValue(chunk.Position + Vector3Int.forward, out chunk.FrontChunk);
                    _chunks.TryGetValue(chunk.Position + Vector3Int.back, out chunk.BackChunk);

                    chunk.State = ChunkState.MeshBuilding;
                    var mesh = MeshBuilder.GenerateMesh(chunk);
                    chunk.State = ChunkState.Loaded;

                    _meshingResults.Enqueue(mesh);
                }
            }

            yield return null;
        }

        public IEnumerator RenderWaterChunks(Vector3Int position)
        {
            var playerChunkPosition = GetChunkContainBlock(currentPosition);
            if (_waterChunks.TryGetValue(position, out var chunk))
            {
                if (playerChunkPosition.x - settings.viewDistanceInChunks <= chunk.Position.x &&
                    playerChunkPosition.x + settings.viewDistanceInChunks >= chunk.Position.x &&
                    playerChunkPosition.z - settings.viewDistanceInChunks <= chunk.Position.z &&
                    playerChunkPosition.z + settings.viewDistanceInChunks >= chunk.Position.z)
                {
                    _waterChunks.TryGetValue(chunk.Position + Vector3Int.left, out chunk.LeftChunk);
                    _waterChunks.TryGetValue(chunk.Position + Vector3Int.right, out chunk.RightChunk);
                    _waterChunks.TryGetValue(chunk.Position + Vector3Int.up, out chunk.UpChunk);
                    _waterChunks.TryGetValue(chunk.Position + Vector3Int.down, out chunk.DownChunk);
                    _waterChunks.TryGetValue(chunk.Position + Vector3Int.forward, out chunk.FrontChunk);
                    _waterChunks.TryGetValue(chunk.Position + Vector3Int.back, out chunk.BackChunk);

                    chunk.State = ChunkState.MeshBuilding;
                    var mesh = MeshBuilder.GenerateMesh(chunk);
                    chunk.State = ChunkState.Loaded;

                    _meshingWaterResults.Enqueue(mesh);
                }
            }

            yield return null;
        }

        public IEnumerator RenderFloraChunks(Vector3Int position)
        {
            var playerChunkPosition = GetChunkContainBlock(currentPosition);
            if (_floraChunks.TryGetValue(position, out var chunk))
            {
                if (playerChunkPosition.x - settings.viewDistanceInChunks <= chunk.Position.x &&
                    playerChunkPosition.x + settings.viewDistanceInChunks >= chunk.Position.x &&
                    playerChunkPosition.z - settings.viewDistanceInChunks <= chunk.Position.z &&
                    playerChunkPosition.z + settings.viewDistanceInChunks >= chunk.Position.z)
                {
                    _floraChunks.TryGetValue(chunk.Position + Vector3Int.left, out chunk.LeftChunk);
                    _floraChunks.TryGetValue(chunk.Position + Vector3Int.right, out chunk.RightChunk);
                    _floraChunks.TryGetValue(chunk.Position + Vector3Int.up, out chunk.UpChunk);
                    _floraChunks.TryGetValue(chunk.Position + Vector3Int.down, out chunk.DownChunk);
                    _floraChunks.TryGetValue(chunk.Position + Vector3Int.forward, out chunk.FrontChunk);
                    _floraChunks.TryGetValue(chunk.Position + Vector3Int.back, out chunk.BackChunk);

                    chunk.State = ChunkState.MeshBuilding;
                    var mesh = MeshBuilder.GenerateMesh(chunk);
                    chunk.State = ChunkState.Loaded;

                    _meshingFloraResults.Enqueue(mesh);
                }
            }

            yield return null;
        }

        public Block GetBlockAtPosition(Vector3 blockPosition)
        {
            var blockWorldPosition = Vector3Int.FloorToInt(blockPosition);
            var chunkPosition = GetChunkContainBlock(Vector3Int.FloorToInt(blockPosition));

            int blockId = 0;
            if (_chunks.TryGetValue(chunkPosition, out var chunk))
            {
                var chunkOrigin = new Vector3Int(chunkPosition.x, chunkPosition.y, chunkPosition.z) * ChunkWidth;
                var blockChunkPosition = blockWorldPosition - chunkOrigin;
                blockId = chunk.Blocks[blockChunkPosition.x, blockChunkPosition.y, blockChunkPosition.z];
            }

            return ResourceLoader.Instance.GetBlock(blockId);
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


        public void DestroyBlock(Vector3 blockPosition)
        {
            var blockWorldPosition = Vector3Int.FloorToInt(blockPosition);

            NetworkManager.instance.SendBlockDestroyed(blockWorldPosition);
        }

        public void SpawnBlock(Vector3 blockPosition, int blockType)
        {
            if (blockType == 0) return;
            var blockWorldPosition = Vector3Int.FloorToInt(blockPosition);

            NetworkManager.instance.SendBlockPlaced(blockWorldPosition, blockType);
        }

        public void UpdateBlock(Vector3Int position, int newBlockType)
        {
            var blockWorldPosition = Vector3Int.FloorToInt(position);
            var chunkPosition = GetChunkContainBlock(Vector3Int.FloorToInt(blockWorldPosition));
            if (_chunks.TryGetValue(chunkPosition, out var chunk))
            {
                var chunkOrigin = new Vector3Int(chunkPosition.x, chunkPosition.y, chunkPosition.z) * ChunkWidth;
                var blockChunkPosition = blockWorldPosition - chunkOrigin;

                chunk.Renderer.SpawnBlock(blockChunkPosition, newBlockType);
            }
        }

        public void GetInventory(Vector3 blockPosition)
        {
            NetworkManager.instance.GetInventory(blockPosition);
        }

        public void UpdateChest(Vector3Int chestPosition, List<ItemInSlot> chestSlots)
        {
            NetworkManager.instance.SetInventory(chestPosition, chestSlots);
        }
    }

    [System.Serializable]
    public class ServerSettings
    {
        [Header("Performance")] [Range(1, 16)] public int viewDistanceInChunks = 8;
        [Range(1, 32)] public int loadDistance = 16;
    }
}