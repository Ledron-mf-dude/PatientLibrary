using System;
using System.IO;
using Path = System.IO.Path;

namespace PatientLibrary
{
    public static class LastSelectedFileHandler
    {
        private static readonly string resourcePath = "Resources/LastSecretFilePath.txt";
        private static string currentDirectory = Directory.GetCurrentDirectory();
        public static Stream GetLastSelectedFileStream()
        {
            Stream stream = null;
            string path = FindFileAbove(currentDirectory, resourcePath);
            if (path == null) return stream;
            string lastFilePath = File.ReadAllText(path);
            if(File.Exists(lastFilePath))
            {
                stream = File.OpenRead(lastFilePath);
            }
            return stream;
        }
        public static void SaveLastFilePath(string path)
        {
            try
            {
                string fullFilePath = FindFileAbove(currentDirectory, resourcePath);
                File.WriteAllText(fullFilePath, path);
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
