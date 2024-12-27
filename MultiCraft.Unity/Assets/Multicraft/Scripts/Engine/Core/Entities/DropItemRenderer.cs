using System;
using MultiCraft.Scripts.Engine.Core.MeshBuilders;
using UnityEngine;
using UnityEngine.Rendering;

namespace MultiCraft.Scripts.Engine.Core.Entities
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
    public class DropItemRenderer : MonoBehaviour
    {
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        
        public Mesh mesh;
        
        private static int[] _triangles;
        
        public static void InitializeTriangles()
        {
            _triangles = new int[36];

            var vertexNumber = 4;
            for (var i = 0; i < _triangles.Length; i += 6)
            {
                _triangles[i] = vertexNumber - 4;
                _triangles[i + 1] = vertexNumber - 3;
                _triangles[i + 2] = vertexNumber - 2;
                _triangles[i + 3] = vertexNumber - 3;
                _triangles[i + 4] = vertexNumber - 1;
                _triangles[i + 5] = vertexNumber - 2;

                vertexNumber += 4;
            }
        }

        private void Awake()
        {
            mesh = new Mesh();

            meshFilter.sharedMesh = mesh;
        }

        public void SetMesh(DropItemGeneratedMesh dropItemGeneratedMesh)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.SNorm8, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            };

            this.mesh.SetVertexBufferParams(dropItemGeneratedMesh.Vertices.Length, layout);
            this.mesh.SetVertexBufferData(dropItemGeneratedMesh.Vertices, 0, 0, dropItemGeneratedMesh.Vertices.Length, 0,
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);

            var trianglesCount = 36;
            this.mesh.SetIndexBufferParams(trianglesCount, IndexFormat.UInt32);
            this.mesh.SetIndexBufferData(_triangles, 0, 0, trianglesCount,
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontRecalculateBounds);

            this.mesh.subMeshCount = 1;
            this.mesh.SetSubMesh(0, new SubMeshDescriptor(0, trianglesCount));

            this.mesh.bounds = dropItemGeneratedMesh.Bounds;
        }
    }
}