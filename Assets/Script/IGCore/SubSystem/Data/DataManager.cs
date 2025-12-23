using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IGCore.SubSystem.Data
{
    public class DataManager : MonoBehaviour
    {
        private string dataPath;
        private Dictionary<string, JObject> dataCache = new Dictionary<string, JObject>();

        private void Awake()
        {
            dataPath = Path.Combine(Application.persistentDataPath, "GameData");
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
        }

        public void SaveData<T>(string key, T data)
        {
            try
            {
                string filePath = Path.Combine(dataPath, $"{key}.json");
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
                
                // 캐시 업데이트
                if (dataCache.ContainsKey(key))
                {
                    dataCache[key] = JObject.Parse(json);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving data for key {key}: {e.Message}");
                throw;
            }
        }

        public T LoadData<T>(string key)
        {
            try
            {
                string filePath = Path.Combine(dataPath, $"{key}.json");
                if (!File.Exists(filePath))
                {
                    return default(T);
                }

                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading data for key {key}: {e.Message}");
                throw;
            }
        }

        public JObject GetData(string key)
        {
            if (dataCache.TryGetValue(key, out var cachedData))
            {
                return cachedData;
            }

            string filePath = Path.Combine(dataPath, $"{key}.json");
            if (!File.Exists(filePath))
            {
                return null;
            }

            string json = File.ReadAllText(filePath);
            var data = JObject.Parse(json);
            dataCache[key] = data;
            return data;
        }

        public void UpdateData(string key, Action<JObject> updateAction)
        {
            try
            {
                var data = GetData(key) ?? new JObject();
                updateAction(data);
                SaveData(key, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating data for key {key}: {e.Message}");
                throw;
            }
        }

        public void ClearData(string key)
        {
            try
            {
                string filePath = Path.Combine(dataPath, $"{key}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                dataCache.Remove(key);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error clearing data for key {key}: {e.Message}");
                throw;
            }
        }

        public void ClearAllData()
        {
            try
            {
                foreach (var file in Directory.GetFiles(dataPath, "*.json"))
                {
                    File.Delete(file);
                }
                dataCache.Clear();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error clearing all data: {e.Message}");
                throw;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("PlasticGames/Data/Open Data Folder")]
        private static void OpenDataFolder()
        {
            string path = Path.Combine(Application.persistentDataPath, "GameData");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            UnityEditor.EditorUtility.RevealInFinder(path);
        }

        [UnityEditor.MenuItem("PlasticGames/Data/Backup Data")]
        private static void BackupData()
        {
            string sourcePath = Path.Combine(Application.persistentDataPath, "GameData");
            string backupPath = Path.Combine(Application.persistentDataPath, 
                $"GameData_Backup_{DateTime.Now:yyyyMMdd_HHmmss}");

            if (Directory.Exists(sourcePath))
            {
                Directory.CreateDirectory(backupPath);
                foreach (string file in Directory.GetFiles(sourcePath))
                {
                    string fileName = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(backupPath, fileName));
                }
                UnityEditor.EditorUtility.RevealInFinder(backupPath);
            }
        }
#endif
    }
} 