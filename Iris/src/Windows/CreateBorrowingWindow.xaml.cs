using Iris.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
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

        private DateTime? lastSelectedToDate;

        private List<MultiBorrowingTimeSpan> multiBorrowingTimeSpans = new();
        #endregion

        #region Constructors
        public CreateBorrowingWindow()
        {
            Owner = Global.MainWindow;
            LoadedLoaners = DataHandler.Loaners!.Select(l => l.Name).ToList();

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
            if (string.IsNullOrWhiteSpace(LenderNameTextBox.Text) || DeviceComboBox.SelectedIndex == -1 || (!MultipleBorrowCheckBox.IsChecked!.Value && (FromDatePicker.SelectedDate is null || ToDatePicker.SelectedDate is null)))
            {
                MessageBox.Show("Bitte alle Pflichtfelder ausfüllen!", "Ausleihe erstellen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (MultipleBorrowCheckBox.IsChecked.Value)
            {
                if (MultipleBorrowTimeSpansDataGrid.Items.Count == 0)
                {
                    MessageBox.Show("Es muss mindestens ein Zeitraum angegeben werden!", "Ausleihe erstellen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
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

            FromToDatePicker_SelectedDateChanged(sender, null);

            string notes = new TextRange(NotesRichTextBox.Document.ContentStart, NotesRichTextBox.Document.ContentEnd).Text.Trim();

            if (MultipleBorrowCheckBox.IsChecked.Value)
            {
                List<Borrowing> borrowings = new();

                foreach (MultiBorrowingTimeSpan timeSpan in MultipleBorrowTimeSpansDataGrid.Items)
                {
                    borrowings.Add((await Borrowing.CreateNewBorrowing((DeviceComboBox.SelectedItem as Device)!.ID,
                                   string.IsNullOrWhiteSpace(LoanerComboBox.Text) ? null : LoanerComboBox.Text,
                                   LenderNameTextBox.Text,
                                   string.IsNullOrWhiteSpace(LenderPhoneTextBox.Text) ? null : LenderPhoneTextBox.Text,
                                   string.IsNullOrWhiteSpace(LenderEMailTextBox.Text) ? null : LenderEMailTextBox.Text,
                                   timeSpan.Start,
                                   timeSpan.End,
                                   false,
                                   string.IsNullOrWhiteSpace(notes) ? null : notes))!);
                }

                Global.CopyMultiBorrowingEMailString(borrowings);
            }
            else
            {
                Borrowing? borrowing = await Borrowing.CreateNewBorrowing((DeviceComboBox.SelectedItem as Device)!.ID,
                                                   string.IsNullOrWhiteSpace(LoanerComboBox.Text) ? null : LoanerComboBox.Text,
                                                   LenderNameTextBox.Text,
                                                   string.IsNullOrWhiteSpace(LenderPhoneTextBox.Text) ? null : LenderPhoneTextBox.Text,
                                                   string.IsNullOrWhiteSpace(LenderEMailTextBox.Text) ? null : LenderEMailTextBox.Text,
                                                   FromDatePicker.SelectedDate!.Value,
                                                   ToDatePicker.SelectedDate!.Value,
                                                   InstantBorrowCheckBox.IsChecked!.Value,
                                                   string.IsNullOrWhiteSpace(notes) ? null : notes);

                Global.CopyBorrowingEMailString(borrowing);
            }

            DataHandler.LoadDataFromDatabase(devices: false, loaners: false);

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
        private void FromToDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs? e)
        {
            DatePicker dt = (sender as DatePicker)!;
            if (dt is not null && dt.Equals(FromDatePicker) && FromDatePicker.SelectedDate is not null)
            {
                ToDatePicker.SelectedDate = FromDatePicker.SelectedDate;
            }

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
                    IsDeviceAvailable = DataHandler.IsDeviceAvailable((DeviceComboBox.SelectedItem as Device)!, FromDatePicker.SelectedDate.Value, ToDatePicker.SelectedDate.Value);

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
            DataHandler.ReleaseTemporaryBlocks();
            MultipleBorrowTimeSpansDataGrid.Items.Clear();

            //TODO: Rückgabedatum  // dauerleihgaben markieren

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

        #region PermanentBorrowCheckBox
        private void PermanentBorrowCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MultipleBorrowCheckBox.IsEnabled = true;

            ToDatePicker.IsEnabled = true;

            ToDatePicker.SelectedDate = lastSelectedToDate;
        }

        private void PermanentBorrowCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MultipleBorrowCheckBox.IsEnabled = false;
            MultipleBorrowCheckBox.IsChecked = false;

            lastSelectedToDate = ToDatePicker.SelectedDate;
            
            ToDatePicker.IsEnabled = false;
            ToDatePicker.SelectedDate = new DateTime(DataHandler.permanentBorrowingYear, 1, 1);
        }
        #endregion

        #region MultipleBorrowCheckBox
        private void MultipleBorrowCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PermanentBorrowCheckBox.IsEnabled = true;
            InstantBorrowCheckBox.IsEnabled = true;

            MultipleBorrowingGrid.Visibility = Visibility.Collapsed;

            FromDateTextBlock.Text = "Von:*";
            ToDateTextBlock.Text = "Bis:*";
        }

        private void MultipleBorrowCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PermanentBorrowCheckBox.IsEnabled = false;
            PermanentBorrowCheckBox.IsChecked = false;
            InstantBorrowCheckBox.IsEnabled = false;
            InstantBorrowCheckBox.IsChecked = false;

            MultipleBorrowingGrid.Visibility = Visibility.Visible;

            FromDateTextBlock.Text = "Von:";
            ToDateTextBlock.Text = "Bis:";
        }
        #endregion

        #region AddRemoveBorrowingTimeSpan Buttons
        private void AddBorrowingTimeSpanButton_Click(object sender, RoutedEventArgs e)
        {
            if (FromDatePicker.SelectedDate is not null && ToDatePicker.SelectedDate is not null)
            {
                if (!IsDeviceAvailable)
                {
                    MessageBox.Show("Das ausgewählte Gerät ist in dem angegebenem Zeitraum nicht verfügbar.", "Zeitraum hinzufügen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MultiBorrowingTimeSpan ts = new(FromDatePicker.SelectedDate.Value, ToDatePicker.SelectedDate.Value);
                MultipleBorrowTimeSpansDataGrid.Items.Add(ts);
                DataHandler.TemporaryBlock((DeviceComboBox.SelectedItem as Device)!, ts);

                FromDatePicker.SelectedDate = null;
                ToDatePicker.SelectedDate = null;
            }
        }

        private void RemoveBorrowingTimeSpanButton_Click(object sender, RoutedEventArgs e)
        {
            MultiBorrowingTimeSpan item = (MultipleBorrowTimeSpansDataGrid.SelectedItem as MultiBorrowingTimeSpan)!;

            if (item is not null)
            {
                MultipleBorrowTimeSpansDataGrid.Items.Remove(item);
                DataHandler.ReleaseTemporaryBlock((DeviceComboBox.SelectedItem as Device)!, item);
            }
        }
        #endregion

        #region LenderNameTextBox
        private void LenderNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string[] vs = LenderNameTextBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (vs.Length > 1)
            {
                LenderEMailTextBox.Text = $"{vs[0]}.{vs[1]}@en-kreis.de".ToLower();
            }
            else
            {
                LenderEMailTextBox.Text = "";
            }
        }
        #endregion
        #endregion
    }

    public class MultiBorrowingTimeSpan
    {
        #region Properties
        public DateTime Start { get; init; }
        public DateTime End { get; init; }

        public string TimeSpanString => $"{Start.Date:d}-{End.Date:d}";
        #endregion

        #region Constructors
        public MultiBorrowingTimeSpan(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
        #endregion
    }
}
