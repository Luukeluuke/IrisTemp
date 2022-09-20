using Iris.Database;
using Iris.src.Windows;
using Iris.Structures;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Iris.src.Views
{
    /// <summary>
    /// Interaktionslogik für Borrowings.xaml
    /// </summary>
    public partial class Borrowings : UserControl
    {
        #region Properties and Variables
        /// <summary>
        /// The ItemSource for <see cref="BorrowingsDataGrid"/>.
        /// </summary>
        private IEnumerable<Borrowing>? LoadedBorrowings { get; set; }
        /// <summary>
        /// The currently selected borrowing in <see cref="BorrowingsDataGrid"/>.
        /// </summary>
        private Borrowing? SelectedBorrowing { get; set; }
        #endregion

        #region Constructors
        public Borrowings()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        #region AddBorrowingButton
        private void AddBorrowingButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            new CreateBorrowingWindow().ShowDialog();

            LoadBorrowings();
        }
        #endregion

        #region UserControl
        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadBorrowings();
        }
        #endregion

        #region RemoveFiltersButton
        private void RemoveFiltersButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            FilterFromDatePicker.SelectedDate = null;
            FilterToDatePicker.SelectedDate = null;
            FilterDeviceComboBox.SelectedIndex = -1;
            FilterContainsTextBox.Text = "";

            LoadBorrowings();
        }
        #endregion

        #region ApplyFiltersButton
        private void ApplyFiltersButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadBorrowings();
        }
        #endregion

        #region DeleteBorrowingsButton
        private void DeleteBorrowingButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (SelectedBorrowing is null)
            {
                MessageBox.Show("Es ist keine Ausleihe ausgewählt.", "Keine Ausleihe ausgewählt", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Soll die Ausleihe: \"{SelectedBorrowing.LenderName}, {SelectedBorrowing.Device.Name}, {SelectedBorrowing.DateStart:yyyy.dd.MM} - {SelectedBorrowing.DatePlannedEnd:yyyy.dd.MM}\" wirklich gelöscht werden?", $"{SelectedBorrowing.LenderName}, {SelectedBorrowing.Device.Name}, {SelectedBorrowing.DateStart:yyyy.dd.MM} - {SelectedBorrowing.DatePlannedEnd:yyyy.dd.MM} löschen", MessageBoxButton.YesNo, MessageBoxImage.Question).Equals(MessageBoxResult.Yes))
            {
                DatabaseHandler.DeleteBorrowing(SelectedBorrowing.ID);
                LoadBorrowings();
            }
        }
        #endregion

        #region BorrowingsDataGrid
        private void BorrowingsDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SelectedBorrowing = (BorrowingsDataGrid.SelectedItem as Borrowing)!;

            DeleteBorrowingButton.IsEnabled = SelectedBorrowing is not null;
            EditBorrowingButton.IsEnabled = SelectedBorrowing is not null;
        }

        private void BorrowingsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new EditBorrowingWindow(SelectedBorrowing!).ShowDialog();

            LoadBorrowings();
        }
        #endregion

        #region RefreshBorrowingsButton
        private void RefreshBorrowingsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBorrowings();
        }
        #endregion

        #region EditBorrowingButton
        private void EditBorrowingButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBorrowing is null)
            {
                MessageBox.Show("Es ist keine Ausleihe ausgewählt.", "Keine Ausleihe ausgewählt", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            new EditBorrowingWindow(SelectedBorrowing).ShowDialog();

            LoadBorrowings();
        }
        #endregion

        #region OnlyPermenentBorrowingsCheckBox
        private void OnlyPermanentBorrowingsCheckBox_Check(object sender, RoutedEventArgs e)
        {
            ApplyFiltersButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        #endregion

        #region DataGrid
        private void BorrowingsDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;
            if (column.Header.Equals("Ausleihdatum") || column.Header.Equals("Geplante Rückgabe") || column.Header.Equals("Zurückgabedatum"))
            {
                e.Handled = true;

                IComparer<DateTime>

            }
        }
        #endregion
        #endregion

        #region Methods
        #region Private
        /// <summary>
        /// Load the borrowings out of the database. Including the filters.
        /// </summary>
        private void LoadBorrowings()
        {
            DataHandler.LoadDataFromDatabase();

            LoadedBorrowings = OnlyPermanentBorrowingsCheckBox.IsChecked!.Value ? DataHandler.GetPermenentBorrowings() : DataHandler.Borrowings!;

            #region Filter
            if (FilterDeviceComboBox.SelectedIndex != -1)
            {
                LoadedBorrowings = LoadedBorrowings.Where(b => b.Device.Type.Equals((DeviceType)(FilterDeviceComboBox.SelectedIndex + 1)));
            }
            if (FilterFromDatePicker.SelectedDate is not null)
            {
                LoadedBorrowings = LoadedBorrowings.Where(b => b.DateStart >= FilterFromDatePicker.SelectedDate.Value);
            }
            if (FilterToDatePicker.SelectedDate is not null)
            {
                LoadedBorrowings = LoadedBorrowings.Where(b => b.DatePlannedEnd <= FilterToDatePicker.SelectedDate.Value);
            }
            if (!FilterContainsTextBox.Text.Equals(""))
            {
                LoadedBorrowings = LoadedBorrowings.Where(b => b.Contains(FilterContainsTextBox.Text));
            }
            #endregion

            BorrowingsDataGrid.ItemsSource = LoadedBorrowings;
        }
        #endregion

        #endregion
    }
}
