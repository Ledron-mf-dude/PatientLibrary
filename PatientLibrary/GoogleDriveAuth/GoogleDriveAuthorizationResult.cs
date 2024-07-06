using Google.Apis.Drive.v3;

namespace PatientLibrary.GoogleDriveAuth
{
    public class GoogleDriveAuthorizationResult
    {
        DriveService driveService;
        bool isSuccessfully;
        string errorText;

        public DriveService DriveService { get => driveService; }
        public bool IsSuccessfully { get => isSuccessfully; }
        public string ErrorText { get => errorText; }

        public GoogleDriveAuthorizationResult(DriveService driveService, bool IsSuccessfully, string errorText)
        {
            this.driveService = driveService;
            this.isSuccessfully = IsSuccessfully;
            this.errorText = errorText;
        }
    }
}
