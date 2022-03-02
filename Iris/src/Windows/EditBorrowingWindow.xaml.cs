using Iris.Structures;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Iris.src.Windows
{
    /// <summary>
    /// Interaktionslogik für EditBorrowingWindow.xaml
    /// </summary>
    public partial class EditBorrowingWindow : Window
    {
        #region Properties and Variables
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

        }
        #endregion

        #region BorrowTakeButton
        private void BorrowTakeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Borrowing.IsBorrowed && Borrowing.DateEnd is not null)
            {
                Close();
            }
            else if (Borrowing.IsBorrowed)
            {
                // Zurücknehmen

            }
            else
            {
                // Herausgeben

            }
        }
        #endregion

        #region FromToDatePicker
        private void FromToDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        #endregion

        #region Window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LenderNameTextBox.Text = Borrowing.LenderName;
            DeviceComboBox.SelectedItem = Borrowing.Device;
            LenderEMailTextBox.Text = Borrowing.LenderEmail;
            LenderPhoneTextBox.Text = Borrowing.LenderPhone;
            FromDatePicker.SelectedDate = Borrowing.DateStart;
            ToDatePicker.SelectedDate = Borrowing.DatePlannedEnd;
            NotesRichTextBox.Document.Blocks.Add(new Paragraph(new Run(Borrowing.Notes)));
            LoanerTextBox.Text = Borrowing.Loaner;
            TakerTextBox.Text = Borrowing.Taker;
            EndDatePicker.SelectedDate = Borrowing.DateEnd;
            
            if (Borrowing.IsBorrowed)
            {
                LenderNameTextBox.IsEnabled = false;
                FromDatePicker.IsEnabled = false;
            }
            else if (Borrowing.IsBorrowed && Borrowing.DateEnd is not null)
            {
                LenderNameTextBox.IsEnabled = false;
                LenderEMailTextBox.IsEnabled = false;
                LenderPhoneTextBox.IsEnabled = false;
                FromDatePicker.IsEnabled = false;
                ToDatePicker.IsEnabled = false;
                NotesRichTextBox.IsEnabled = false;
            }

            BorrowTakeTextBlock.Text = (Borrowing.IsBorrowed ? Borrowing.DateEnd is not null ? "Schließen" : "Zurücknehmen" : "Ausleihen");
        }
        #endregion
        #endregion
    }
}
