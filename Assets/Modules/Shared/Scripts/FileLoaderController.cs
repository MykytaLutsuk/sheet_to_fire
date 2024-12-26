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
        public Button loadMainSceneButton;
        public Button deleteCredentialsButton;
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
            loadMainSceneButton.onClick.AddListener(LoadMainScene);
            deleteCredentialsButton.onClick.AddListener(DeleteCredentials);
        }

        private void CheckExistingFiles()
        {
            bool sheetsExists = File.Exists(_sheetsCredentialsPath);
            bool firebaseExists = File.Exists(_firebaseCredentialsPath);

            if (sheetsExists && firebaseExists)
            {
                statusText.text = "Both credentials files are uploaded successfully. You can proceed to the next step.";
                uploadGoogleCredentialsButton.interactable = false;
                uploadFirebaseCredentialsButton.interactable = false;
                loadMainSceneButton.interactable = true;
                deleteCredentialsButton.interactable = true;
            }
            else if (sheetsExists)
            {
                statusText.text = "Google Sheets credentials uploaded. Please upload Firebase credentials to proceed.";
                uploadGoogleCredentialsButton.interactable = false;
                uploadFirebaseCredentialsButton.interactable = true;
                loadMainSceneButton.interactable = false;
                deleteCredentialsButton.interactable = true;
            }
            else if (firebaseExists)
            {
                statusText.text = "Firebase credentials uploaded. Please upload Google Sheets credentials to proceed.";
                uploadGoogleCredentialsButton.interactable = true;
                uploadFirebaseCredentialsButton.interactable = false;
                loadMainSceneButton.interactable = false;
                deleteCredentialsButton.interactable = true;
            }
            else
            {
                statusText.text = "No credentials found. Please upload both Google Sheets and Firebase credentials to proceed.";
                uploadGoogleCredentialsButton.interactable = true;
                uploadFirebaseCredentialsButton.interactable = true;
                loadMainSceneButton.interactable = false;
                deleteCredentialsButton.interactable = false;
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

        private void LoadMainScene()
        {
            CredentialsManager.SheetsCredentialsPath = _sheetsCredentialsPath;
            CredentialsManager.FirebaseCredentialsPath = _firebaseCredentialsPath;
            SceneManager.LoadScene("MainScene");
        }

        private void DeleteCredentials()
        {
            if (File.Exists(_sheetsCredentialsPath))
            {
                File.Delete(_sheetsCredentialsPath);
            }

            if (File.Exists(_firebaseCredentialsPath))
            {
                File.Delete(_firebaseCredentialsPath);
            }

            statusText.text = "Credentials files deleted. Please upload both files to proceed.";
            CheckExistingFiles();
        }
    }
}
