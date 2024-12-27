using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading.Tasks;

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
                .CreateScoped(new[] { "https://www.googleapis.com/auth/firebase.remoteconfig" });
        }

        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}