using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace PatientLibrary
{
    public class GoogleDriveHelper
    {
        private readonly string fields = "nextPageToken, files(id, name, mimeType, parents, createdTime, modifiedTime)";
        private readonly List<string> _subFolderList = new List<string> {"Первинні огляди", "Щоденники", "Консиліуми", "Виписні епікризи"};
        private DriveService _driveService;
        public Dictionary<string, string> _mimeTypeAndExtantion =
            new Dictionary<string, string>
            {
                {"application/rtf"  ,".rtf" },
                {"text/plain"   ,".txt" },
                {"application/epub+zip" ,".epub" },
                {"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"    ,".xlsx" },
                {"application/x-vnd.oasis.opendocument.spreadsheet" ,".ods" },
                {"text/csv" ,".csv" },
                {"text/tab-separated-values"    ,".tsv" },
                {"application/vnd.openxmlformats-officedocument.presentationml.presentation"    ,".pptx" },
                {"application/vnd.oasis.opendocument.presentation"  ,".odp" },
                {"image/jpeg"   ,".jpg" },
                {"image/svg+xml"    ,".svg" },
                {"application/vnd.google-apps.script+json"  ,".json" },
                { "image/png", ".png"},
                { "video/mp4", ".mp4" },
                { "application/pdf", ".pdf" },
                { "application/zip", ".zip" },
                { "application/msword", ".doc" },
                { "application/vnd.ms-excel", ".xls" },
                { "application/vnd.ms-powerpoint", ".ppt" },
                { "application/vnd.google-apps.document", ".docx" },
                { "application/vnd.google-apps.spreadsheet", ".xlsx" },
                { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx" },
                { "application/vnd.oasis.opendocument.text", ".odt" }
            };

        public GoogleDriveHelper(DriveService driveService)
        {
            _driveService = driveService;
        }

        public string CreateFolder(string folderName, string parentId = null, bool needCreatePatientSubfolder = false)
        {
            var folder = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = string.IsNullOrEmpty(parentId) ? null : new List<string> { parentId }
            };

            try
            {
                var request = _driveService.Files.Create(folder);
                request.Fields = "id";
                var createdFolder = request.Execute();
                if(needCreatePatientSubfolder && createdFolder.Id != null)
                {
                    string subFolderParentId = createdFolder.Id;
                    foreach (var newFolderName in _subFolderList)
                    {
                        CreateFolder(newFolderName, subFolderParentId);
                    }
                }
                
                return createdFolder.Id;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при створенні папки: {ex.Message}");
                return null;
            }
        }

        public List<FormFile> GetAllFolders()
        {
            FilesResource.ListRequest listRequest = _driveService.Files.List();
            listRequest.Fields = fields;
            listRequest.Q = "mimeType='application/vnd.google-apps.folder'"; // Фільтр за типом: папки

            FileList fileList = listRequest.Execute();
            List<FormFile> sortedList = fileList.Files.Select((x)=> { return new FormFile(x.Name, x.Id); }).OrderBy(f => f.Name).ToList();
            return sortedList;
        }

        public List<FormFile> GetChildFolders(string parentId)
        {
            FilesResource.ListRequest listRequest = _driveService.Files.List();
            listRequest.Fields = fields;
            listRequest.Q = $"mimeType='application/vnd.google-apps.folder' and parents='{parentId}'";

            FileList fileList = listRequest.Execute();
            var sortedList = fileList.Files.Select((x) => { return new FormFile(x.Name, x.Id); }).OrderBy(f => f.Name).ToList();
            return sortedList;
        }

        public List<FormFile> GetFileList(string parentId)
        {
            Dictionary<string, string> resultFileDict = new Dictionary<string, string>();
            
            FilesResource.ListRequest listRequest = _driveService.Files.List();
            listRequest.Fields = fields;
            listRequest.Q = $"mimeType != 'application/vnd.google-apps.folder' and parents='{parentId}'";

            FileList fileList = listRequest.Execute();
            var sortedList = fileList.Files.Select((x) => { return new FormFile(x.Name, x.Id); }).OrderBy(f => f.Name).ToList();
            return sortedList;
        }

        public List<FormFile> SearchFileByUploadDate(DateTime searchDate, string parentFolderId = "")
        {
            Dictionary<string, string> searchFileResultDict = new Dictionary<string, string>();

            FilesResource.ListRequest listRequest = _driveService.Files.List();
            listRequest.Q = $"createdTime > '{searchDate.ToString("yyyy-MM-dd'T'HH:mm:ss")}' and mimeType != 'application/vnd.google-apps.folder'";
            if(!string.IsNullOrEmpty(parentFolderId))
            {
                listRequest.Q += $"and '{parentFolderId}' in parents";
            }
            FileList fileList = listRequest.Execute();
            var sortedList = fileList.Files.Select((x) => { return new FormFile(x.Name, x.Id); }).OrderBy(f => f.Name).ToList();
            return sortedList;
        }

        public List<FormFile> SearchInFile(string searchText)
        {
            FilesResource.ListRequest listRequest = _driveService.Files.List();
            listRequest.Fields = fields;
            listRequest.Q = $"fullText contains '{searchText}'"; // Фільтр за вмістом файлів

            FileList fileList = listRequest.Execute();
            var sortedList = fileList.Files.Select((x) => { return new FormFile(x.Name, x.Id); }).OrderBy(f => f.Name).ToList();
            return sortedList;
        }

        public string DownloadFile(string fileId, string localFilePath)
        {
            string downLoadResult = "";
            try
            {
                var getFileRequest = _driveService.Files.Get(fileId);
                var file = getFileRequest.Execute();
                if(file == null)
                {
                    return downLoadResult;
                }
                var stream = new MemoryStream();
                //var request = _driveService.Files.Get(fileId);
                if (file.MimeType.Contains("application/vnd.google-apps."))
                {
                    string mimeType;
                    if(!file.Name.EndsWith(".doc") && !file.Name.EndsWith(".docx") && (file.Name.EndsWith(".xls") || file.Name.EndsWith(".xlsx")))
                    {
                        mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    }
                    else if((file.Name.EndsWith(".doc") || file.Name.EndsWith(".docx")) && !file.Name.EndsWith(".xls") && !file.Name.EndsWith(".xlsx"))
                    {
                        mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    }
                    else
                    {
                        if(file.MimeType == "application/vnd.google-apps.document")
                        {
                            mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        }
                        else if(file.MimeType == "application/vnd.google-apps.spreadsheet")
                        {
                            mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        }
                        else
                        {
                            mimeType = file.MimeType;
                        }
                        
                    }
                    var request = _driveService.Files.Export(fileId, mimeType);
                    request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Downloading:
                                {
                                    MessageBox.Show("Downloading...");
                                    break;
                                }
                            case DownloadStatus.Completed:
                                {
                                    MessageBox.Show("Download complete.");
                                    break;
                                }
                            case DownloadStatus.Failed:
                                {
                                    MessageBox.Show($"Download failed: {progress.Exception}");
                                    break;
                                }
                        }
                    };
                    request.Download(stream);
                }
                else
                {
                    var request = _driveService.Files.Get(fileId);
                    request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Downloading:
                                {
                                    MessageBox.Show("Downloading...");
                                    break;
                                }
                            case DownloadStatus.Completed:
                                {
                                    MessageBox.Show("Download complete.");
                                    break;
                                }
                            case DownloadStatus.Failed:
                                {
                                    MessageBox.Show($"Download failed: {progress.Exception}");
                                    break;
                                }
                        }
                    };
                    request.Download(stream);
                }
                string path;
                if(file.Name.EndsWith(_mimeTypeAndExtantion[file.MimeType]))
                {
                    path = localFilePath +"\\"+ file.Name;
                }
                else
                {
                    path = localFilePath + $"\\{file.Name}{_mimeTypeAndExtantion[file.MimeType]}";
                }
                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }

                downLoadResult = $"File downloaded to: {localFilePath}";
            }
            catch (Exception e)
            {
                downLoadResult = $"An error occurred: {e.Message}";
            }
            return downLoadResult;
        }
    }
}
