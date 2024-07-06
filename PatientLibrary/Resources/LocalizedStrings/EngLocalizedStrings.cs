using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PatientLibrary.Resources.LocalizedStrings
{
    public static class EngLocalizedStrings
    {
        static ResourceDictionary resourceDictionary = new ResourceDictionary
        {
            {"GoogleDriveAuthException", "Google Drive authorization exception. Try upload anothe file."},
            {"UploadSecretFile", "Upload secret file." }
        };

        public static ResourceDictionary GetResource()
        {
            return resourceDictionary;
        }
    }
}
