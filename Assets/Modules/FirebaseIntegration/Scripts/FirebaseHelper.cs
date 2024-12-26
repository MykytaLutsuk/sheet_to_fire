using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace FirebaseIntegration
{
    public class FirebaseHelper
    {
        private string _projectId;
        
        private readonly FirebaseAuthHelper _authHelper;
        private readonly HttpClient _httpClient;

        public FirebaseHelper(string serviceAccountPath)
        {
            _authHelper = new FirebaseAuthHelper(serviceAccountPath);
            _httpClient = new HttpClient();
        }

        public void SetProjectId(string projectId)
        {
            _projectId = projectId;
        }
        
        public async Task<List<string>> GetRemoteConfigVariablesAsync()
        {
            try
            {
                string token = await _authHelper.GetAccessTokenAsync();

                string url = $"https://firebaseremoteconfig.googleapis.com/v1/projects/{_projectId}/remoteConfig";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Debug.LogError($"Failed to fetch Remote Config: {response.StatusCode}");
                    Debug.LogError($"Error response: {errorResponse}");
                    return new List<string>();
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                Debug.Log($"Remote Config Response: {jsonResponse}");

                var remoteConfig = JsonConvert.DeserializeObject<RemoteConfigResponse>(jsonResponse);
                var variableNames = new List<string>();

                if (remoteConfig?.Parameters != null)
                {
                    foreach (var key in remoteConfig.Parameters.Keys)
                    {
                        variableNames.Add(key);
                    }
                }

                return variableNames;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error fetching Remote Config variables: {ex.Message}");
                return new List<string>();
            }
        }
        
        public async void UpdateRemoteConfigAsync(string variableName, string jsonContent)
        {
            string token = await _authHelper.GetAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("Failed to obtain access token.");
                return;
            }

            string url = $"https://firebaseremoteconfig.googleapis.com/v1/projects/{_projectId}/remoteConfig";
            
            string jsonPayload = GenerateRemoteConfigPayload(variableName, jsonContent);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                client.DefaultRequestHeaders.Add("If-Match", "*");
                
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Debug.Log("Remote Config updated successfully.");
                }
                else
                {
                    Debug.LogError($"Failed to update Remote Config: {response.StatusCode}");
                    Debug.LogError(await response.Content.ReadAsStringAsync());
                }
            }
        }
        
        private string GenerateRemoteConfigPayload(string variableName, string newValue)
        {
            newValue = JsonConvert.SerializeObject(newValue);
            
            string cleanedJsonString = RemoveLineBreaks(newValue);
            cleanedJsonString = RemoveExtraSpaces(cleanedJsonString);
            cleanedJsonString = RemoveFirstAndLastCharacter(cleanedJsonString);
            cleanedJsonString = JsonConvert.SerializeObject(cleanedJsonString);
            
            string payload = $@"
                                {{
                                    ""parameters"": {{
                                        ""{variableName}"": {{
                                            ""defaultValue"": {{
                                                ""value"": {cleanedJsonString}
                                            }}
                                        }}
                                    }}
                                }}";

            Debug.Log($"Generated Payload: {payload}");
            return payload;
        }
        
        private static string RemoveFirstAndLastCharacter(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= 1)
                return string.Empty;

            return input.Substring(1, input.Length - 2);
        }
        
        private static string RemoveExtraSpaces(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            input = input.Trim();

            return Regex.Replace(input, @"\s+", " ");
        }
        
        private static string RemoveLineBreaks(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input.Replace("\\r", "").Replace("\\n", "").Replace("\\", "");
        }
     
        // Клас для десеріалізації відповіді Remote Config
        private class RemoteConfigResponse
        {
            [JsonProperty("parameters")]
            public Dictionary<string, object> Parameters { get; set; }
        }
    }
}
