using Iris.Database;
using Iris.Structures;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Iris.src.Windows
{
    /// <summary>
    /// Interaktionslogik für EditBorrowingWindow.xaml
    /// </summary>
    public partial class EditBorrowingWindow : Window
    {
        #region Properties and Variables
        /// <summary>
        /// Whether the currently selected device is available in the selected timespan.
        /// </summary>
        private bool IsDeviceAvailable { get; set; } = true;

        private Borrowing Borrowing { get; set; }
        #endregion

        #region Constructors
        /// <param name="borrowing">The borrowing which should be edited.</param>
        public EditBorrowingWindow(Borrowing borrowing)
        {
            InitializeComponent();

            Borrowing = borrowing;
        }
        #endregion

        #region Events
        #region ApplyButton
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LenderNameTextBox.Text) || DeviceComboBox.SelectedIndex == -1 || FromDatePicker.SelectedDate is null || ToDatePicker.SelectedDate is null)
            {
                MessageBox.Show("Bitte alle Pflichtfelder ausfüllen!", "Ausleihe bearbeiten fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!IsDeviceAvailable)
            {
                MessageBox.Show("Das ausgewählte Gerät ist in dem angegebenem Zeitraum nicht verfügbar.", "Ausleihe bearbeiten fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime dateStart = FromDatePicker.SelectedDate.Value;
            DateTime datePlannedEnd = ToDatePicker.SelectedDate.Value;
            DateTime? dateEnd = EndDatePicker.SelectedDate;

            DatabaseHandler.UpdateBorrowing(Borrowing.ID,
                Borrowing.DeviceID,
                string.IsNullOrWhiteSpace(LoanerTextBox.Text) ? Global.NullDBString : LoanerTextBox.Text,
                string.IsNullOrWhiteSpace(TakerTextBox.Text) ? Global.NullDBString : TakerTextBox.Text,
                LenderNameTextBox.Text,
                string.IsNullOrWhiteSpace(LenderPhoneTextBox.Text) ? Global.NullDBString : LenderPhoneTextBox.Text,
                string.IsNullOrWhiteSpace(LenderEMailTextBox.Text) ? Global.NullDBString : LenderEMailTextBox.Text,
                new DateTimeOffset(new DateTime(dateStart.Year, dateStart.Month, dateStart.Day)).ToUnixTimeSeconds(),
                new DateTimeOffset(new DateTime(datePlannedEnd.Year, datePlannedEnd.Month, datePlannedEnd.Day)).ToUnixTimeSeconds(),
                dateEnd is null ? -1 : new DateTimeOffset(new DateTime(dateEnd.Value.Year, dateEnd.Value.Month, dateEnd.Value.Day)).ToUnixTimeSeconds(),
                Borrowing.IsBorrowed,
                new TextRange(NotesRichTextBox.Document.ContentStart, NotesRichTextBox.Document.ContentEnd).Text.Trim());

            MessageBox.Show("Die Änderungen wurden erfolgreich übernommen.", "Ausleihe bearbeiten erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region BorrowTakeButton
        private void BorrowTakeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Borrowing.IsBorrowed && Borrowing.DateEndUnix != -1)
            {
                Close();
            }
            else if (Borrowing.IsBorrowed)
            {
                Borrowing.TakeBack();

                DatabaseHandler.UpdateBorrowing(Borrowing.ID,
                    Borrowing.DeviceID,
                    Borrowing.Loaner,
                    Global.CurrentUser,
                    Borrowing.LenderName,
                    Borrowing.LenderPhone,
                    Borrowing.LenderEmail,
                    Borrowing.DateStartUnix,
                    Borrowing.DatePlannedEndUnix,
                    Borrowing.DateEndUnix,
                    Borrowing.IsBorrowed,
                    new TextRange(NotesRichTextBox.Document.ContentStart, NotesRichTextBox.Document.ContentEnd).Text.Trim());
            }
            else
            {
                Borrowing.Borrow();

                DateTime dateStart = FromDatePicker.SelectedDate.Value;
                DateTime datePlannedEnd = ToDatePicker.SelectedDate.Value;
                DateTime? dateEnd = EndDatePicker.SelectedDate;

                DatabaseHandler.UpdateBorrowing(Borrowing.ID,
                    Borrowing.DeviceID,
                    Global.CurrentUser,
                    Global.NullDBString,
                    Borrowing.LenderName,
                    Borrowing.LenderPhone,
                    Borrowing.LenderEmail,
                    Borrowing.DateStartUnix,
                    Borrowing.DatePlannedEndUnix,
                    -1,
                    Borrowing.IsBorrowed,
                    Borrowing.Notes);
            }

            Close();
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
                    IsDeviceAvailable = DataHandler.IsDeviceAvailable(DeviceComboBox.SelectedItem as Device, 
                        FromDatePicker.SelectedDate.Value, 
                        ToDatePicker.SelectedDate.Value,
                        Borrowing.ID);

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

        #region Window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Borrowing is null)
            {
                Close();
                return;
            }

            LenderNameTextBox.Text = Borrowing.LenderName;
            DeviceComboBox.Items.Add(Borrowing.Device);
            DeviceComboBox.SelectedIndex = 0;
            LenderEMailTextBox.Text = Borrowing.LenderEmail;
            LenderPhoneTextBox.Text = Borrowing.LenderPhone;
            FromDatePicker.SelectedDate = Borrowing.DateStart;
            ToDatePicker.SelectedDate = Borrowing.DatePlannedEnd;
            NotesRichTextBox.Document.Blocks.Clear();
            NotesRichTextBox.Document.Blocks.Add(new Paragraph(new Run(Borrowing.Notes)));
            LoanerTextBox.Text = Borrowing.Loaner;
            TakerTextBox.Text = Borrowing.Taker;
            EndDatePicker.SelectedDate = Borrowing.DateEndUnix == -1 ? null : Borrowing.DateEnd;
            
            if (Borrowing.IsBorrowed && Borrowing.DateEndUnix == -1)
            {
                LenderNameTextBox.IsEnabled = false;
                FromDatePicker.IsEnabled = false;
            }
            else if (Borrowing.IsBorrowed && Borrowing.DateEndUnix != -1)
            {
                LenderNameTextBox.IsEnabled = false;
                LenderEMailTextBox.IsEnabled = false;
                LenderPhoneTextBox.IsEnabled = false;
                FromDatePicker.IsEnabled = false;
                ToDatePicker.IsEnabled = false;
                NotesRichTextBox.IsEnabled = false;
                ApplyButton.IsEnabled = false;
            }

            BorrowTakeTextBlock.Text = (Borrowing.IsBorrowed ? Borrowing.DateEndUnix == -1 ? "Zurücknehmen" : "Schließen" : "Ausleihen");
        }
        #endregion
        #endregion
    }
}
