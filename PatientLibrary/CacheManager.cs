using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientLibrary
{
    public static class CacheManager
    {
        private static List<FormFile> patientFolders;
        private static List<FormFile> patients;
        private static List<FormFile> allFolder;

        public static List<FormFile> AllFolder { get => allFolder; set => allFolder = value; }
        public static List<FormFile> Patients { get => patients; set => patients = value; }
        public static List<FormFile> PatientFolders { get => patientFolders; set => patientFolders = value; }
        public static List<FormFile> PatientFoldersItems { get; internal set; }
    }
}
