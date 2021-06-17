using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace project
{
    using BitLockerManager;
    using Microsoft.Win32.SafeHandles;
    using Microsoft.WindowsAPICodePack.Shell;
    using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
    using System.ComponentModel;
    using System.Management;
    using System.Threading;

    /// <summary>
    /// Logique d'interaction pour AnalysingCancelBtn.xaml
    /// </summary>
    public partial class AnalysingCancelBtn : Page
    {
        private DriveInfo[] tabDrives = null;
        private DriveInfo lockedDrive;

        public AnalysingCancelBtn()
        {
            InitializeComponent();
            letterKeyCaseTab();
        }

        private void letterKeyCaseTab()
        {
            tabDrives = BitLockerManager.EnumDrives();
            var tabSize = tabDrives.Length - 1;
            var lastCase = tabDrives[tabSize];

            foreach (ManagementObject drive in new ManagementObjectSearcher("select * from Win32_DiskDrive where InterfaceType='USB'").Get())
            {
                foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"] + "'} WHERE AssocClass =Win32_DiskDriveToDiskPartition").Get())
                {
                    foreach (ManagementObject disk in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass =Win32_LogicalDiskToPartition").Get())
                    {
                        lockedDrive = lastCase;
                    }
                }
            }
        }


        private async void btnAnalysingDrive_Click(object sender, RoutedEventArgs e)
        {
            var process = new Process();
            var fileInfoLockedDrive = new FileInfo(@" " + lockedDrive);

            var processStartInfo = new ProcessStartInfo("C:\\Program Files\\Windows Defender\\MpCmdRun.exe")
            {
                /*
                 * ScanType 3 : Analyses des fichiers et des répertoires
                 * File : Chemin d'accès\               
                 */

                Arguments = $"-Scan -ScanType 3 -File" + fileInfoLockedDrive,

                //Processus démarre dans une autre fenêtre
                CreateNoWindow = true,

                //boite de dialogue non retourné si erreur
                ErrorDialog = false,

                //Fenetre masqué lors du demarrage du processus
                WindowStyle = ProcessWindowStyle.Hidden,

                //Shell a pas besoin d etre utilisé pour le demarrage du process
                UseShellExecute = false
            };

            process.StartInfo = processStartInfo;
            process.Start();

            btnAnalyse.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;
            pbStatus.Visibility = Visibility.Visible;

            await Task.Factory.StartNew(() =>
            {
                process.WaitForExit();
            });

            if (process.ExitCode == 0)
            {
                btnAnalyse.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
                pbStatus.Visibility = Visibility.Hidden;

                var result = MessageBox.Show("Votre clé ne contient pas de virus",
                "Information",
                 MessageBoxButton.OK,
                 MessageBoxImage.Information);

                if(result == MessageBoxResult.OK)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.NavigationService.Navigate(new Uri("DisconnectionKey.xaml", UriKind.Relative));
                    });
                }
            }
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Êtes-vous certain de vouloir annuler l'opération ? ",
                "Attention",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    this.Dispatcher.Invoke(() =>
                    {
                        this.NavigationService.Navigate(new Uri("DisconnectionKey.xaml", UriKind.Relative));
                    });
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }
    }
}
