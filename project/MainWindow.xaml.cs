using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
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
    using Microsoft.WindowsAPICodePack.Shell;
    using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DriveInfo[] tabDrives = null;
        private DriveInfo lockedDrive;
        private ManagementEventWatcher mwe_connection;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txtMessage.Text = "Bienvenue";
            alreadyConnction();
        }

        private bool alreadyConnction()
        {
            tabDrives = BitLockerManager.EnumDrives();
            var taille = tabDrives.Length - 1;
            var lastCase = tabDrives[taille];

            foreach (ManagementObject drive in new ManagementObjectSearcher("select * from Win32_DiskDrive where InterfaceType='USB'").Get())
            {
                foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"] + "'} WHERE AssocClass =Win32_DiskDriveToDiskPartition").Get())
                {
                    foreach (ManagementObject disk in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass =Win32_LogicalDiskToPartition").Get())
                    {
                        IShellProperty prop = ShellObject.FromParsingName("" + disk["Name"]).Properties.GetProperty("System.Volume.BitLockerProtection");
                        int? bitLockerProtectionStatus = (prop as ShellProperty<int?>).Value;

                        lockedDrive = lastCase;

                        if (bitLockerProtectionStatus.HasValue && (bitLockerProtectionStatus == 6))
                        {
                            string message = "Ce lecteur est protégé.";
                            MessageBox.Show(
                                message,
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            this.Dispatcher.Invoke(() =>
                            {
                                PrincipalWindow.Navigate(new PasswordPage());
                            });
                        }
                        else
                        {
                            MessageBox.Show(
                                "Ce lecteur n'est pas protégé",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            this.Dispatcher.Invoke(() =>
                            {
                                PrincipalWindow.Navigate(new AnalysingCancelBtn());
                            });
                        }
                    }
                }
            }
            return true;
        }


        private void btnUSB_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                PrincipalWindow.Navigate(new ConnectionKey());
            });
        }
    }
}
