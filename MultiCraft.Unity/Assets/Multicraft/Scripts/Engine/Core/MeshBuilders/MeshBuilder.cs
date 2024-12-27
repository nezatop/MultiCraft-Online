using System.Collections.Generic;
using MultiCraft.Scripts.Engine.Core.Blocks;
using MultiCraft.Scripts.Engine.Core.Chunks;
using MultiCraft.Scripts.Engine.Core.Worlds;
using MultiCraft.Scripts.Engine.Utils;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.MeshBuilders
{
    public static class MeshBuilder
    {
        public static GeneratedMesh GenerateMesh(Chunk chunk)
        {
            List<GeneratedMeshVertex> vertices = new List<GeneratedMeshVertex>();

            int maxY = 0;
            for (int y = 0; y < World.ChunkHeight; y++)
            {
                for (int x = 0; x < World.ChunkWidth; x++)
                {
                    for (int z = 0; z < World.ChunkWidth; z++)
                    {
                        if (GenerateBlock(x, y, z, vertices, chunk))
                        {
                            if (maxY < y) maxY = y;
                        }
                    }
                }
            }

            var mesh = new GeneratedMesh
            {
                Vertices = vertices.ToArray()
            };

            var boundsSize = new Vector3(World.ChunkWidth, maxY + 1, World.ChunkWidth);
            mesh.Bounds = new Bounds(boundsSize / 2, boundsSize);
            mesh.Chunk = chunk;

            return mesh;
        }

        private static bool GenerateBlock(int x, int y, int z, List<GeneratedMeshVertex> vertices, Chunk chunk)
        {
            Vector3Int blockPosition = new Vector3Int(x, y, z);

            var blockType = GetBlockPosition(blockPosition, chunk);
            if (blockType == 0) return false;

            var block = ResourceLoader.Instance.GetBlock(blockType);

            var otherBlockType = GetBlockPosition(blockPosition + Vector3Int.right, chunk);
            if (otherBlockType == 0)
                GenerateRightSide(blockPosition, vertices, block);
            else if (otherBlockType != -1)
                if (ResourceLoader.Instance.GetBlock(otherBlockType).IsTransparent)
                    GenerateRightSide(blockPosition, vertices, block);

            otherBlockType = GetBlockPosition(blockPosition + Vector3Int.left, chunk);
            if (otherBlockType == 0)
                GenerateLeftSide(blockPosition, vertices, block);
            else if (otherBlockType != -1)
                if (ResourceLoader.Instance.GetBlock(otherBlockType).IsTransparent)
                    GenerateLeftSide(blockPosition, vertices, block);

            otherBlockType = GetBlockPosition(blockPosition + Vector3Int.up, chunk);
            if (otherBlockType == 0)
            {
                GenerateTopSide(blockPosition, vertices, block);
            }
            else if (otherBlockType != -1)
                if (ResourceLoader.Instance.GetBlock(otherBlockType).IsTransparent)
                    GenerateTopSide(blockPosition, vertices, block);

            otherBlockType = GetBlockPosition(blockPosition + Vector3Int.down, chunk);
            if (blockPosition.y > 0 && otherBlockType == 0)
            {
                GenerateBottomSide(blockPosition, vertices, block);
            }
            else if (blockPosition.y > 0 && otherBlockType != -1)
                if (ResourceLoader.Instance.GetBlock(otherBlockType).IsTransparent)
                    GenerateBottomSide(blockPosition, vertices, block);

            otherBlockType = GetBlockPosition(blockPosition + Vector3Int.forward, chunk);
            if (otherBlockType == 0)
                GenerateFrontSide(blockPosition, vertices, block);
            else if (otherBlockType != -1)
                if (ResourceLoader.Instance.GetBlock(otherBlockType).IsTransparent)
                    GenerateFrontSide(blockPosition, vertices, block);

            otherBlockType = GetBlockPosition(blockPosition + Vector3Int.back, chunk);
            if (otherBlockType == 0)
                GenerateBackSide(blockPosition, vertices, block);
            else if (otherBlockType != -1)
                if (ResourceLoader.Instance.GetBlock(otherBlockType).IsTransparent)
                    GenerateBackSide(blockPosition, vertices, block);

            return true;
        }

        private static int GetBlockPosition(Vector3Int blockPosition, Chunk chunk)
        {
            if (blockPosition.x >= 0 && blockPosition.x < World.ChunkWidth &&
                blockPosition.y >= 0 && blockPosition.y < World.ChunkHeight &&
                blockPosition.z >= 0 && blockPosition.z < World.ChunkWidth)
            {
                return chunk.Blocks[blockPosition.x, blockPosition.y, blockPosition.z];
            }
            else
            {
                if (blockPosition.x < 0)
                    if (chunk.LeftChunk != null)
                        return chunk.LeftChunk.Blocks[blockPosition.x + World.ChunkWidth,
                            blockPosition.y,
                            blockPosition.z];
                if (blockPosition.x >= World.ChunkWidth)
                    if (chunk.RightChunk != null)
                        return chunk.RightChunk.Blocks[blockPosition.x - World.ChunkWidth,
                            blockPosition.y,
                            blockPosition.z];
                if (blockPosition.y < 0)
                    if (chunk.UpChunk != null)
                        return chunk.DownChunk.Blocks[blockPosition.x,
                            blockPosition.y + World.ChunkHeight,
                            blockPosition.z];
                if (blockPosition.y >= World.ChunkHeight)
                    if (chunk.UpChunk != null)
                        return chunk.UpChunk.Blocks[blockPosition.x,
                            blockPosition.y - World.ChunkHeight,
                            blockPosition.z];
                if (blockPosition.z < 0)
                    if (chunk.BackChunk != null)
                        return chunk.BackChunk.Blocks[blockPosition.x, blockPosition.y,
                            blockPosition.z + World.ChunkWidth];
                if (blockPosition.z >= World.ChunkWidth)
                    if (chunk.FrontChunk != null)
                        return chunk.FrontChunk.Blocks[blockPosition.x, blockPosition.y,
                            blockPosition.z - World.ChunkWidth];
            }

            return 0;
        }

        private static void GenerateTopSide(Vector3Int blockPosition, List<GeneratedMeshVertex> vertices,
            Block blockType)
        {
            var vertex = new GeneratedMeshVertex
            {
                NormalX = 0,
                NormalY = sbyte.MaxValue,
                NormalZ = 0,
                NormalW = 1
            };

            GetUvs(blockType, Vector3Int.up, out var uv1, out var uv2, out var uv3, out var uv4);

            if (blockType is { IsFlora: true }) return;

            vertex.Position = new Vector3(0, 1, 0) + blockPosition;
            vertex.Uv = uv1;
            vertices.Add(vertex);
            vertex.Position = new Vector3(0, 1, 1) + blockPosition;
            vertex.Uv = uv2;
            vertices.Add(vertex);
            vertex.Position = new Vector3(1, 1, 0) + blockPosition;
            vertex.Uv = uv3;
            vertices.Add(vertex);
            vertex.Position = new Vector3(1, 1, 1) + blockPosition;
            vertex.Uv = uv4;
            vertices.Add(vertex);
        }

        private static void GenerateBottomSide(Vector3Int blockPosition, List<GeneratedMeshVertex> vertices,
            Block blockType)
        {
            var vertex = new GeneratedMeshVertex
            {
                NormalX = 0,
                NormalY = sbyte.MinValue,
                NormalZ = 0,
                NormalW = 1,
            };
            GetUvs(blockType, Vector3Int.down, out var uv1, out var uv2, uv3: out var uv3, out var uv4);

            if (blockType is { IsFlora: true }) return;
            vertex.Position = new Vector3(0, 0, 0) + blockPosition;
            vertex.Uv = uv1;
            vertices.Add(vertex);
            vertex.Position = new Vector3(1, 0, 0) + blockPosition;
            vertex.Uv = uv2;
            vertices.Add(vertex);
            vertex.Position = new Vector3(0, 0, 1) + blockPosition;
            vertex.Uv = uv3;
            vertices.Add(vertex);
            vertex.Position = new Vector3(1, 0, 1) + blockPosition;
            vertex.Uv = uv4;
            vertices.Add(vertex);
        }

        private static void GenerateFrontSide(Vector3Int blockPosition, List<GeneratedMeshVertex> vertices,
            Block blockType)
        {
            var vertex = new GeneratedMeshVertex
            {
                NormalX = 0,
                NormalY = 0,
                NormalZ = sbyte.MaxValue,
                NormalW = 1,
            };
            
            GetUvs(blockType, Vector3Int.forward, out var uv1, out var uv2, out var uv3, out var uv4);

            if (blockType is { IsFlora: true })
            {
                vertex.Position = new Vector3(1, 0, 0) + blockPosition;
                vertex.Uv = uv1;
                vertices.Add(vertex);
                vertex.Position = new Vector3(1, 1, 0) + blockPosition;
                vertex.Uv = uv2;
                vertices.Add(vertex);
                vertex.Position = new Vector3(0, 0, 1) + blockPosition;
                vertex.Uv = uv3;
                vertices.Add(vertex);
                vertex.Position = new Vector3(0, 1, 1) + blockPosition;
                vertex.Uv = uv4;
                vertices.Add(vertex);
                return;
            }
            
            vertex.Position = new Vector3(1, 0, 1) + blockPosition;
            vertex.Uv = uv1;
            vertices.Add(vertex);
            vertex.Position = new Vector3(1, 1, 1) + blockPosition;
            vertex.Uv = uv2;
            vertices.Add(vertex);
            vertex.Position = new Vector3(0, 0, 1) + blockPosition;
            vertex.Uv = uv3;
            vertices.Add(vertex);
            vertex.Position = new Vector3(0, 1, 1) + blockPosition;
            vertex.Uv = uv4;
            vertices.Add(vertex);
        }

        private static void GenerateBackSide(Vector3Int blockPosition, List<GeneratedMeshVertex> vertices,
            Block blockType)
        {
            var vertex = new GeneratedMeshVertex
            {
                NormalX = 0,
                NormalY = 0,
                NormalZ = sbyte.MinValue,
                NormalW = 1
            };

            GetUvs(blockType, Vector3Int.back, out var uv1, out var uv2, out var uv3, out var uv4);

            if (blockType is { IsFlora: true })
            {
                vertex.Position = new Vector3(0, 0, 1) + blockPosition;
                vertex.Uv = uv1;
                vertices.Add(vertex);
                vertex.Position = new Vector3(0, 1, 1) + blockPosition;
                vertex.Uv = uv2;
                vertices.Add(vertex);
                vertex.Position = new Vector3(1, 0, 0) + blockPosition;
                vertex.Uv = uv3;
                vertices.Add(vertex);
                vertex.Position = new Vector3(1, 1, 0) + blockPosition;
                vertex.Uv = uv4;
                vertices.Add(vertex);
                return;
            }
            
            vertex.Position = new Vector3(0, 0, 0) + blockPosition;
            vertex.Uv = uv1;
            vertices.Add(vertex);
            vertex.Position = new Vector3(0, 1, 0) + blockPosition;
            vertex.Uv = uv2;
            vertices.Add(vertex);
            vertex.Position = new Vector3(1, 0, 0) + blockPosition;
            vertex.Uv = uv3;
            vertices.Add(vertex);
            vertex.Position = new Vector3(1, 1, 0) + blockPosition;
            vertex.Uv = uv4;
            vertices.Add(vertex);
        }

        private static void GenerateRightSide(Vector3Int blockPosition, List<GeneratedMeshVertex> vertices,
            Block blockType)
        {
            var vertex = new GeneratedMeshVertex
            {
                NormalX = sbyte.MaxValue,
                NormalY = 0,
                NormalZ = 0,
                NormalW = 1
            };
            
            GetUvs(blockType, Vector3Int.right, out var uv1, out var uv2, out var uv3, out var uv4);
            
            if (blockType is { IsFlora: true })
            {
                vertex.Position = new Vector3(0, 0, 0) + blockPosition;
                vertex.Uv = uv1;
                vertices.Add(vertex);
                vertex.Position = new Vector3(0, 1, 0) + blockPosition;
                vertex.Uv = uv2;
                vertices.Add(vertex);
                vertex.Position = new Vector3(1, 0, 1) + blockPosition;
                vertex.Uv = uv3;
                vertices.Add(vertex);
                vertex.Position = new Vector3(1, 1, 1) + blockPosition;;
                vertex.Uv = uv4;
                vertices.Add(vertex);
                return;
            }
            vertex.Position = new Vector3(1, 0, 0) + blockPosition;
            vertex.Uv = uv1;
            vertices.Add(vertex);
            vertex.Position = new Vector3(1, 1, 0) + blockPosition;
            vertex.Uv = uv2;
            vertices.Add(vertex);
            vertex.Position = new Vector3(1, 0, 1) + blockPosition;
            vertex.Uv = uv3;
            vertices.Add(vertex);
            vertex.Position = new Vector3(1, 1, 1) + blockPosition;
            vertex.Uv = uv4;
            vertices.Add(vertex);
        }

        private static void GenerateLeftSide(Vector3Int blockPosition, List<GeneratedMeshVertex> vertices,
            Block blockType)
        {
            var vertex = new GeneratedMeshVertex
            {
                NormalX = sbyte.MinValue,
                NormalY = 0,
                NormalZ = 0,
                NormalW = 0,
            };

            GetUvs(blockType, Vector3Int.left, out var uv1, out var uv2, out var uv3, out var uv4);

            if (blockType is { IsFlora: true })
            {
                vertex.Position = new Vector3(1, 0, 1) + blockPosition;
                vertex.Uv = uv1;
                vertices.Add(vertex);
                vertex.Position = new Vector3(1, 1, 1) + blockPosition;
                vertex.Uv = uv2;
                vertices.Add(vertex);
                vertex.Position = new Vector3(0, 0, 0) + blockPosition;
                vertex.Uv = uv3;
                vertices.Add(vertex);
                vertex.Position = new Vector3(0, 1, 0) + blockPosition;
                vertex.Uv = uv4;
                vertices.Add(vertex);
                return;
            }
            
            vertex.Position = new Vector3(0, 0, 1) + blockPosition;
            vertex.Uv = uv1;
            vertices.Add(vertex);
            vertex.Position = new Vector3(0, 1, 1) + blockPosition;
            vertex.Uv = uv2;
            vertices.Add(vertex);
            vertex.Position = new Vector3(0, 0, 0) + blockPosition;
            vertex.Uv = uv3;
            vertices.Add(vertex);
            vertex.Position = new Vector3(0, 1, 0) + blockPosition;
            vertex.Uv = uv4;
            vertices.Add(vertex);
        }

        private static void GetUvs(Block blockType, Vector3Int normal, out Vector2 uv1, out Vector2 uv2,
            out Vector2 uv3, out Vector2 uv4)
        {
            if (blockType == null)
            {
                uv1 = new Vector2(0, 0);
                uv2 = new Vector2(1, 0);
                uv3 = new Vector2(0, 1);
                uv4 = new Vector2(1, 1);
                return;
            }

            float textureAtlasSize = ResourceLoader.Instance.TextureData.AtlasSize;
            var textureResolution = ResourceLoader.Instance.TextureData.TextureResolution;
            var pixelOffset = blockType.GetUvsPixelsOffset(normal);

            uv1 = new Vector2(
                (pixelOffset.x * textureResolution) / textureAtlasSize,
                (pixelOffset.y * textureResolution) / textureAtlasSize);
            uv2 = new Vector2(
                (pixelOffset.x * textureResolution) / textureAtlasSize,
                ((pixelOffset.y + 1) * textureResolution) / textureAtlasSize);
            uv3 = new Vector2(
                ((pixelOffset.x + 1) * textureResolution) / textureAtlasSize,
                (pixelOffset.y * textureResolution) / textureAtlasSize);
            uv4 = new Vector2(
                ((pixelOffset.x + 1) * textureResolution) / textureAtlasSize,
                ((pixelOffset.y + 1) * textureResolution) / textureAtlasSize);
        }
    }
}