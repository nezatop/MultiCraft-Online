using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Worlds
{
    [System.Serializable]
    public class NoiseOctaveSetting
    {
        [Header("Noise Octave Settings")]
        public FastNoiseLite.NoiseType NoiseType;
        public float Frequency;
        public float Amplitude;
        
        [Header("Fractal")]
        public FastNoiseLite.FractalType FractalType;
        public int FractalOctaves;
        public float FractalGain;

        [Header("Cellular")] 
        public FastNoiseLite.CellularDistanceFunction CellularDistanceFunction;
        public FastNoiseLite.CellularReturnType CellularReturnType;
        public float CellularJitter;

        [Header("Domain Warp")]
        public FastNoiseLite.DomainWarpType DomainWarpType;
        public float DomainWarpAmplitude;
    }
}