using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientLibrary
{
    public static class LastParentFolderHandler
    {
        private static readonly string resourcePath = "Resources/LastParentFolder.txt";
        private static string currentDirectory = Directory.GetCurrentDirectory();

        public static string GetLastParentFolderName()
        {
            string parentName = null;
            string path = FindFileAbove(currentDirectory, resourcePath);
            if (path == null) return parentName;
            parentName = File.ReadAllText(path);
            return parentName;
        }
        public static void SaveLastParentFolderName(string parentFolderName)
        {
            try
            {
                string fullFilePath = FindFileAbove(currentDirectory, resourcePath);
                File.WriteAllText(fullFilePath, parentFolderName);
            }
            catch (Exception ex)
            {
                return;
            }
        }


        public static void CleanLastFilePath()
        {
            string fullFilePath = FindFileAbove(currentDirectory, resourcePath);
            File.WriteAllText(fullFilePath, string.Empty);
        }

        private static string FindFileAbove(string startDirectory, string fileName)
        {
            // Пошук файлу вище по ієрархії
            string currentDirectory = startDirectory;

            while (currentDirectory != null)
            {
                string filePath = Path.Combine(currentDirectory, fileName);

                if (File.Exists(filePath))
                {
                    return filePath;
                }

                // Переходимо вище в ієрархії
                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }

            return null;
        }
    }
}
