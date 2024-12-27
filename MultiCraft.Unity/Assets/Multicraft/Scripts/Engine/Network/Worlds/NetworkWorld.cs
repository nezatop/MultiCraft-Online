using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MultiCraft.Scripts.Engine.Core.Blocks;
using MultiCraft.Scripts.Engine.Core.Chunks;
using MultiCraft.Scripts.Engine.Core.Entities;
using MultiCraft.Scripts.Engine.Core.MeshBuilders;
using MultiCraft.Scripts.Engine.Core.Player;
using MultiCraft.Scripts.Engine.Utils;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Network.Worlds
{
    public class NetworkWorld : MonoBehaviour
    {
        public static NetworkWorld instance { get;private set; }

        [Header("User settings")] 
        public ServerSettings settings;
        
        [Header("Chunk Settings")] 
        public const int ChunkWidth = 16;
        public const int ChunkHeight = 256;
        
        [Header("Chunks Dictionary")] 
        public Dictionary<Vector3Int, Chunk> Chunks;
        public Dictionary<Vector3Int, Chunk> WaterChunks;
        public Dictionary<Vector3Int, Chunk> FloraChunks;
        private readonly ConcurrentQueue<GeneratedMesh> _meshingResults = new ConcurrentQueue<GeneratedMesh>();
        private readonly ConcurrentQueue<GeneratedMesh> _meshingWaterResults = new ConcurrentQueue<GeneratedMesh>();
        private readonly ConcurrentQueue<GeneratedMesh> _meshingFloraResults = new ConcurrentQueue<GeneratedMesh>();
        
        [Header("Prefabs")]
        public ChunkRenderer chunkPrefab;
        public ChunkRenderer waterChunkPrefab;
        public ChunkRenderer floraChunkPrefab;
        
        [Header("Resource Loader")]
        public ResourceLoader resourceLoader;
        
        private HashSet<Vector3Int> _updateChunksPoll = new HashSet<Vector3Int>();
       
        private void Awake()
        {
            instance = this;
            StartCoroutine(InitializeWorld());
        }

        private IEnumerator InitializeWorld()
        {
            
            Chunks = new Dictionary<Vector3Int, Chunk>();
            WaterChunks = new Dictionary<Vector3Int, Chunk>();
            FloraChunks = new Dictionary<Vector3Int, Chunk>();
            
            ChunkRenderer.InitializeTriangles();
            DropItemRenderer.InitializeTriangles();
            HandRenderer.InitializeTriangles();
            
            yield return StartCoroutine(resourceLoader.Initialize());
        }

        public bool SpawnChunk(Vector3Int position, int[,,] blocks)
        {
            Chunk chunk = new Chunk()
            {
                Position = position,
                Blocks = blocks,
                State = ChunkState.Generated
            };
            
            Chunks.Add(position, chunk);
            
            return true;
        }

        public void RenderChunks()
        {
            foreach (Chunk chunk in Chunks.Values)
            {
                Chunks.TryGetValue(chunk.Position + Vector3Int.left, out chunk.LeftChunk);
                Chunks.TryGetValue(chunk.Position + Vector3Int.right, out chunk.RightChunk);
                Chunks.TryGetValue(chunk.Position + Vector3Int.up, out chunk.UpChunk);
                Chunks.TryGetValue(chunk.Position + Vector3Int.down, out chunk.DownChunk);
                Chunks.TryGetValue(chunk.Position + Vector3Int.forward, out chunk.FrontChunk);
                Chunks.TryGetValue(chunk.Position + Vector3Int.back, out chunk.BackChunk);

                chunk.State = ChunkState.MeshBuilding;
                var mesh = MeshBuilder.GenerateMesh(chunk);
                chunk.State = ChunkState.Loaded;

                var chunkObject = Instantiate(chunkPrefab, 
                    new Vector3(chunk.Position.x*ChunkWidth, chunk.Position.y, chunk.Position.z*ChunkWidth), 
                    Quaternion.identity,
                    transform);
                chunkObject.Chunk = mesh.Chunk;

                chunkObject.SetMesh(mesh);

                mesh.Chunk.Renderer = chunkObject;
                mesh.Chunk.State = ChunkState.Active;
            }
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


        public int DestroyBlock(Vector3 blockPosition)
        {
            var blockWorldPosition = Vector3Int.FloorToInt(blockPosition);
            var chunkPosition = GetChunkContainBlock(Vector3Int.FloorToInt(blockWorldPosition));

            NetworkManager.Instance.SendBlockDestroyed(blockWorldPosition);
            
            if (Chunks.TryGetValue(chunkPosition, out var chunk))
            {
                var chunkOrigin = new Vector3Int(chunkPosition.x, chunkPosition.y, chunkPosition.z) * ChunkWidth;
                var blockChunkPosition = blockWorldPosition - chunkOrigin;

                Debug.Log(chunkPosition);
                Debug.Log(blockChunkPosition);
                //var destroyedBlock = chunk.Renderer.DestroyBlock(blockChunkPosition);

                //DropItem(blockWorldPosition, ResourceLoader.Instance.GetItem(destroyedBlock), 1);

                //return destroyedBlock;
            }

            return -1;
        }

        public void SpawnBlock(Vector3 blockPosition, int blockType)
        {
            if(blockType == 0) return;
            var blockWorldPosition = Vector3Int.FloorToInt(blockPosition);
            var chunkPosition = GetChunkContainBlock(Vector3Int.FloorToInt(blockWorldPosition));

            NetworkManager.Instance.SendBlockPlaced(blockWorldPosition, blockType);
            
            if (Chunks.TryGetValue(chunkPosition, out var chunk))
            {
                var chunkOrigin = new Vector3Int(chunkPosition.x, chunkPosition.y, chunkPosition.z) * ChunkWidth;
                var blockChunkPosition = blockWorldPosition - chunkOrigin;
                //chunk.Renderer.SpawnBlock(blockChunkPosition, blockType);
                /*if (ResourceLoader.Instance.GetBlock(blockType).HaveInventory)
                {
                    _inventories.Add(blockWorldPosition, Enumerable.Repeat(new ItemInSlot(), 36).ToList());
                }*/
            }
        }

        public void UpdateBlock(Vector3Int position, int newBlockType)
        {
            var blockWorldPosition = Vector3Int.FloorToInt(position);
            var chunkPosition = GetChunkContainBlock(Vector3Int.FloorToInt(blockWorldPosition));
            if (Chunks.TryGetValue(chunkPosition, out var chunk))
            {
                var chunkOrigin = new Vector3Int(chunkPosition.x, chunkPosition.y, chunkPosition.z) * ChunkWidth;
                var blockChunkPosition = blockWorldPosition - chunkOrigin;
                
                chunk.Renderer.SpawnBlock(blockChunkPosition, newBlockType);
            }
        }
    }
    
    [System.Serializable]
    public class ServerSettings
    {
        [Header("Performance")] [Range(1, 16)] public int viewDistanceInChunks = 8;
        [Range(1, 32)] public int loadDistance = 16;
    }
}