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

        private const string GoogleCredentialsName = "google_credentials.json";
        private const string FirebaseCredentialsName = "firebase_credentials.json";
        
        private string _googleCredentialsPath;
        private string _firebaseCredentialsPath;

        private void Start()
        {
            _googleCredentialsPath = Path.Combine(Application.persistentDataPath, GoogleCredentialsName);
            _firebaseCredentialsPath = Path.Combine(Application.persistentDataPath, FirebaseCredentialsName);

            CheckExistingFiles();

            uploadGoogleCredentialsButton.onClick.AddListener(() => UploadFile(GoogleCredentialsName, _googleCredentialsPath));
            uploadFirebaseCredentialsButton.onClick.AddListener(() => UploadFile(FirebaseCredentialsName, _firebaseCredentialsPath));
        }

        private void CheckExistingFiles()
        {
            bool sheetsExists = File.Exists(_googleCredentialsPath);
            bool firebaseExists = File.Exists(_firebaseCredentialsPath);

            if (sheetsExists && firebaseExists)
            {
                statusText.text = "Both credentials files are uploaded successfully. You can proceed to the next step.";
                uploadGoogleCredentialsButton.interactable = false;
                uploadFirebaseCredentialsButton.interactable = false;
                ProceedToNextScene();
            }
            else if (sheetsExists)
            {
                statusText.text = "Google Sheets credentials uploaded. Please upload Firebase credentials to proceed.";
                uploadGoogleCredentialsButton.interactable = false;
                uploadFirebaseCredentialsButton.interactable = true;
            }
            else if (firebaseExists)
            {
                statusText.text = "Firebase credentials uploaded. Please upload Google Sheets credentials to proceed.";
                uploadGoogleCredentialsButton.interactable = true;
                uploadFirebaseCredentialsButton.interactable = false;
            }
            else
            {
                statusText.text = "No credentials found. Please upload both Google Sheets and Firebase credentials to proceed.";
                uploadGoogleCredentialsButton.interactable = true;
                uploadFirebaseCredentialsButton.interactable = true;
            }
        }

        private void UploadFile(string fileName, string targetPath)
        {
            string path = FilePicker.ShowFilePicker($"Select {fileName}", "json");
            if (string.IsNullOrEmpty(path))
            {
                statusText.text = $"No file selected for {fileName}. Please try again.";
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
            CredentialsManager.SheetsCredentialsPath = _googleCredentialsPath;
            CredentialsManager.FirebaseCredentialsPath = _firebaseCredentialsPath;
            SceneManager.LoadScene("DataManager");
        }
    }
}
