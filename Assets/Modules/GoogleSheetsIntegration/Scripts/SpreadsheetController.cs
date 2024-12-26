using System.Collections.Generic;
using System.IO;
using Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GoogleSheetsIntegration
{
    public class SpreadsheetController : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_InputField spreadsheetInputField;
        public Button addButton;
        public Button removeButton;
        public Button fetchAndParseContentButton;
        public TMP_Dropdown spreadsheetDropdown;
        public TMP_Dropdown tableDropdown;

        private SpreadsheetModel _model;
        private GoogleSheetsHelper _sheetsHelper;
        
        private void Start()
        {
            string credentialsPath = CredentialsManager.SheetsCredentialsPath;;

            if (string.IsNullOrEmpty(credentialsPath))
            {
                Debug.LogError("No credentials file provided.");
                return;
            }
            
            _model = new SpreadsheetModel();
            _sheetsHelper = new GoogleSheetsHelper(credentialsPath);

            _model.LoadFromFile();

            addButton.onClick.AddListener(AddSpreadsheet);
            removeButton.onClick.AddListener(RemoveSpreadsheet);
            fetchAndParseContentButton.onClick.AddListener(FetchAndParseContent);
            spreadsheetDropdown.onValueChanged.AddListener(LoadTableNames);

            UpdateSpreadsheetDropdown();
        }

        private void AddSpreadsheet()
        {
            string id = spreadsheetInputField.text.Trim();
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("Spreadsheet ID is empty. Please enter a valid ID.");
                return;
            }

            try
            {
                List<string> tableNames = _sheetsHelper.GetSheetNames(id);

                if (tableNames.Count == 0)
                {
                    Debug.LogWarning($"No sheets found for Spreadsheet ID: {id}. ID will not be added.");
                    return;
                }

                _model.AddSpreadsheetID(id);
                
                _model.SaveToFile();

                UpdateSpreadsheetDropdown();
                Debug.Log($"Spreadsheet ID {id} added successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to validate Spreadsheet ID: {id}. Error: {ex.Message}");
            }
        }

        private void RemoveSpreadsheet()
        {
            string id = spreadsheetInputField.text.Trim();
            if (!string.IsNullOrEmpty(id))
            {
                _model.RemoveSpreadsheetID(id);

                _model.SaveToFile();

                UpdateSpreadsheetDropdown();
            }
        }

        private void FetchAndParseContent()
        {
            string spreadsheetId = spreadsheetDropdown.options[spreadsheetDropdown.value].text;
            string sheetName = tableDropdown.options[tableDropdown.value].text;

            if (string.IsNullOrEmpty(spreadsheetId) || string.IsNullOrEmpty(sheetName))
            {
                Debug.LogWarning("Spreadsheet ID or Sheet Name is empty.");
                return;
            }

            var tableContent = _sheetsHelper.GetTableContent(spreadsheetId, sheetName);
            if (tableContent.Count == 0)
            {
                Debug.LogWarning("No data found in the selected table.");
                return;
            }

            string outputPath = Path.Combine(Application.persistentDataPath, "output.json");
            _sheetsHelper.SaveTableToJson(tableContent, outputPath);
            FileOpener.OpenFile(outputPath);

            Debug.Log($"Table content saved to {outputPath}");
        }

        private void UpdateSpreadsheetDropdown()
        {
            spreadsheetDropdown.ClearOptions();
            spreadsheetDropdown.AddOptions(_model.SpreadsheetIDs);

            if (_model.SpreadsheetIDs.Count > 0)
            {
                LoadTableNames(0);
            }
            else
            {
                tableDropdown.ClearOptions();
            }
        }

        private void LoadTableNames(int spreadsheetIndex)
        {
            if (spreadsheetIndex < 0 || spreadsheetIndex >= _model.SpreadsheetIDs.Count)
            {
                Debug.LogWarning("Invalid spreadsheet index.");
                tableDropdown.ClearOptions();
                return;
            }

            string selectedID = _model.SpreadsheetIDs[spreadsheetIndex];
            try
            {
                List<string> tableNames = _sheetsHelper.GetSheetNames(selectedID);

                tableDropdown.ClearOptions();
                tableDropdown.AddOptions(tableNames);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load table names for spreadsheet ID: {selectedID}. Error: {ex.Message}");
                tableDropdown.ClearOptions();
            }
        }
    }
}
