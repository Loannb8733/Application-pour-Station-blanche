using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// <summary>
    /// Logique d'interaction pour DisconnectionKey.xaml
    /// </summary>
    public partial class DisconnectionKey : Page
    {
        private ManagementEventWatcher mwe_disconnection;

        public DisconnectionKey()
        {
            InitializeComponent();
            
            txtMessageDisconnectionKey.Text = "Veuillez débrancher votre clé";
            USB_DetectionDeletion();
        }

        private void USB_DetectionDeletion() //FAIT
        {
            WqlEventQuery disconnection = new WqlEventQuery();
            disconnection.EventClassName = "__InstanceDeletionEvent";

            //Durée de détection de la clé, affichage du MessageBox au bout d'1s
            disconnection.WithinInterval = new TimeSpan(0, 0, 1);

            //Renvoie une valeur contenant la ou les conditions dans la requête d'événement
            disconnection.Condition = @"TargetInstance ISA 'Win32_DiskDriveToDiskPartition'  ";

            //Initialiser un observateur d'événements à laquelle on passe la variable disconnection
            mwe_disconnection = new ManagementEventWatcher(disconnection);

            //Appel de la méthode pour savoir quoi faire une fois la clé détecté et débranché
            //Se produit quand un nouvel évènement arrive
            mwe_disconnection.EventArrived += new EventArrivedEventHandler(USBEventArrived_mwe_Disconnection);

            //lance la détection pour le débranchement de la clé
            mwe_disconnection.Start();
        }

        private void USBEventArrived_mwe_Disconnection(object sender, EventArrivedEventArgs e) //FAIT
        {
            MessageBox.Show("Le lecteur USB vient d'être retiré",
                "Attention",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            this.Dispatcher.Invoke(() =>
            {
                //NavigationService.Navigate(new MainWindow());

                /*MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.Visibility = Visibility.Visible;
                this.Visibility = Visibility.Hidden;*/
                Application.Current.Shutdown();
            });
        }
    }
}
