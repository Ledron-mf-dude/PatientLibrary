using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;

namespace PatientLibrary.GoogleDriveAuth
{
    public class GoogleDriveAuthorization
    {
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Patient Library";
        static StringLocalizer stringLocalizer = new StringLocalizer();
        static DriveService driveService;
        public GoogleDriveAuthorization()
        {
            
        }

        public GoogleDriveAuthorizationResult GetService(Stream file)
        {
            if (file == null)
            {
                return new GoogleDriveAuthorizationResult(null, false, stringLocalizer["GoogleDriveAuthException"]);
            }

            try
            {
                UserCredential credential;
                using (file)
                {
                    string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(file).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                }

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                driveService = service;
                return new GoogleDriveAuthorizationResult(service, true, "");
            }
            catch (Exception)
            {
                return new GoogleDriveAuthorizationResult(null, false, stringLocalizer["GoogleDriveAuthException"]);
            }
        }
    }
}
