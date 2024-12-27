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

        private const string OutputFileName = "output.json";

        private async void Start()
        {
            string serviceAccountPath = CredentialsManager.FirebaseCredentialsPath;

            if (string.IsNullOrEmpty(serviceAccountPath) || !File.Exists(serviceAccountPath))
            {
                PopupManager.Instance.ShowSimplePopup("Firebase service account file not found.");
                return;
            }

            _firebaseHelper = new FirebaseHelper(serviceAccountPath);
            _outputFilePath = Path.Combine(Application.persistentDataPath, OutputFileName);
            _firebaseModel = new FirebaseModel();

            _firebaseModel.LoadFromFile();
            UpdateProjectsDropdown();

            addProjectButton.onClick.AddListener(OnAddProjectButtonClick);
            removeProjectButton.onClick.AddListener(OnRemoveProjectButtonClick);
            uploadJsonButton.onClick.AddListener(OnUploadJsonButtonClick);

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

        private async void OnAddProjectButtonClick()
        {
            await AddProject();
        }

        private void OnRemoveProjectButtonClick()
        {
            RemoveProject();
        }

        private async void OnUploadJsonButtonClick()
        {
            await UploadJson();
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
            RemoteConfigResponse remoteConfig = await _firebaseHelper.GetRemoteConfigAsync();

            if (remoteConfig == null || remoteConfig.Parameters.Count == 0)
            {
                PopupManager.Instance.ShowSimplePopup($"No variables found for project: {projectId}. Project not added.");
                return;
            }

            _firebaseModel.AddProjectId(projectId);
            _firebaseModel.SaveToFile();
            UpdateProjectsDropdown();

            await FetchVariables(projectId);

            PopupManager.Instance.ShowSimplePopup($"Project ID {projectId} added successfully.");
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

        private async Task FetchVariables(string projectId)
        {
            PopupManager.Instance.ShowSimplePopup($"Fetching Remote Config variables for project: {projectId}");
            _firebaseHelper.SetProjectId(projectId);

            RemoteConfigResponse remoteConfig = await _firebaseHelper.GetRemoteConfigAsync();

            if (remoteConfig == null || remoteConfig.Parameters == null || remoteConfig.Parameters.Count == 0)
            {
                PopupManager.Instance.ShowSimplePopup($"No variables found for project: {projectId}");
                variablesDropdown.ClearOptions();
                return;
            }

            var jsonVariables = new List<string>();
            foreach (var parameter in remoteConfig.Parameters)
            {
                if (parameter.Value.ValueType == "JSON")
                {
                    jsonVariables.Add(parameter.Key);
                }
            }

            if (jsonVariables.Count == 0)
            {
                PopupManager.Instance.ShowSimplePopup($"No JSON variables found for project: {projectId}");
                variablesDropdown.ClearOptions();
                return;
            }

            variablesDropdown.ClearOptions();
            variablesDropdown.AddOptions(jsonVariables);
            PopupManager.Instance.ShowSimplePopup($"Loaded {jsonVariables.Count} JSON variables for project: {projectId}");
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

        private async Task UploadJson()
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
            
            uploadJsonButton.interactable = false;

            string jsonContent = File.ReadAllText(_outputFilePath);

            RemoteConfigResponse remoteConfig = await _firebaseHelper.GetRemoteConfigAsync();
            if (remoteConfig == null || remoteConfig.Parameters == null)
            {
                PopupManager.Instance.ShowSimplePopup("Failed to fetch existing Remote Config variables.");
                return;
            }

            if (remoteConfig.Parameters.TryGetValue(selectedVariable, out var parameter))
            {
                parameter.DefaultValue.Value = jsonContent;
            }
            else
            {
                remoteConfig.Parameters[selectedVariable] = new RemoteConfigParameter
                {
                    DefaultValue = new DefaultValue { Value = jsonContent },
                    ValueType = "JSON"
                };
            }

            bool success = await _firebaseHelper.UpdateRemoteConfigAsync(remoteConfig.Parameters);
            if (success)
            {
                PopupManager.Instance.ShowSimplePopup("Remote Config updated successfully.");
            }
            else
            {
                PopupManager.Instance.ShowSimplePopup("Failed to update Remote Config.");
            }
            
            uploadJsonButton.interactable = true;
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
