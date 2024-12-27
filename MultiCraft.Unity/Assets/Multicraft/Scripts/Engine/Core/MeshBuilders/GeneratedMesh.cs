using MultiCraft.Scripts.Engine.Core.Chunks;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.MeshBuilders
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public class GeneratedMesh
    {
        public GeneratedMeshVertex[] Vertices;
        public Bounds Bounds;
        public Chunk Chunk;
    }
    
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public class DropItemGeneratedMesh
    {
        public GeneratedMeshVertex[] Vertices;
        public Bounds Bounds;
    }
    
}