using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace FirebaseIntegration
{
    public class FirebaseHelper
    {
        private string _projectId;
        private readonly FirebaseAuthHelper _authHelper;

        public FirebaseHelper(string serviceAccountPath)
        {
            _authHelper = new FirebaseAuthHelper(serviceAccountPath);
        }

        public void SetProjectId(string projectId)
        {
            _projectId = projectId;
        }

        public async Task<RemoteConfigResponse> GetRemoteConfigAsync()
        {
            string url = $"https://firebaseremoteconfig.googleapis.com/v1/projects/{_projectId}/remoteConfig";
            string token = await _authHelper.GetAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("Failed to obtain access token.");
                return null;
            }

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.LogError($"Failed to fetch Remote Config: {response.StatusCode}");
                    Debug.LogError(await response.Content.ReadAsStringAsync());
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RemoteConfigResponse>(jsonResponse);
            }
        }

        public async Task<bool> UpdateRemoteConfigAsync(Dictionary<string, RemoteConfigParameter> parameters)
        {
            string url = $"https://firebaseremoteconfig.googleapis.com/v1/projects/{_projectId}/remoteConfig";
            string token = await _authHelper.GetAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("Failed to obtain access token.");
                return false;
            }

            var payload = new { parameters };
            string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("If-Match", "*");

                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.LogError($"Failed to update Remote Config: {response.StatusCode}");
                    Debug.LogError(await response.Content.ReadAsStringAsync());
                    return false;
                }

                Debug.Log("Remote Config updated successfully.");
                return true;
            }
        }
    }

    public class RemoteConfigResponse
    {
        [JsonProperty("parameters")]
        public Dictionary<string, RemoteConfigParameter> Parameters { get; set; }
    }

    public class RemoteConfigParameter
    {
        [JsonProperty("defaultValue")]
        public DefaultValue DefaultValue { get; set; }

        [JsonProperty("valueType")]
        public string ValueType { get; set; }
    }

    public class DefaultValue
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
