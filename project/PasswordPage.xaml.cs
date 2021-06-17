using System;
using System.Collections.Generic;
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
    using BitlockerManager;
    using BitLockerManager;
    using Microsoft.WindowsAPICodePack.Shell;
    using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management;

    /// <summary>
    /// Logique d'interaction pour PasswordPage.xaml
    /// </summary>
    public partial class PasswordPage : Page
    {

        public string password = String.Empty;
        private DriveInfo lockedDrive;

        private ManagementObject _vol;
        private DriveInfo[] tabDrives;

        public PasswordPage()
        {
            InitializeComponent();
            letterKeyCaseTab();
            txtPassword.Clear();
        }

        private void letterKeyCaseTab()
        {
            tabDrives = BitLockerManager.EnumDrives();
            var tabSize = tabDrives.Length - 1;
            var derniereCase = tabDrives[tabSize];

            foreach (ManagementObject drive in new ManagementObjectSearcher("select * from Win32_DiskDrive where InterfaceType='USB'").Get())
            {
                foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"] + "'} WHERE AssocClass =Win32_DiskDriveToDiskPartition").Get())
                {
                    foreach (ManagementObject disk in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass =Win32_LogicalDiskToPartition").Get())
                    {
                        lockedDrive = derniereCase;
                    }
                }
            }
        }

        public void UnlockDriveWithPassphrase(string password)
        {
            using (var inParams = _vol.GetMethodParameters("UnlockWithPassphrase"))
            {
                inParams["Passphrase"] = password;
                using (var outParams = _vol.InvokeMethod("UnlockWithPassphrase", inParams, null))
                {
                    if (outParams == null)
                    {
                        throw new InvalidOperationException("Impossible d'appeler la méthode. Les paramètres de sortie sont null.");
                    }

                    var result = (uint)outParams["returnValue"];
                    switch (result)
                    {
                        case 0:
                            return;
                        case 0x80310008:
                            throw new InvalidOperationException("BitLocker n'est pas activé sur le volume. Ajouter un protecteur de clé pour activer BitLocker.").SetCode(0x80310008);
                        case 0x8031006C:
                            throw new InvalidOperationException("Le paramètre de politique de groupe qui exige la conformité FIPS a empêché la génération ou l'utilisation du mot de passe.").SetCode(0x8031006C);
                        case 0x80310080:
                            throw new InvalidOperationException("Le mot de passe fourni ne répond pas aux exigences de longueur minimale ou maximale.").SetCode(0x80310080);
                        case 0x80310081:
                            throw new InvalidOperationException("Le mot de passe ne répond pas aux exigences de complexité définies par l'administrateur dans la stratégie de groupe.").SetCode(0x80310081);
                        case 0x80310027:
                            throw new InvalidOperationException("Le volume ne peut pas être déverrouillé avec les informations fournies.").SetCode(0x80310027);
                        case 0x80310033:
                            throw new InvalidOperationException("Le protecteur de clé fourni n'existe pas sur le volume. Vous devez entrer un autre protecteur de clé.").SetCode(0x80310033);
                        default:
                            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Code inconnu {0:X}.", result));
                    }
                }
            }
        }

        private BitLockerManager GetCurrentManager()
        {
            foreach (ManagementObject drive in new ManagementObjectSearcher("select * from Win32_DiskDrive where InterfaceType='USB'").Get())
            {
                foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"] + "'} WHERE AssocClass =Win32_DiskDriveToDiskPartition").Get())
                {
                    foreach (ManagementObject disk in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass =Win32_LogicalDiskToPartition").Get())
                    {
                        //Retourne la lettre du lecteur bloqué
                        return new BitLockerManager(lockedDrive);
                    }
                }
            }
            return null;
        }

        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            password = txtPassword.Password;
            var drive = this.GetCurrentManager();

            try
            {
                drive.UnlockDriveWithPassphrase(password);

                MessageBox.Show(
                        "Le clé a bien été dévérouillée",
                        "Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                this.Dispatcher.Invoke(() =>
                {
                    this.NavigationService.Navigate(new Uri("AnalysingCancelBtn.xaml", UriKind.Relative));
                });
            }
            catch
            {
                MessageBoxResult result =
                    MessageBox.Show(
                        "Mot de passe eronné \nVoulez vous ressayer ?",
                        "Erreur",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        this.Dispatcher.Invoke(() =>
                        {
                            txtPassword.Clear();
                        });
                        break;
                    case MessageBoxResult.No:
                        this.Dispatcher.Invoke(() =>
                        {
                            this.NavigationService.Navigate(new Uri("DisconnectionKey.xaml", UriKind.Relative));
                        });
                        break;
                }
            }
        }

        private void btnKeyboard_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startKeyboard = new ProcessStartInfo(@"C:\Windows\System32\osk.exe");
            startKeyboard.UseShellExecute = true;
            Process.Start(startKeyboard);
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
