using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Utils
{
    public class StructureSaver
    {
        public int[,,] LoadStructure(string structureName)
        {
            string folderPath = Path.Combine(Application.dataPath, "Resources/Structures");
            string filePath = Path.Combine(folderPath, structureName + ".json");

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Файл структуры {structureName} не найден по пути: {filePath}");
                return null;
            }

            string json = File.ReadAllText(filePath);
            Serializable3DArray deserializedData = JsonUtility.FromJson<Serializable3DArray>(json);

            if (deserializedData == null || deserializedData.Data == null)
            {
                Debug.LogError("Ошибка десериализации данных!");
                return null;
            }

            return ConvertSerializableToArray(deserializedData);
        }

        public string SaveStructure(int[,,] blocks, string structureName)
        {
            var serializableData = ConvertArrayToSerializable(blocks);

            string json =
                JsonUtility.ToJson(
                    new Serializable3DArray(serializableData, structureName,
                        new Vector3Int(blocks.GetLength(0), blocks.GetLength(1), blocks.GetLength(2))), true);

            Debug.Log($"Сериализованные данные: {json}");

            string folderPath = Path.Combine(Application.dataPath, "Resources/Structures");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, structureName + ".json");
            File.WriteAllText(filePath, json);

            Debug.Log($"Файл сохранён в папку Resources: {filePath}");
            return filePath;
        }

        private List<int> ConvertArrayToSerializable(int[,,] array)
        {
            int sizeX = array.GetLength(0);
            int sizeY = array.GetLength(1);
            int sizeZ = array.GetLength(2);

            var result = new List<int>();

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        result.Add(array[x, y, z]);
                    }
                }
            }

            return result;
        }

        private int[,,] ConvertSerializableToArray(Serializable3DArray data)
        {
            int[,,] result = new int[data.Length, data.Height, data.Width];

            int index = 0;
            for (int x = 0; x < data.Length; x++)
            {
                for (int y = 0; y < data.Height; y++)
                {
                    for (int z = 0; z < data.Width; z++)
                    {
                        result[x, y, z] = data.Data[index++];
                    }
                }
            }

            return result;
        }

        [System.Serializable]
        private class Serializable3DArray
        {
            public string Name;
            public int Length;
            public int Width;
            public int Height;
            public List<int> Data;

            public Serializable3DArray(List<int> data, string name, Vector3Int size)
            {
                Name = name;
                Data = data;
                Length = size.x;
                Width = size.z;
                Height = size.y;
            }
        }
    }
}
