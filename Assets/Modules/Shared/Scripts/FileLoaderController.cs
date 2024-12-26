using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

namespace Shared
{
    public class FileLoaderController : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button uploadGoogleCredentialsButton;
        public Button uploadFirebaseCredentialsButton;
        public TMP_Text statusText;

        private string _sheetsCredentialsPath;
        private string _firebaseCredentialsPath;

        private void Start()
        {
            _sheetsCredentialsPath = Path.Combine(Application.persistentDataPath, "google_credentials.json");
            _firebaseCredentialsPath = Path.Combine(Application.persistentDataPath, "firebase_credentials.json");

            CheckExistingFiles();

            uploadGoogleCredentialsButton.onClick.AddListener(() => UploadFile("google_credentials.json", _sheetsCredentialsPath));
            uploadFirebaseCredentialsButton.onClick.AddListener(() => UploadFile("firebase_credentials.json", _firebaseCredentialsPath));
        }

        private void CheckExistingFiles()
        {
            bool sheetsExists = File.Exists(_sheetsCredentialsPath);
            bool firebaseExists = File.Exists(_firebaseCredentialsPath);

            if (sheetsExists && firebaseExists)
            {
                statusText.text = "Both credentials files found. You can proceed.";
                ProceedToNextScene();
            }
            else
            {
                statusText.text = "Please upload both credentials files.";
            }
        }

        private void UploadFile(string fileName, string targetPath)
        {
            string path = FilePicker.ShowFilePicker($"Select {fileName}", "json");
            if (string.IsNullOrEmpty(path))
            {
                statusText.text = $"No file selected for {fileName}.";
                return;
            }

            if (!ValidateJsonFile(path))
            {
                statusText.text = $"Invalid file: {fileName}. Please try again.";
                return;
            }

            File.Copy(path, targetPath, true);
            statusText.text = $"{fileName} uploaded successfully.";
            CheckExistingFiles();
        }

        private bool ValidateJsonFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var obj = JsonUtility.FromJson<object>(json);
                return obj != null;
            }
            catch
            {
                return false;
            }
        }

        private void ProceedToNextScene()
        {
            CredentialsManager.SheetsCredentialsPath = _sheetsCredentialsPath;
            CredentialsManager.FirebaseCredentialsPath = _firebaseCredentialsPath;
            SceneManager.LoadScene("DataManager");
        }
    }
}
