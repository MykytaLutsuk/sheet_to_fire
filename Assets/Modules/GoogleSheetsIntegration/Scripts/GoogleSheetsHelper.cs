using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace GoogleSheetsIntegration
{
    public class GoogleSheetsHelper
    {
        private readonly SheetsService _sheetsService;

        public GoogleSheetsHelper(string credentialsPath)
        {
            GoogleCredential credential;
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Unity Google Sheets Integration"
            });
        }

        public List<string> GetSheetNames(string spreadsheetId)
        {
            if (_sheetsService == null)
            {
                Debug.LogError("Sheets service is not initialized. Ensure credentials are correctly set up.");
                return new List<string>();
            }

            try
            {
                var request = _sheetsService.Spreadsheets.Get(spreadsheetId);
                Spreadsheet spreadsheet = request.Execute();
                List<string> sheetNames = new List<string>();

                foreach (var sheet in spreadsheet.Sheets)
                {
                    sheetNames.Add(sheet.Properties.Title);
                }

                return sheetNames;
            }
            catch (Google.GoogleApiException e)
            {
                Debug.LogError(
                    $"Google API Error: {e.Message}. Check if the spreadsheet ID is correct and accessible.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex.Message}");
            }

            return new List<string>();
        }

        public List<IList<object>> GetTableContent(string spreadsheetId, string sheetName)
        {
            try
            {
                string range = $"{sheetName}";
                var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
                ValueRange response = request.Execute();

                return (List<IList<object>>)(response.Values ?? new List<IList<object>>());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error fetching table content: {ex.Message}");
                return new List<IList<object>>();
            }
        }

        public void SaveTableToJson(List<IList<object>> tableData, string outputPath)
        {
            try
            {
                var rows = new List<Dictionary<string, string>>();

                if (tableData.Count > 0)
                {
                    var headers = tableData[0];

                    for (int i = 1; i < tableData.Count; i++)
                    {
                        var row = tableData[i];
                        var rowDict = new Dictionary<string, string>();

                        for (int j = 0; j < headers.Count; j++)
                        {
                            string header = headers[j]?.ToString() ?? $"Column{j + 1}";
                            string value = j < row.Count ? row[j]?.ToString() : null;
                            
                            if (!string.IsNullOrEmpty(value))
                            {
                                rowDict[header] = value;
                            }
                        }

                        if (rowDict.Count > 0)
                        {
                            rows.Add(rowDict);
                        }
                    }
                }

                string json = JsonConvert.SerializeObject(rows, Formatting.Indented);
                File.WriteAllText(outputPath, json);
                Debug.Log($"Table data saved to {outputPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error saving table to JSON: {ex.Message}");
            }
        }
    }
}
