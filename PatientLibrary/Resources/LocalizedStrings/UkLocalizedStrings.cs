using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PatientLibrary.Resources.LocalizedStrings
{
    public static class UkLocalizedStrings
    {
        static ResourceDictionary resourceDictionary = new ResourceDictionary
        {
            {"GoogleDriveAuthException", "Помилка авторизації до Google Drive. Спробуйте завантажини інший файл."},
            {"UploadSecretFile", "Завантажити секретний файл." }
        };

        public static ResourceDictionary GetResource()
        {
            return resourceDictionary;
        }
    }
}
