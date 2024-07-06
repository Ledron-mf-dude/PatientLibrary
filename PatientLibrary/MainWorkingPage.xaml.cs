using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Timer = System.Threading.Timer;
using System.DirectoryServices;
using System.IO;
using Path = System.IO.Path;
using System.Security.AccessControl;
using Microsoft.Win32;
using PatientLibrary.GoogleDriveAuth;
using System.Management;
using Google.Apis.Drive.v3.Data;
using File = Google.Apis.Drive.v3.Data.File;

namespace PatientLibrary
{
    /// <summary>
    /// Interaction logic for MainWorkingPage.xaml
    /// </summary>
    public partial class MainWorkingPage : Page
    {
        private DriveService service;
        private GoogleDriveHelper googleDriveHelper;
        private string patientId = "";
        private Timer delayTimer;
        public MainWorkingPage()
        {
            InitializeComponent();
            delayTimer = new Timer(OnTimedEvent, null, Timeout.Infinite, Timeout.Infinite);
            if (GoogleDriveService.GetDriveService() == null)
            {
                new MainWindow().Show();
            }
            else
            {
                service = GoogleDriveService.GetDriveService();
            }
            googleDriveHelper = new GoogleDriveHelper(service);
            var folders = googleDriveHelper.GetAllFolders();
            CacheManager.AllFolder = folders;
            FoldersComboBox.ItemsSource = folders;
            string lastParentFolderName = LastParentFolderHandler.GetLastParentFolderName();
            if (folders.Any(x => x.Name == lastParentFolderName))
            {
                FormFile lastSelectedFolder = folders.First(x => x.Name == lastParentFolderName);
                FoldersComboBox.SelectedItem = lastSelectedFolder;
            }
        }

        private void FoldersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFolder = (FormFile)FoldersComboBox.SelectedItem;
            var selectedFolderFile = CacheManager.AllFolder.FirstOrDefault(x => x.Id == selectedFolder.Id);
            if (selectedFolderFile != null)
            {
                GeneratPatientListBox(selectedFolderFile.Id);
                LastParentFolderHandler.SaveLastParentFolderName(selectedFolderFile.Name);
            }
            else
            {
                PatientListBox.ItemsSource = null;
                PatientListBox.SelectedItem = null;
            }
        }

        private void GeneratPatientListBox(string parentId)
        {
            var patients = this.googleDriveHelper.GetChildFolders(parentId);
            CacheManager.Patients = patients;
            PatientListBox.ItemsSource = patients;
        }

        private void SearchPatientButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchPatientTextBox.Text;
            if (CacheManager.PatientFolders == null) return;
            if (!string.IsNullOrEmpty(searchText))
            {
                var searchResult = CacheManager.PatientFolders.Where(x => x.Name.Contains(searchText)).ToList();
                PatientListBox.ItemsSource = searchResult;
            }
            else
            {
                PatientListBox.ItemsSource = CacheManager.PatientFolders;
            }
        }

        private void SearchPatientTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            delayTimer.Change(1000, Timeout.Infinite);
        }

        private void OnTimedEvent(object state)
        {
            Dispatcher.Invoke(() =>
            {
                SearchPatientButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            });
        }

        private void PatientListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPatient = (FormFile)PatientListBox.SelectedItem;
            PatientFolderListBox.ItemsSource = null;
            PatientFolderListBox.SelectedItem = null;
            PatientFolderItemsListBox.ItemsSource = null;
            PatientFolderItemsListBox.SelectedItem = null;
            FileList.ItemsSource = null;
            FileList.SelectedItem = null;
            patientId = "";
            if (selectedPatient != null && CacheManager.Patients.Contains(selectedPatient))
            {
                PatientLabel.Content = $"Patient: {selectedPatient.Name}";
                GeneratPatientFolders(selectedPatient.Id);
            }
        }

        private void GeneratPatientFolders(string parentId)
        {
            var patientFolders = this.googleDriveHelper.GetChildFolders(parentId);
            CacheManager.PatientFolders = patientFolders;
            PatientFolderListBox.ItemsSource = patientFolders;
        }

        private void PatientFolderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPatientFolder = (FormFile)PatientFolderListBox.SelectedItem;

            if (selectedPatientFolder != null)
            {
                GeneratPatientFolderItemsListBox(selectedPatientFolder.Id);
                GeneratFileList(selectedPatientFolder.Id);
                patientId = selectedPatientFolder.Id;
            }
        }

        private void GeneratPatientFolderItemsListBox(string parentId)
        {
            PatientFolderItemsListBox.ItemsSource = null;
            PatientFolderItemsListBox.SelectedItem = null;
            var PatientFolderItems = this.googleDriveHelper.GetChildFolders(parentId);
            
            PatientFolderItemsListBox.ItemsSource = PatientFolderItems;
            CacheManager.PatientFoldersItems = PatientFolderItems;
        }

        private void GeneratFileList(string parentId)
        {
            FileList.ItemsSource = null;
            FileList.SelectedItem = null;
            var fileList = this.googleDriveHelper.GetFileList(parentId);
            FileList.ItemsSource = fileList;
        }

        private void PatientFolderItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFolder = (FormFile)PatientFolderItemsListBox.SelectedItem;

            if (selectedFolder != null)
            {
                GeneratFileList(selectedFolder.Id);
                patientId = selectedFolder.Id;
            }
        }

        private void SearchInFileButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchInFileTextBox.Text;
            if (!string.IsNullOrEmpty(searchText))
            {
                GeneratSearchFileList(searchText);
            }
            else
            {
                SearchFileList.ItemsSource = null;
            }
        }

        private void GeneratSearchFileList(string searchText)
        {
            SearchFileList.ItemsSource = null;
            SearchFileList.SelectedItem = null;
            var searchFileResult = googleDriveHelper.SearchInFile(searchText);
            SearchFileList.ItemsSource = searchFileResult;
        }

        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileList.SelectedItem != null)
            {
                SearchFileList.SelectedItem = null;
            }
        }

        private void SearchFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SearchFileList.SelectedItem != null)
            {
                FileList.SelectedItem = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FormFile selectedFile = null;
            if(FileList.SelectedItem != null)
            {
                selectedFile = (FormFile)FileList.SelectedItem;
            }
            if(SearchFileList.SelectedItem != null)
            {
                selectedFile = (FormFile)SearchFileList.SelectedItem;
            }
            if(selectedFile != null)
            {
                string fileId = selectedFile.Id;
                if(!string.IsNullOrEmpty(fileId))
                {
                    string projectDirectory = Directory.GetCurrentDirectory();
                    var a = Directory.GetParent(projectDirectory).FullName;
                    var b = Directory.GetParent(a).FullName;
                    var c = Directory.GetParent(b).FullName;
                    string authDirectory = Path.Combine(c, "Downloads");
                    string path = authDirectory;

                    if(!Directory.Exists(authDirectory))
                    {
                        var directoryInfo = Directory.CreateDirectory(authDirectory);
                        DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

                        directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.CreateFiles, AccessControlType.Allow));
                        directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Delete, AccessControlType.Allow));
                        directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.ReadAndExecute, AccessControlType.Allow));
                        directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow));

                        directoryInfo.SetAccessControl(directorySecurity);
                        path = directoryInfo.FullName;
                    }
                    googleDriveHelper.DownloadFile(fileId, path);
                }
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            string parentId = this.patientId;
            if(string.IsNullOrEmpty(parentId))
            {
                MessageBox.Show("Оберіть папку.");
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName.Split("\\").Last();
                string extantion = '.'+fileName.Split(".").Last();
                string mimeType = "";
                if (googleDriveHelper._mimeTypeAndExtantion.ContainsValue(extantion))
                {
                    if(extantion == ".docx")
                    {
                        mimeType = googleDriveHelper._mimeTypeAndExtantion.LastOrDefault(x => x.Value == extantion).Key;
                    }
                    else
                    {
                        mimeType = googleDriveHelper._mimeTypeAndExtantion.FirstOrDefault(x => x.Value == extantion).Key;
                    }
                    
                }
                else
                {
                    MessageBox.Show("Додаток не розрахований на цей тип файлу.");
                    return;
                }
                using (Stream stream = openFileDialog.OpenFile())
                {
                    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = fileName,
                        Parents = new[] { parentId },
                    };

                    FilesResource.CreateMediaUpload request;
                    request = service.Files.Create(fileMetadata, stream, mimeType);
                    request.Upload();

                    var file = request.ResponseBody;
                    if(file == null)
                    {
                        MessageBox.Show("Помилка створення файлу. Спробуйте завантажити його через інтерфейс Google Drive.");
                    }
                    else
                    {
                        GeneratFileList(parentId);
                        MessageBox.Show("Файл додано успішно.");
                    }
                    
                }
            }
        }

        private void AddPatientButton_Click(object sender, RoutedEventArgs e)
        {
            string patientName = AddPatientTextBox.Text;
            string parentId = null;
            var selectedFolder = (FormFile)FoldersComboBox.SelectedItem;
            if (selectedFolder != null)
            {
                parentId = selectedFolder.Id;
            }
            if(!string.IsNullOrEmpty(patientName) && !string.IsNullOrEmpty(parentId))
            {
                var newFolderId = googleDriveHelper.CreateFolder(patientName, parentId, true);
                if(newFolderId != null)
                {
                    GeneratPatientListBox(parentId);
                    if(PatientListBox.Items.Contains(patientName))
                    {
                        PatientListBox.SelectedItem = patientName;
                    }
                    MessageBox.Show("Папку нового пацієнта створено.");
                }
            }
        }

        private void AddPatientFolderFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string folderName = AddPatientFolderTextBox.Text;
            string parentId = null;
            var selectedPatient = (FormFile)PatientListBox.SelectedItem;
            if(selectedPatient != null)
            {
                parentId = selectedPatient.Id;
            }
            if (!string.IsNullOrEmpty(folderName) && !string.IsNullOrEmpty(parentId))
            {
                var newFolderId = googleDriveHelper.CreateFolder(folderName, parentId);
                if (newFolderId != null)
                {
                    GeneratPatientFolders(parentId);
                    if (PatientFolderListBox.Items.Contains(folderName))
                    {
                        PatientFolderListBox.SelectedItem = folderName;
                    }
                    MessageBox.Show("Папку створено.");
                }
            }
        }

        private void AddDiaryFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string diaryName = AddDiaryTextBox.Text;
            string parentId = null;
            var selectedFolder = (FormFile)PatientFolderListBox.SelectedItem;
            if (selectedFolder != null)
            {
                parentId = selectedFolder.Id;
            }
            if (!string.IsNullOrEmpty(diaryName) && !string.IsNullOrEmpty(parentId))
            {
                var newFolderId = googleDriveHelper.CreateFolder(diaryName, parentId);
                if (newFolderId != null)
                {
                    GeneratPatientFolderItemsListBox(parentId);
                    if (PatientFolderItemsListBox.Items.Contains(diaryName))
                    {
                        PatientFolderItemsListBox.SelectedItem = diaryName;
                    }
                    MessageBox.Show("Папку створено.");
                }
            }
        }

        private void SearchByUploadDate_Click(object sender, RoutedEventArgs e)
        {
            var selectedDateValue = UploadDatePicker.SelectedDate;
            if (selectedDateValue == null)
            {
                SearchFileList.ItemsSource = null;
                SearchFileList.SelectedItem = null;
                MessageBox.Show("Select a date to search.");
                return;
            }
            DateTime searchUploadDate = (DateTime)selectedDateValue;
            string parentFolderId = "";
            var searchResult = googleDriveHelper.SearchFileByUploadDate(searchUploadDate, parentFolderId);
            SearchFileList.ItemsSource = searchResult;
        }
    }
}
