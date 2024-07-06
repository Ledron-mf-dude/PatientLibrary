using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientLibrary
{
    public static class GoogleDriveService
    {
        private static DriveService driveService = null;

        public static void SetDriveService(DriveService service)
        {
            driveService = service;
        }

        public static DriveService GetDriveService()
        {
            return driveService;
        }
    }
}
