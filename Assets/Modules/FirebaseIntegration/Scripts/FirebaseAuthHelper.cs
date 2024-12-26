using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;

namespace FirebaseIntegration
{
    public class FirebaseAuthHelper
    {
        private readonly string _serviceAccountPath;

        public FirebaseAuthHelper(string serviceAccountPath)
        {
            _serviceAccountPath = serviceAccountPath;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            GoogleCredential credential;

            using (var stream = new FileStream(_serviceAccountPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(new[]
                    {
                        "https://www.googleapis.com/auth/cloud-platform",
                        "https://www.googleapis.com/auth/firebase.remoteconfig"
                    }); 
            }

            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            return token;
        }
    }
}