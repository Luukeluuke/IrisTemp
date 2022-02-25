using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Iris.src.Views
{
    //TODO: Bei EditDevicesGrid alle Elemente Disablen wenn kein Gerät ausgewählt

    /// <summary>
    /// Interaktionslogik für Devices.xaml
    /// </summary>
    public partial class Devices : UserControl
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
                        case MenuTab.AddDevice:
                            {
                                NewDeviceButton.Background = Global.MaterialDesignDarkBackground;
                                break;
                            }
                        case MenuTab.EditDevice:
                            {
                                EditDeviceButton.Background = Global.MaterialDesignDarkBackground;
                                break;
                            }
                    }

                    //Set the new active menu tab & change content frame
                    switch (value)
                    {
                        case MenuTab.AddDevice:
                            {
                                NewDeviceButton.Background = Global.MaterialDesignDarkSeparatorBackground;
                                ChangeContentGrid(MenuTab.AddDevice);

                                break;
                            }
                        case MenuTab.EditDevice:
                            {
                                EditDeviceButton.Background = Global.MaterialDesignDarkSeparatorBackground;
                                ChangeContentGrid(MenuTab.EditDevice);

                                break;
                            }
                    }
                }

                selectedMenuTab = value;
            }
        }
        private MenuTab selectedMenuTab = MenuTab.AddDevice;

        private enum MenuTab
        {
            AddDevice,
            EditDevice
        }
        #endregion

        #region Constructors
        public Devices()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        #region MenuButtons
        private void NewDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMenuTab = MenuTab.AddDevice;
        }

        private void EditDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMenuTab = MenuTab.EditDevice;
        }
        #endregion
        #endregion

        #region Methods
        #region Private
        /// <summary>
        /// Changes the content of add a new device or edit a device.
        /// </summary>
        /// <param name="menuTab">The wanted menu tab.</param>
        private void ChangeContentGrid(MenuTab menuTab)
        {
            switch (menuTab)
            {
                case MenuTab.AddDevice:
                    {
                        NewDeviceGrid.Visibility = Visibility.Visible;
                        EditDeviceGrid.Visibility = Visibility.Hidden;
                        break;
                    }
                case MenuTab.EditDevice:
                    {
                        EditDeviceGrid.Visibility = Visibility.Visible;
                        NewDeviceGrid.Visibility = Visibility.Hidden;
                        break;
                    }
            }
        }
        #endregion

        #endregion
    }
}
