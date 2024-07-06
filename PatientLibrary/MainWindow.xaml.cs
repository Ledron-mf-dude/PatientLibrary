using Google.Apis.Drive.v3;
using Microsoft.Win32;
using PatientLibrary.GoogleDriveAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;

namespace PatientLibrary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            var lastFileStream = LastSelectedFileHandler.GetLastSelectedFileStream();
            if(lastFileStream != null)
            {
                GoogleAuth(lastFileStream);
            }
        }

        private void UploadSecretFileClick(object sender, RoutedEventArgs e)
        {
            GoogleAuth();
        }

        private void GoogleAuth(Stream stream = null)
        {
            if (stream == null)
            {
                GoogleAothBySelectedFile();
            }
            else
            {
                try
                {
                    GoogleAothByLastLoadFile(stream);
                }
                catch
                {
                    GoogleAothBySelectedFile();
                }
            }
        }

        private void GoogleAothByLastLoadFile(Stream stream)
        {
            GoogleDriveAuthorization googleDriveAuthorization = new GoogleDriveAuthorization();
            GoogleDriveAuthorizationResult authorizationResult = googleDriveAuthorization.GetService(stream);
            if (authorizationResult.IsSuccessfully)
            {
                GoogleDriveService.SetDriveService(authorizationResult.DriveService);
                MainFrame.NavigationService.Navigate(new MainWorkingPage());
            }
            else
            {
                LastSelectedFileHandler.CleanLastFilePath();
            }
        }

        private void GoogleAothBySelectedFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                using (Stream stream = openFileDialog.OpenFile())
                {
                    GoogleDriveAuthorization googleDriveAuthorization = new GoogleDriveAuthorization();
                    GoogleDriveAuthorizationResult authorizationResult = googleDriveAuthorization.GetService(stream);
                    if (authorizationResult.IsSuccessfully)
                    {
                        GoogleDriveService.SetDriveService(authorizationResult.DriveService);
                        LastSelectedFileHandler.SaveLastFilePath(openFileDialog.FileName);
                        MainFrame.NavigationService.Navigate(new MainWorkingPage());
                    }
                    else
                    {
                        MessageBox.Show(authorizationResult.ErrorText);
                    }
                }
            }
        }
    }
}
