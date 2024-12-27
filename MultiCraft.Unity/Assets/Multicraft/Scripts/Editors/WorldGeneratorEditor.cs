#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MultiCraft.Scripts.Engine.Core.Biomes;
using MultiCraft.Scripts.Engine.Core.Worlds;

namespace MultiCraft.Scripts.Editors
{
    [CustomEditor(typeof(WorldGenerator))]
    public class WorldGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            WorldGenerator worldGenerator = (WorldGenerator)target;

            if (GUILayout.Button("Save Settings to JSON"))
            {
                SaveSettingsToJson(worldGenerator);
            }
        }

        private void SaveSettingsToJson(WorldGenerator worldGenerator)
        {
            // Создаём объект, который будет сериализован в JSON
            WorldGeneratorSettings settings = new WorldGeneratorSettings
            {
                BaseHeight = worldGenerator.BaseHeight,
                WaterLevel = worldGenerator.WaterLevel,
                RiverChance = worldGenerator.RiverChance,
                TreeFrequency = worldGenerator.TreeFrequency,
                Biomes = worldGenerator.Biomes,
                BiomeNoiseOctaves = worldGenerator.BiomeNoiseOctaves,
                FloraNoiseOctaves = worldGenerator.FloraNoiseOctaves,
                TreeNoiseOctaves = worldGenerator.TreeNoiseOctaves,
                WaterNoiseOctaves = worldGenerator.WaterNoiseOctaves,
                SurfaceNoiseOctaves = worldGenerator.SurfaceNoiseOctaves
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // Красивый формат JSON
                Converters = { new JsonStringEnumConverter() }, // Обработка enum как строки
                IncludeFields = true 
            };
            
            string json = JsonSerializer.Serialize(settings, options);
            string path = "Assets/WorldGeneratorSettings.json";

            // Записываем в файл
            File.WriteAllText(path, json);

            // Перезагружаем ассет в Unity, чтобы изменения были применены сразу
            AssetDatabase.Refresh();

            Debug.Log("Settings saved to " + path);
        }
    }

    [System.Serializable]
    public class WorldGeneratorSettings
    {
        public int BaseHeight { get; set; }
        public int WaterLevel{ get; set; }
        public float RiverChance{ get; set; }
        public float TreeFrequency{ get; set; }
        public Biome[] Biomes{ get; set; }
        public NoiseOctaveSetting BiomeNoiseOctaves{ get; set; }
        public NoiseOctaveSetting FloraNoiseOctaves{ get; set; }
        public NoiseOctaveSetting[] TreeNoiseOctaves{ get; set; }
        public NoiseOctaveSetting WaterNoiseOctaves{ get; set; }
        public NoiseOctaveSetting[] SurfaceNoiseOctaves{ get; set; }
    }
}

#endif