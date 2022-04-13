using Iris.Database;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Iris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Properties and Variables
        public UserControl SelectedMenuTab
        {
            get => selectedMenuTab;
            set
            {
                if (value?.GetType() == selectedMenuTab?.GetType())
                    return;
                
                //Deselect the old active menu tab
                switch (selectedMenuTab)
                {
                    case src.Views.Startpage _:
                        {
                            StartpageMenuButton.Background = Global.MaterialDesignDarkBackground;
                            break;
                        }
                    case src.Views.Borrowings _:
                        {
                            BorrowingsMenuButton.Background = Global.MaterialDesignDarkBackground;
                            break;
                        }
                    case src.Views.Devices _:
                        {
                            DevicesMenuButton.Background = Global.MaterialDesignDarkBackground;
                            break;
                        }
                }

                //Set the new active menu tab & change content frame
                switch (value)
                {
                    case src.Views.Startpage _:
                        {
                            StartpageMenuButton.Background = Global.MaterialDesignDarkSeparatorBackground;
                            break;
                        }
                    case src.Views.Borrowings _:
                        {
                            BorrowingsMenuButton.Background = Global.MaterialDesignDarkSeparatorBackground;
                            break;
                        }
                    case src.Views.Devices _:
                        {
                            DevicesMenuButton.Background = Global.MaterialDesignDarkSeparatorBackground;
                            break;
                        }
                }
                selectedMenuTab = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedMenuTab)));                
            }
        }
        private UserControl selectedMenuTab;

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            SelectedMenuTab = new src.Views.Startpage();
        }
        #endregion

        #region Events
        #region Window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DatabaseHandler.IsConnected)
            {
                MessageBox.Show("Es konnte keine Verbindung zur Datenbank hergestellt werden.", "Keine Datenbank Verbindung", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            Global.MainWindow = this;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DatabaseHandler.CloseDBConnection();
        }
        #endregion

        #region MenuButtons
        private void StartpageMenuButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMenuTab = new src.Views.Startpage();
        }

        private void BorrowingsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMenuTab = new src.Views.Borrowings();
        }

        private void DevicesMenuButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMenuTab = new src.Views.Devices();;
        }
        #endregion
        #endregion
    }
}
