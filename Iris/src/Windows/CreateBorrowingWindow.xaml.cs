using Iris.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Iris.src.Windows
{
    /// <summary>
    /// Interaktionslogik für CreateBorrowingWindow.xaml
    /// </summary>
    public partial class CreateBorrowingWindow : Window
    {
        #region Properties and Variables
        /// <summary>
        /// Whether the currently selected device is available in the selected timespan.
        /// </summary>
        private bool IsDeviceAvailable { get; set; } = false;

        private IEnumerable<Device> LoadedDevices => DataHandler.AvailableDevices;
        private List<string> LoadedLoaners { get; init; }
        #endregion

        #region Constructors
        public CreateBorrowingWindow()
        {
            Owner = Global.MainWindow;
            LoadedLoaners = DataHandler.Loaners.Select(l => l.Name).ToList();

            InitializeComponent();
        }

        public CreateBorrowingWindow(Device preSelectedDevice, DateTime preSelectedStartDate, DateTime preSelectedEndDate) : this()
        {
            DeviceComboBox.SelectedItem = preSelectedDevice;
            FromDatePicker.SelectedDate = preSelectedStartDate;
            ToDatePicker.SelectedDate = preSelectedEndDate;
        }
        #endregion

        #region Events
        #region AddBorrowingConfirmButton
        private async void AddBorrowingConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LenderNameTextBox.Text) || DeviceComboBox.SelectedIndex == -1 || FromDatePicker.SelectedDate is null || ToDatePicker.SelectedDate is null)
            {
                MessageBox.Show("Bitte alle Pflichtfelder ausfüllen!", "Ausleihe erstellen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!string.IsNullOrWhiteSpace(LenderEMailTextBox.Text))
            {
                try
                {
                    MailAddress mail = new(LenderEMailTextBox.Text);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Die angegebene E-Mail Adresse ist ungültig.", "Ungültige E-Mail Adresse", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            if (!IsDeviceAvailable)
            {
                MessageBox.Show("Das ausgewählte Gerät ist in dem angegebenem Zeitraum nicht verfügbar.", "Ausleihe erstellen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FromToDatePicker_SelectedDateChanged(null, null);

            string notes = new TextRange(NotesRichTextBox.Document.ContentStart, NotesRichTextBox.Document.ContentEnd).Text.Trim();

            Borrowing borrowing = await Borrowing.CreateNewBorrowing((DeviceComboBox.SelectedItem as Device).ID,
                                               string.IsNullOrWhiteSpace(LoanerComboBox.Text) ? null : LoanerComboBox.Text,
                                               LenderNameTextBox.Text,
                                               string.IsNullOrWhiteSpace(LenderPhoneTextBox.Text) ? null : LenderPhoneTextBox.Text,
                                               string.IsNullOrWhiteSpace(LenderEMailTextBox.Text) ? null : LenderEMailTextBox.Text,
                                               FromDatePicker.SelectedDate.Value,
                                               ToDatePicker.SelectedDate.Value,
                                               InstantBorrowCheckBox.IsChecked.Value,
                                               string.IsNullOrWhiteSpace(notes) ? null : notes);

            DataHandler.RefreshData();

            Global.CopyBorrowingEMailString(borrowing);

            Close();
        }
        #endregion

        #region AddBorrowingCancelButton
        private void AddBorrowingCancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(string.IsNullOrWhiteSpace(LenderNameTextBox.Text) && DeviceComboBox.SelectedIndex == -1 && string.IsNullOrWhiteSpace(LenderEMailTextBox.Text) && string.IsNullOrWhiteSpace(LenderPhoneTextBox.Text) && FromDatePicker.SelectedDate is null && ToDatePicker.SelectedDate is null && string.IsNullOrWhiteSpace(new TextRange(NotesRichTextBox.Document.ContentStart, NotesRichTextBox.Document.ContentEnd).Text)))
            {
                if (MessageBox.Show("Soll die Erstellung dieser Ausleihe wirklich abgebrochen werden?", "Erstellung von Ausleihe abbrechen", MessageBoxButton.YesNo, MessageBoxImage.Question).Equals(MessageBoxResult.Yes))
                {
                    this.Close();
                }
                else
                {
                    return;
                }
            }

            this.Close();
        }
        #endregion

        #region FromToDatePicker
        private void FromToDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FromDatePicker.SelectedDate is not null && ToDatePicker.SelectedDate is not null)
            {
                if (ToDatePicker.SelectedDate.Value < FromDatePicker.SelectedDate.Value)
                {
                    DeviceAvailabilityTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    DeviceAvailabilityTextBlock.Text = "Ungültige Zeitspanne";
                    return;
                }
                else
                {
                    DeviceAvailabilityTextBlock.Text = "";
                }

                if (DeviceComboBox.SelectedIndex != -1)
                {
                    IsDeviceAvailable = DataHandler.IsDeviceAvailable(DeviceComboBox.SelectedItem as Device, FromDatePicker.SelectedDate.Value, ToDatePicker.SelectedDate.Value);

                    if (IsDeviceAvailable)
                    {
                        DeviceAvailabilityTextBlock.Foreground = new SolidColorBrush(Colors.ForestGreen);
                        DeviceAvailabilityTextBlock.Text = "Verfügbar";
                    }
                    else
                    {
                        DeviceAvailabilityTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                        DeviceAvailabilityTextBlock.Text = "Nicht Verfügbar";
                    }

                    return;
                }
            }
            else
            {
                DeviceAvailabilityTextBlock.Text = "";
            }
        }
        #endregion

        #region InstantBorrowCheckBox
        private void InstantBorrowCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!LoadedLoaners.Contains(Global.CurrentUser))
            {
                LoadedLoaners.Add(Global.CurrentUser);
            }
            LoanerComboBox.SelectedItem = Global.CurrentUser;
            LoanerComboBox.ItemsSource = LoadedLoaners;
            LoanerComboBox.IsEnabled = true;
            LoanerTextBlock.Text = "Herausgeber:*";
            LoanerTextBlock.Foreground = Global.MaterialDesignDarkForeground;

            AddBorrowingConfirmTextBlock.Text = "Ausleihen";
        }

        private void InstantBorrowCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            LoadedLoaners.Remove(Global.CurrentUser);
            LoanerComboBox.SelectedIndex = -1;
            LoanerComboBox.ItemsSource = LoadedLoaners;
            LoanerComboBox.IsEnabled = false;
            LoanerTextBlock.Text = "Herausgeber:";
            LoanerTextBlock.Foreground = Global.MaterialDesignLightSeparatorBackground;

            AddBorrowingConfirmTextBlock.Text = "Reservieren";
        }
        #endregion

        #region DeviceComboBox
        private void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FromToDatePicker_SelectedDateChanged(sender, e);
        }
        #endregion

        #region Window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DeviceComboBox.ItemsSource = LoadedDevices;
            LoanerComboBox.ItemsSource = LoadedLoaners;
        }
        #endregion

        #endregion
    }
}
