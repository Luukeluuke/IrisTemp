using Iris.Database;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Iris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties and Variables
        private MenuTab SelectedMenuTab 
        {
            get => selectedMenuTab;
            set
            {
                if (!selectedMenuTab.Equals(value))
                {
                    //Deselect the old active menu tab
                    switch (selectedMenuTab)
                    {
                        case MenuTab.Startpage:
                            {
                                StartpageMenuButton.Background = Global.MaterialDesignDarkBackground;
                                break;
                            }
                        case MenuTab.Borrowings:
                            {
                                BorrowingsMenuButton.Background = Global.MaterialDesignDarkBackground;
                                break;
                            }
                        case MenuTab.Devices:
                            {
                                DevicesMenuButton.Background = Global.MaterialDesignDarkBackground;
                                break;
                            }
                    }

                    //Set the new active menu tab & change content frame
                    switch (value)
                    {
                        case MenuTab.Startpage:
                            {
                                StartpageMenuButton.Background = Global.MaterialDesignDarkSeparatorBackground;
                                ChangeContentFrame(MenuTab.Startpage);

                                break;
                            }
                        case MenuTab.Borrowings:
                            {
                                BorrowingsMenuButton.Background = Global.MaterialDesignDarkSeparatorBackground;
                                ChangeContentFrame(MenuTab.Borrowings);

                                break;
                            }
                        case MenuTab.Devices:
                            {
                                DevicesMenuButton.Background = Global.MaterialDesignDarkSeparatorBackground;
                                ChangeContentFrame(MenuTab.Devices);

                                break;
                            }
                    }
                }

                selectedMenuTab = value;
            }
        }
        private MenuTab selectedMenuTab = MenuTab.Startpage;

        private enum MenuTab
        {
            Startpage,
            Borrowings,
            Devices
        }
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        #region Window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeContentFrame(MenuTab.Startpage);

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
            SelectedMenuTab = MenuTab.Startpage;
        }

        private void BorrowingsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMenuTab = MenuTab.Borrowings;
        }

        private void DevicesMenuButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMenuTab = MenuTab.Devices;
        }
        #endregion
        #endregion

        #region Methods
        #region Private
        /// <summary>
        /// Changes the active/visible UserControl of ContentGrid.
        /// </summary>
        /// <param name="menuTab">The menu tab you want to show.</param>
        private void ChangeContentFrame(MenuTab menuTab)
        {
            string uriString = "";

            switch (menuTab)
            {
                case MenuTab.Startpage:
                    {
                        uriString = "src/Views/Startpage.xaml";
                        break;
                    }
                case MenuTab.Borrowings:
                    {
                        uriString = "src/Views/Borrowings.xaml";
                        break;
                    }
                case MenuTab.Devices:
                    {
                        uriString = "src/Views/Devices.xaml";
                        break;
                    }
            }

            ContentGrid.Children.Clear();
            ContentGrid.Children.Add(new Frame() { Source = new Uri(uriString, UriKind.Relative) });
        }
        #endregion

        #endregion
    }
}
