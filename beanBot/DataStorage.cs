using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace beanBot
{
    class DataStorage
    {
        private static Dictionary<string, string> pairs = new Dictionary<string, string>();

        public static void AddPairToStorage(string key, string value)
        {
            if (pairs.ContainsKey(key)) pairs.Remove(key);
            pairs.Add(key, value);
            SaveData();
        }

        public static string GetValue(string key, string value = "")
        {
            if (pairs.ContainsKey(key))
            {
                return pairs[key];
            }
            AddPairToStorage(key, value);
            return GetValue(key, value);
        }

        static DataStorage()
        {
            // Load data
            if (!ValidateStorageFile("DataStorage.json")) return;
            string json = File.ReadAllText("DataStorage.json");
            pairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public static void SaveData()
        {
            // Save data
            string json = JsonConvert.SerializeObject(pairs, Formatting.Indented);
            File.WriteAllText("DataStorage.json", json);
        }

        private static bool ValidateStorageFile(string file)
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, "");
                SaveData();
                return false;
            }
            return true;
        }
    }
}
