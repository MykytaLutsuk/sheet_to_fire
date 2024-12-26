using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GoogleSheetsIntegration
{
    public class SpreadsheetModel
    {
        public List<string> SpreadsheetIDs { get; private set; } = new List<string>();

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "spreadsheets.json");

        public void AddSpreadsheetID(string id)
        {
            if (!SpreadsheetIDs.Contains(id))
            {
                SpreadsheetIDs.Add(id);
            }
        }

        public void RemoveSpreadsheetID(string id)
        {
            if (SpreadsheetIDs.Contains(id))
            {
                SpreadsheetIDs.Remove(id);
            }
        }

        public void SaveToFile()
        {
            var data = new SpreadsheetData
            {
                spreadsheetIDs = SpreadsheetIDs
            };

            File.WriteAllText(SaveFilePath, JsonUtility.ToJson(data, true));
            Debug.Log($"Data saved to {SaveFilePath}");
        }

        public void LoadFromFile()
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning($"Save file not found at path: {SaveFilePath}");
                return;
            }

            string json = File.ReadAllText(SaveFilePath);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("Save file is empty.");
                return;
            }

            var data = JsonUtility.FromJson<SpreadsheetData>(json);
            if (data != null && data.spreadsheetIDs != null)
            {
                SpreadsheetIDs = data.spreadsheetIDs;
            }
            else
            {
                Debug.LogWarning("Failed to parse save file.");
            }
        }

        [System.Serializable]
        private class SpreadsheetData
        {
            public List<string> spreadsheetIDs;
        }
    }
}