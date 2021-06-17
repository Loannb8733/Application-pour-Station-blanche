using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
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
    /// Logique d'interaction pour BranchementCle.xaml
    /// </summary>
    public partial class ConnectionKey : Page
    {
        private ManagementEventWatcher mwe_connection;
        
        private DriveInfo[] tabDrives = null;
        private DriveInfo lockedDrive;


        public ConnectionKey()
        {
            InitializeComponent();
            txtMessageConnectionKey.Text = "Veuillez brancher votre clé";
            USB_DetectionArrived();
        }

        private void USB_DetectionArrived()
        {
            //initialisation de la classe WqlEventQuery
            WqlEventQuery connection = new WqlEventQuery();

            //Contient le nom de la classe d'évenement à interroger
            connection.EventClassName = "__InstanceCreationEvent";

            //Durée de détection de la clé, affichage du MessageBox au bout d'1s
            connection.WithinInterval = new TimeSpan(0, 0, 1);

            //Renvoie une valeur contenant la ou les conditions dans la requête d'événement
            connection.Condition = @"TargetInstance ISA 'Win32_DiskDriveToDiskPartition'";

            //Initialiser un observateur d'événements à laquelle on passe la variable connection
            mwe_connection = new ManagementEventWatcher(connection);

            //Appel de la méthode pour savoir quoi faire une fois la clé détecté est branché
            //Se produit quand un nouvel évènement arrive
            mwe_connection.EventArrived += new EventArrivedEventHandler(USBEventArrived_Connection);

            //Lance la détection pour le branchement de la clé
            mwe_connection.Start();
        }

        //------------------------------------------------

        private void USBEventArrived_Connection(object sender, EventArrivedEventArgs e)
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
                        IShellProperty prop = ShellObject.FromParsingName("" + disk["Name"]).Properties.GetProperty("System.Volume.BitLockerProtection");
                        int? bitLockerProtectionStatus = (prop as ShellProperty<int?>).Value;

                        lockedDrive = lastCase;

                        if (bitLockerProtectionStatus.HasValue && (bitLockerProtectionStatus == 6))
                        {
                            string message = "Le lecteur USB " + disk["Name"] + " vient d'être inséré \nCe lecteur est protégé.";
                            MessageBox.Show(
                                message,
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            this.Dispatcher.Invoke(() =>
                            {
                                this.NavigationService.Navigate(new Uri("PasswordPage.xaml", UriKind.Relative));
                            });
                        }
                        else
                        {
                            var result = MessageBox.Show(
                                "Le lecteur USB " + disk["Name"] + " vient d'être inséré" +
                                '\n' + "Ce lecteur n'est pas protégé",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            if (result == MessageBoxResult.OK)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    this.NavigationService.Navigate(new Uri("AnalysingCancelBtn.xaml", UriKind.Relative));
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}
