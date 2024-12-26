using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Shared;
using TMPro;

namespace FirebaseIntegration
{
    public class FirebaseController : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_InputField projectIdInputField;
        public Button addProjectButton;
        public Button removeProjectButton;
        public TMP_Dropdown projectsDropdown;
        public TMP_Dropdown variablesDropdown;
        public Button uploadJsonButton;

        private FirebaseHelper _firebaseHelper;
        private string _outputFilePath;
        private FirebaseModel _firebaseModel;

        private async void Start()
        {
            string serviceAccountPath = CredentialsManager.FirebaseCredentialsPath;

            if (string.IsNullOrEmpty(serviceAccountPath) || !File.Exists(serviceAccountPath))
            {
                PopupManager.Instance.ShowSimplePopup("Firebase service account file not found.");
                return;
            }

            _firebaseHelper = new FirebaseHelper(serviceAccountPath);
            _outputFilePath = Path.Combine(Application.persistentDataPath, "output.json");
            _firebaseModel = new FirebaseModel();

            _firebaseModel.LoadFromFile();
            UpdateProjectsDropdown();

            addProjectButton.onClick.AddListener(async () => await AddProject());
            removeProjectButton.onClick.AddListener(RemoveProject);
            uploadJsonButton.onClick.AddListener(UploadJson);

            if (_firebaseModel.ProjectIds.Count > 0)
            {
                await FetchVariables(_firebaseModel.ProjectIds[0]);
            }

            projectsDropdown.onValueChanged.AddListener(async delegate
            {
                string selectedProject = GetSelectedProject();
                if (!string.IsNullOrEmpty(selectedProject))
                {
                    await FetchVariables(selectedProject);
                }
            });
        }

        private async Task AddProject()
        {
            string projectId = projectIdInputField.text.Trim();

            if (string.IsNullOrEmpty(projectId))
            {
                PopupManager.Instance.ShowSimplePopup("Project ID is empty. Please enter a valid Project ID.");
                return;
            }

            if (_firebaseModel.ProjectIds.Contains(projectId))
            {
                PopupManager.Instance.ShowSimplePopup($"Project ID {projectId} already exists in the list.");
                return;
            }

            PopupManager.Instance.ShowSimplePopup($"Fetching variables for project: {projectId}");
            _firebaseHelper.SetProjectId(projectId);
            List<string> variables = await _firebaseHelper.GetRemoteConfigVariablesAsync();

            if (variables == null || variables.Count == 0)
            {
                PopupManager.Instance.ShowSimplePopup($"No variables found for project: {projectId}. Project not added.");
                return;
            }

            _firebaseModel.AddProjectId(projectId);
            _firebaseModel.SaveToFile();
            UpdateProjectsDropdown();

            await FetchVariables(projectId);

            PopupManager.Instance.ShowSimplePopup($"Project ID {projectId} added successfully with {variables.Count} variables.");
        }

        private void RemoveProject()
        {
            string selectedProject = GetSelectedProject();

            if (string.IsNullOrEmpty(selectedProject))
            {
                PopupManager.Instance.ShowSimplePopup("No project selected to remove.");
                return;
            }

            _firebaseModel.RemoveProjectId(selectedProject);
            _firebaseModel.SaveToFile();
            UpdateProjectsDropdown();

            PopupManager.Instance.ShowSimplePopup($"Project ID {selectedProject} removed successfully.");
        }

        private void UpdateProjectsDropdown()
        {
            projectsDropdown.ClearOptions();
            projectsDropdown.AddOptions(_firebaseModel.ProjectIds);

            if (_firebaseModel.ProjectIds.Count <= 0)
            {
                variablesDropdown.ClearOptions();
            }
        }

        private async Task FetchVariables(string projectId)
        {
            PopupManager.Instance.ShowSimplePopup($"Fetching Remote Config variables for project: {projectId}");
            _firebaseHelper.SetProjectId(projectId);

            List<string> variables = await _firebaseHelper.GetRemoteConfigVariablesAsync();

            if (variables == null || variables.Count == 0)
            {
                PopupManager.Instance.ShowSimplePopup($"No variables found for project: {projectId}");
                variablesDropdown.ClearOptions();
                return;
            }

            variablesDropdown.ClearOptions();
            variablesDropdown.AddOptions(variables);
            PopupManager.Instance.ShowSimplePopup($"Loaded {variables.Count} variables for project: {projectId}");
        }

        private void UploadJson()
        {
            string selectedProject = GetSelectedProject();
            string selectedVariable = GetSelectedVariable();

            if (string.IsNullOrEmpty(selectedProject) || string.IsNullOrEmpty(selectedVariable))
            {
                PopupManager.Instance.ShowSimplePopup("Please select a project and a variable.");
                return;
            }

            if (!File.Exists(_outputFilePath))
            {
                PopupManager.Instance.ShowSimplePopup("Output file not found.");
                return;
            }

            string jsonContent = File.ReadAllText(_outputFilePath);
            _firebaseHelper.UpdateRemoteConfigAsync(selectedVariable, jsonContent);
            PopupManager.Instance.ShowSimplePopup("Remote Config updated successfully.");
        }

        private string GetSelectedProject()
        {
            if (projectsDropdown.options.Count > 0)
            {
                return projectsDropdown.options[projectsDropdown.value].text;
            }
            return null;
        }

        private string GetSelectedVariable()
        {
            if (variablesDropdown.options.Count > 0)
            {
                return variablesDropdown.options[variablesDropdown.value].text;
            }
            return null;
        }
    }
}
