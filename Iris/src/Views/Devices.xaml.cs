using Iris.Database;
using Iris.Devices;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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

        /// <summary>
        /// The ItemSource for <see cref="DevicesDataGrid"/>.
        /// </summary>
        private List<Device> LoadedDevices { get; set; }
        /// <summary>
        /// The currently selected device in <see cref="DevicesDataGrid"/>.
        /// </summary>
        private Device SelectedDevice { get; set; } = null;

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
        #region UserControl
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDevices();
        }
        #endregion

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

        #region Devices DataGrid
        private void DevicesDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SelectedDevice = DevicesDataGrid.SelectedItem as Device;
        }
        #endregion

        #region RefreshDevicesButton
        private void RefreshDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDevices();
        }
        #endregion

        #region DeleteDeviceButton
        private void DeleteDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Verify if SelectedDevice is null when deleted here.

            if (SelectedDevice is null)
            {
                MessageBox.Show("Es ist kein Gerät ausgewählt.", "Kein Gerät ausgewählt", MessageBoxButton.OK);
                return;
            }

            if (MessageBox.Show($"Soll '{SelectedDevice.Type}, {SelectedDevice.Name}' wirklich gelöscht werden?", $"{SelectedDevice.Name} löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning).Equals(MessageBoxResult.Yes))
            {
                DatabaseHandler.DeleteDevice(SelectedDevice.ID);
                LoadDevices();
            }
        }
        #endregion

        #region AddDeviceCancelButton
        private void AddDeviceCancelButton_Click(object sender, RoutedEventArgs e)
        {
            NewDeviceNameTextBox.Text = "";
            NewDeviceTypeComboBox.SelectedIndex = -1;
            NewDeviceNotesRichTextBox.Document.Blocks.Clear();
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

        /// <summary>
        /// Load the devices out of the database.
        /// </summary>
        private async void LoadDevices()
        {
            LoadedDevices = await DatabaseHandler.SelectAllDevices();
            DevicesDataGrid.ItemsSource = LoadedDevices;
        }
        #endregion

        #endregion
    }
}
