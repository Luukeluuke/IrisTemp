using System.Windows;

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

                                //TODO: Change COntent frame

                                break;
                            }
                        case MenuTab.Borrowings:
                            {
                                BorrowingsMenuButton.Background = Global.MaterialDesignDarkSeparatorBackground;

                                //TODO: Change COntent frame

                                break;
                            }
                        case MenuTab.Devices:
                            {
                                DevicesMenuButton.Background = Global.MaterialDesignDarkSeparatorBackground;

                                //TODO: Change COntent frame

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

        #endregion
    }
}
