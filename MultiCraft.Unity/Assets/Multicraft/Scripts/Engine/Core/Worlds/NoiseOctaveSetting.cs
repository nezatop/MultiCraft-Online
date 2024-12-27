using System.Text.Json.Serialization;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Worlds
{
    [System.Serializable]
    public class NoiseOctaveSetting
    {
        [Header("Noise Octave Settings")]
        [JsonConverter(typeof(JsonStringEnumConverter))] public FastNoiseLite.NoiseType NoiseType;
        public float Frequency;
        public float Amplitude;
        
        [Header("Fractal")]
        [JsonConverter(typeof(JsonStringEnumConverter))]public FastNoiseLite.FractalType FractalType;
        public int FractalOctaves;
        public float FractalGain;

        [Header("Cellular")] 
        [JsonConverter(typeof(JsonStringEnumConverter))]public FastNoiseLite.CellularDistanceFunction CellularDistanceFunction;
        [JsonConverter(typeof(JsonStringEnumConverter))]public FastNoiseLite.CellularReturnType CellularReturnType;
        public float CellularJitter;

        [Header("Domain Warp")]
        [JsonConverter(typeof(JsonStringEnumConverter))]public FastNoiseLite.DomainWarpType DomainWarpType;
        public float DomainWarpAmplitude;
    }
}