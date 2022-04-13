using Iris.Database;
using Iris.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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
        private List<string> LoadedTakers { get; init; }
        #endregion

        #region Constructors
        /// <param name="borrowing">The borrowing which should be edited.</param>
        public EditBorrowingWindow(Borrowing borrowing)
        {
            Owner = Global.MainWindow;

            LoadedTakers = DataHandler.Loaners.Select(l => l.Name).ToList();

            InitializeComponent();
            Borrowing = borrowing;
        }
        #endregion

        #region Events
        #region ApplyButton
        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LenderNameTextBox.Text) || DeviceComboBox.SelectedIndex == -1 || FromDatePicker.SelectedDate is null || ToDatePicker.SelectedDate is null)
            {
                MessageBox.Show("Bitte alle Pflichtfelder ausfüllen!", "Ausleihe bearbeiten fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Das ausgewählte Gerät ist in dem angegebenem Zeitraum nicht verfügbar.", "Ausleihe bearbeiten fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime dateStart = FromDatePicker.SelectedDate.Value.Date;
            DateTime datePlannedEnd = ToDatePicker.SelectedDate.Value.Date;
            DateTime? dateEnd = EndDatePicker.SelectedDate;

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

            int offsetStartDate = timeZoneInfo.IsDaylightSavingTime(dateStart) ? 2 : 1;
            int offsetPlannedEndDate = timeZoneInfo.IsDaylightSavingTime(datePlannedEnd) ? 2 : 1;

            await DatabaseHandler.UpdateBorrowing(Borrowing.ID,
                Borrowing.DeviceID,
                string.IsNullOrWhiteSpace(Borrowing.Loaner) ? Global.NullDBString : Borrowing.Loaner,
                string.IsNullOrWhiteSpace(Borrowing.Taker) ? Global.NullDBString : Borrowing.Taker,
                LenderNameTextBox.Text,
                string.IsNullOrWhiteSpace(LenderPhoneTextBox.Text) ? Global.NullDBString : LenderPhoneTextBox.Text,
                string.IsNullOrWhiteSpace(LenderEMailTextBox.Text) ? Global.NullDBString : LenderEMailTextBox.Text,
                new DateTimeOffset(dateStart.AddHours(offsetStartDate)).ToUnixTimeSeconds(),
                new DateTimeOffset(datePlannedEnd.AddHours(offsetPlannedEndDate)).ToUnixTimeSeconds(),
                dateEnd is null ? -1 : new DateTimeOffset(new DateTime(dateEnd.Value.Year, dateEnd.Value.Month, dateEnd.Value.Day)).ToUnixTimeSeconds(),
                Borrowing.IsBorrowed,
                new TextRange(NotesRichTextBox.Document.ContentStart, NotesRichTextBox.Document.ContentEnd).Text.Trim());

            Borrowing = await DatabaseHandler.SelectBorrowing(Borrowing.ID);

            MessageBox.Show("Die Änderungen wurden erfolgreich übernommen.", "Ausleihe bearbeiten erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);

            ApplyButton.IsEnabled = false;
        }
        #endregion

        #region BorrowTakeButton
        private async void BorrowTakeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplyButton.IsEnabled)
            {
                string text = (Borrowing.IsBorrowed ? Borrowing.DateEndUnix == -1 ? "zurückgenommen" : "geschlossen" : "ausgeliehen");
                if (MessageBox.Show($"Es gibt ungespeicherte Änderungen. Soll die Ausleihe dennoch {text} werden?", "Ungespeicherte Änderungen", MessageBoxButton.YesNo, MessageBoxImage.Question).Equals(MessageBoxResult.No))
                {
                    return;
                }
            }

            if (Borrowing.IsBorrowed && Borrowing.DateEndUnix != -1) //Fishied
            {
                Close();
            }
            else if (Borrowing.IsBorrowed) //Take back
            {
                Borrowing.TakeBack();

                await DatabaseHandler.UpdateBorrowing(Borrowing.ID,
                    Borrowing.DeviceID,
                    Borrowing.Loaner,
                    TakerComboBox.Text,
                    Borrowing.LenderName,
                    Borrowing.LenderPhone,
                    Borrowing.LenderEmail,
                    Borrowing.DateStartUnix,
                    Borrowing.DatePlannedEndUnix,
                    Borrowing.DateEndUnix,
                    Borrowing.IsBorrowed,
                    new TextRange(NotesRichTextBox.Document.ContentStart, NotesRichTextBox.Document.ContentEnd).Text.Trim());
            }
            else //Loan
            {
                if (Borrowing.Device.IsBlocked)
                {
                    MessageBox.Show($"Das Gerät '{Borrowing.Device.Name}' kann nicht augeliehen werden, da es zurzeit gesperrt ist.", "Ausleihen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Borrowing.Borrow();

                await DatabaseHandler.UpdateBorrowing(Borrowing.ID,
                    Borrowing.DeviceID,
                    LoanerComboBox.Text,
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
            if (!(FromDatePicker.SelectedDate.Equals(Borrowing.DateStart)) || !(ToDatePicker.SelectedDate.Equals(Borrowing.DatePlannedEnd)))
            {
                ApplyButton.IsEnabled = true;
            }
            else if (FromDatePicker.SelectedDate.Equals(Borrowing.DateStart) || ToDatePicker.SelectedDate.Equals(Borrowing.DatePlannedEnd))
            {
                ApplyButton.IsEnabled = false;
            }

            if (FromDatePicker.SelectedDate is not null && ToDatePicker.SelectedDate is not null)
            {
                if (ToDatePicker.SelectedDate.Value < FromDatePicker.SelectedDate.Value)
                {
                    DeviceAvailabilityTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    DeviceAvailabilityTextBlock.Text = "Ungültige Zeitspanne";
                    IsDeviceAvailable = false;
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
            EndDatePicker.SelectedDate = Borrowing.DateEndUnix == -1 ? null : Borrowing.DateEnd;

            if (!Borrowing.IsBorrowed)
            {
                TakerComboBox.IsEnabled = false;
                LoanerComboBox.IsEnabled = true;

                if (!LoadedTakers.Contains(Global.CurrentUser))
                {
                    LoadedTakers.Add(Global.CurrentUser);
                }
                LoanerComboBox.ItemsSource = LoadedTakers;
                LoanerComboBox.SelectedItem = Global.CurrentUser;
            }
            else
            {
                LoanerComboBox.Items.Add(Borrowing.Loaner);
                LoanerComboBox.SelectedIndex = 0;
            }

            if (Borrowing.IsBorrowed && Borrowing.DateEndUnix == -1)
            {
                LenderNameTextBox.IsEnabled = false;
                FromDatePicker.IsEnabled = false;
                TakerComboBox.IsEnabled = true;

                //Set taker
                if (!LoadedTakers.Contains(Global.CurrentUser))
                {
                    LoadedTakers.Add(Global.CurrentUser);
                }
                TakerComboBox.ItemsSource = LoadedTakers;
                TakerComboBox.SelectedItem = Global.CurrentUser;

                if (Borrowing.DatePlannedEnd < DateTime.Now.Date)
                {
                    SendEmailButton.Visibility = Visibility.Visible;
                }
            }
            else if (Borrowing.IsBorrowed && Borrowing.DateEndUnix != -1)
            {
                LenderNameTextBox.IsEnabled = false;
                LenderEMailTextBox.IsEnabled = false;
                LenderPhoneTextBox.IsEnabled = false;
                FromDatePicker.IsEnabled = false;
                ToDatePicker.IsEnabled = false;
                NotesRichTextBox.IsEnabled = false;
                TakerComboBox.IsEnabled = false;
                ApplyButton.IsEnabled = false;
                DeviceAvailabilityTextBlock.Visibility = Visibility.Hidden;

                //Set taker
                TakerComboBox.Items.Add(Borrowing.Taker);
                TakerComboBox.SelectedIndex = 0;
            }

            BorrowTakeTextBlock.Text = (Borrowing.IsBorrowed ? Borrowing.DateEndUnix == -1 ? "Zurücknehmen" : "Schließen" : "Ausleihen");

            LenderNameTextBlock.Foreground = LenderNameTextBox.IsEnabled ? Global.MaterialDesignDarkForeground : Global.MaterialDesignLightSeparatorBackground;
            DeviceTextBlock.Foreground = DeviceComboBox.IsEnabled ? Global.MaterialDesignDarkForeground : Global.MaterialDesignLightSeparatorBackground;
            LenderEMailTextBlock.Foreground = LenderEMailTextBox.IsEnabled ? Global.MaterialDesignDarkForeground : Global.MaterialDesignLightSeparatorBackground;
            LenderPhoneTextBlock.Foreground = LenderPhoneTextBox.IsEnabled ? Global.MaterialDesignDarkForeground : Global.MaterialDesignLightSeparatorBackground;
            FromTextBlock.Foreground = FromDatePicker.IsEnabled ? Global.MaterialDesignDarkForeground : Global.MaterialDesignLightSeparatorBackground;
            ToTextBlock.Foreground = ToDatePicker.IsEnabled ? Global.MaterialDesignDarkForeground : Global.MaterialDesignLightSeparatorBackground;
            NotesTextBlock.Foreground = NotesRichTextBox.IsEnabled ? Global.MaterialDesignDarkForeground : Global.MaterialDesignLightSeparatorBackground;
            LoanerTextBlock.Foreground = LoanerComboBox.IsEnabled ? Global.MaterialDesignDarkForeground : Global.MaterialDesignLightSeparatorBackground;
            TakerTextBlock.Foreground = TakerComboBox.IsEnabled ? Global.MaterialDesignDarkForeground : Global.MaterialDesignLightSeparatorBackground;
        }
        #endregion

        #region SendMailButton
        private async void SendEmailButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Borrowing.LenderEmail))
            {
                MessageBox.Show("Es ist keine E-Mail Adresse angegeben.", "Fehlende E-Mail Adresse", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Borrowing.LastMailSent.HasValue && (DateTime.Now - Borrowing.LastMailSent).Value.TotalMinutes <= 5)
            {
                MessageBox.Show("Es wurde bereits eine Erinnerungs E-Mail innerhalb der letzten 5 Minuten an den Ausleiher gesendet.", "E-Mail Spam", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            using SmtpClient smtp = new();
            using MailMessage mail = new();

            smtp.Host = "172.28.0.43"; // TODO: Das könnte man noch dynamisch machen -> Guter Ausblick
            smtp.Port = 25;
            smtp.Credentials = CredentialCache.DefaultNetworkCredentials;
            smtp.Timeout = 5000;

            mail.From = new MailAddress("edv.iris@en-kreis.de"); // TODO: Das könnte man noch dynamisch machen -> Guter Ausblick
            mail.Subject = $"Erinnerung an Rückgabe von Verleihgerät ({Borrowing.Device.Name})";
            mail.Body = $"Guten Tag,\n\n" +
                $"bitte denken Sie daran, das ausgeliehene Gerät ({Borrowing.Device.Name}) zurück zu bringen.\n" +
                $"Ihr geplanter Ausleizeitraum: {Borrowing.DateStart.ToLongDateString()} bis {Borrowing.DatePlannedEnd.ToLongDateString()}.\n\n" +
                $"Mit freundlichen Grüßen\n" +
                $"Ihre EDV-Abteilung\n\n\n" +
                $"Diese E-Mail-Adresse ist nicht für den Empfang von Nachrichten vorgesehen!\n" +
                $"Bitte antworten Sie deshalb nicht auf diese E-Mail, da ihre Nachricht nicht gelesen oder weitergeleitet wird."; // TODO: Wild wäre hier noch einen Custom Mail Editor als extra tab oder so vllt Einstellungen (würde auch für smtp usw passen) -> Email Templates

            // TODO: Email für Reservierungsbestätigung

            // TODO: Hier könnte man noch die Email des Email senders rein packen...
            //mail.CC.Add(new MailAddress());
            mail.To.Add(new MailAddress(Borrowing.LenderEmail));

            await Task.Run(() =>
            {
                try
                {
                    smtp.Send(mail);

                    Borrowing.LastMailSent = DateTime.Now;
                }
                catch (SmtpException e)
                {
                    MessageBox.Show("Die E-Mail konnte nicht gesendet werden.", "E-Mail senden fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            });
        }
        #endregion

        #region NotesRichTextBox
        private void NotesRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string notes = new TextRange(NotesRichTextBox.Document.ContentStart, NotesRichTextBox.Document.ContentEnd).Text.Trim();
            if (!string.IsNullOrWhiteSpace(notes) && !notes.Equals(Borrowing.Notes))
            {
                ApplyButton.IsEnabled = true;
            }
            else
            {
                ApplyButton.IsEnabled = false;
            }
        }
        #endregion

        #region LenderEMailTextBox
        private void LenderEMailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string email = LenderEMailTextBox.Text;
            if (!string.IsNullOrWhiteSpace(email) && !email.Equals(Borrowing.LenderEmail))
            {
                ApplyButton.IsEnabled = true;
            }
            else
            {
                ApplyButton.IsEnabled = false;
            }
        }
        #endregion

        #region LenderPhoneTextBox
        private void LenderPhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string phone = LenderPhoneTextBox.Text;
            if (!string.IsNullOrWhiteSpace(phone) && !phone.Equals(Borrowing.LenderPhone))
            {
                ApplyButton.IsEnabled = true;
            }
            else
            {
                ApplyButton.IsEnabled = false;
            }
        }
        #endregion
        #endregion
    }
}
