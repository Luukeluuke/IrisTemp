using Iris.Database;
using Iris.Structures;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Iris.src.Views
{
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
        private IEnumerable<Device>? LoadedDevices { get; set; }
        /// <summary>
        /// The currently selected device in <see cref="DevicesDataGrid"/>.
        /// </summary>
        private Device? SelectedDevice { get; set; }

        private List<DeviceType> SelectedDeviceTypes { get; set; }

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

            SelectedDevice = null;
            SelectedDeviceTypes = new List<DeviceType>();
        }
        #endregion

        #region Events
        #region UserControl
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FilterNotebookCheckBox.IsChecked = true;
            FilterGigaCubeCheckBox.IsChecked = true;
            FilterERKMeetingCheckBox.IsChecked = true;
            FilterSpecialCheckBox.IsChecked = true;

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

            if (SelectedDevice is not null)
            {
                //Enable controls
                EditDeviceNameTextBox.IsEnabled = true;
                EditDeviceBlockedCheckBox.IsEnabled = true;
                EditDeviceNotesRichTextBox.IsEnabled = true;
                EditDeviceCancelButton.IsEnabled = true;
                EditDeviceConfirmButton.IsEnabled = true;

                DeleteDeviceButton.IsEnabled = true;

                //Load data into controls
                EditDeviceNameTextBox.Text = SelectedDevice.Name;
                EditDeviceTypeComboBox.SelectedIndex = (int)SelectedDevice.Type - 1;
                EditDeviceBlockedCheckBox.IsChecked = SelectedDevice.IsBlocked;
                EditDeviceNotesRichTextBox.Document.Blocks.Clear();
                EditDeviceNotesRichTextBox.Document.Blocks.Add(new Paragraph(new Run(SelectedDevice.Notes.Trim())));
            }
            else
            {
                EditDeviceNameTextBox.IsEnabled = false;
                EditDeviceBlockedCheckBox.IsEnabled = false;
                EditDeviceNotesRichTextBox.IsEnabled = false;
                EditDeviceCancelButton.IsEnabled = false;
                EditDeviceConfirmButton.IsEnabled = false;

                DeleteDeviceButton.IsEnabled = false;

                EditDeviceNameTextBox.Text = "";
                EditDeviceTypeComboBox.SelectedIndex = -1;
                EditDeviceBlockedCheckBox.IsChecked = false;
                EditDeviceNotesRichTextBox.Document.Blocks.Clear();
            }
        }
        #endregion

        #region RefreshDevicesButton
        private void RefreshDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDevices();
        }
        #endregion

        #region DeleteDeviceButton
        private async void DeleteDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDevice is null)
            {
                MessageBox.Show("Es ist kein Gerät ausgewählt.", "Kein Gerät ausgewählt", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (DataHandler.Borrowings.Any(b => b.DeviceID.Equals(SelectedDevice.ID)))
            {
                MessageBox.Show("Das ausgewählte Gerät kann nicht gelöscht werden, da bereits Ausleihen mit diesem Gerät existieren.", $"{SelectedDevice.Name} löschen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Soll '{SelectedDevice.Type}, {SelectedDevice.Name}' wirklich gelöscht werden?", $"{SelectedDevice.Name} löschen", MessageBoxButton.YesNo, MessageBoxImage.Question).Equals(MessageBoxResult.Yes))
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

        #region AddDeviceConfirmButton
        private async void AddDeviceConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewDeviceNameTextBox.Text))
            {
                MessageBox.Show("Es muss ein Name angegeben werden.", "Ungültiger Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewDeviceTypeComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Es muss ein Gerätetyp angegeben werden.", "Ungültiger Gerätetyp", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await Device.CreateNewDevice(NewDeviceNameTextBox.Text, new TextRange(NewDeviceNotesRichTextBox.Document.ContentStart, NewDeviceNotesRichTextBox.Document.ContentEnd).Text.Trim(), (DeviceType)(NewDeviceTypeComboBox.SelectedIndex + 1));
            LoadDevices();

            AddDeviceCancelButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        #endregion

        #region EditDeviceCancelButton
        private void EditDeviceCancelButton_Click(object sender, RoutedEventArgs e)
        {
            EditDeviceNameTextBox.Text = SelectedDevice.Name;
            EditDeviceTypeComboBox.SelectedIndex = (int)SelectedDevice.Type - 1;
            EditDeviceNotesRichTextBox.Document.Blocks.Clear();
            EditDeviceNotesRichTextBox.Document.Blocks.Add(new Paragraph(new Run(SelectedDevice.Notes.Trim())));
        }
        #endregion

        #region EditDeviceConfirmButton
        private async void EditDeviceConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditDeviceNameTextBox.Text))
            {
                MessageBox.Show("Es muss ein Name angegeben werden.", "Ungültiger Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //Check if device is currently borrowed somewhere, while trying to block
            if (EditDeviceBlockedCheckBox.IsChecked.Value)
            {
                if (DataHandler.IsDeviceCurrentlyBorrowed(SelectedDevice))
                {
                    MessageBox.Show("Das Gerät ist momentan ausgeliehen und kann daher nicht gesperrt werden.", "Gerät kann nicht gesperrt werden", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            int lastID = SelectedDevice.ID;

            DatabaseHandler.UpdateDevice(SelectedDevice.ID, EditDeviceNameTextBox.Text, new TextRange(EditDeviceNotesRichTextBox.Document.ContentStart, EditDeviceNotesRichTextBox.Document.ContentEnd).Text.Trim(), EditDeviceBlockedCheckBox.IsChecked.Value);
            LoadDevices();

            DevicesDataGrid.SelectedItem = LoadedDevices.Where(x => x.ID.Equals(lastID)).FirstOrDefault();

            MessageBox.Show("Die Änderungen wurden erfolgreich übernommen.", "Änderungen übernommen", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region FilterCheckBoxes
        private void FilterNotebookCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SelectedDeviceTypes.Add(DeviceType.Notebook);
            LoadDevices();
        }
        private void FilterNotebookCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectedDeviceTypes.Remove(DeviceType.Notebook);
            LoadDevices();
        }

        private void FilterGigaCubeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SelectedDeviceTypes.Add(DeviceType.GigaCube);
            LoadDevices();
        }
        private void FilterGigaCubeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectedDeviceTypes.Remove(DeviceType.GigaCube);
            LoadDevices();
        }

        private void FilterERKMeetingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SelectedDeviceTypes.Add(DeviceType.ERK_Meeting);
            LoadDevices();
        }
        private void FilterERKMeetingCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectedDeviceTypes.Remove(DeviceType.ERK_Meeting);
            LoadDevices();
        }

        private void FilterSpecialCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SelectedDeviceTypes.Add(DeviceType.Special);
            LoadDevices();
        }
        private void FilterSpecialCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectedDeviceTypes.Remove(DeviceType.Special);
            LoadDevices();
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
        /// Load the devices out of the database. Including the filters.
        /// </summary>
        private async void LoadDevices()
        {
            DataHandler.RefreshData();
            LoadedDevices = DataHandler.Devices.Where(d => SelectedDeviceTypes.Contains(d.Type));
            DevicesDataGrid.ItemsSource = LoadedDevices;
        }
        #endregion

        #endregion
    }
}
