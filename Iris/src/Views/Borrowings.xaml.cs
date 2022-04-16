using Iris.Database;
using Iris.src.Windows;
using Iris.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        private List<Borrowing> LoadedBorrowings { get; set; }
        /// <summary>
        /// The currently selected borrowing in <see cref="BorrowingsDataGrid"/>.
        /// </summary>
        private Borrowing SelectedBorrowing { get; set; }
        #endregion

        #region Constructors
        public Borrowings()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        #region AddBorrowingButton
        private async void AddBorrowingButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            new CreateBorrowingWindow(Window.GetWindow(this)).ShowDialog();

            await LoadBorrowings();
        }
        #endregion

        #region UserControl
        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadBorrowings();
        }
        #endregion

        #region RemoveFiltersButton
        private async void RemoveFiltersButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            FilterFromDatePicker.SelectedDate = null;
            FilterToDatePicker.SelectedDate = null;
            FilterDeviceComboBox.SelectedIndex = -1;
            FilterContainsTextBox.Text = "";

            await LoadBorrowings();
        }
        #endregion

        #region ApplyFiltersButton
        private async void ApplyFiltersButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadBorrowings();
        }
        #endregion

        #region DeleteBorrowingsButton
        private async void DeleteBorrowingButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (SelectedBorrowing is null)
            {
                MessageBox.Show("Es ist keine Ausleihe ausgewählt.", "Keine Ausleihe ausgewählt", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Soll die Ausleihe: \"{SelectedBorrowing.LenderName}, {SelectedBorrowing.Device.Name}, {SelectedBorrowing.DateStart:yyyy.dd.MM} - {SelectedBorrowing.DatePlannedEnd:yyyy.dd.MM}\" wirklich gelöscht werden?", $"{SelectedBorrowing.LenderName}, {SelectedBorrowing.Device.Name}, {SelectedBorrowing.DateStart:yyyy.dd.MM} - {SelectedBorrowing.DatePlannedEnd:yyyy.dd.MM} löschen", MessageBoxButton.YesNo, MessageBoxImage.Question).Equals(MessageBoxResult.Yes))
            {
                await DatabaseHandler.DeleteBorrowing(SelectedBorrowing.ID);
                await LoadBorrowings();
            }
        }
        #endregion

        #region BorrowingsDataGrid
        private void BorrowingsDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SelectedBorrowing = BorrowingsDataGrid.SelectedItem as Borrowing;

            DeleteBorrowingButton.IsEnabled = SelectedBorrowing is not null;
            EditBorrowingButton.IsEnabled = SelectedBorrowing is not null;
        }

        private async void BorrowingsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new EditBorrowingWindow(SelectedBorrowing, Window.GetWindow(this)).ShowDialog();

            await LoadBorrowings();
        }
        #endregion

        #region RefreshBorrowingsButton
        private async void RefreshBorrowingsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadBorrowings();
        }
        #endregion

        #region EditBorrowingButton
        private async void EditBorrowingButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBorrowing is null)
            {
                MessageBox.Show("Es ist keine Ausleihe ausgewählt.", "Keine Ausleihe ausgewählt", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            new EditBorrowingWindow(SelectedBorrowing, Window.GetWindow(this)).ShowDialog();

            await LoadBorrowings();
        }
        #endregion
        #endregion

        #region Methods
        #region Private
        /// <summary>
        /// Load the borrowings out of the database. Including the filters.
        /// </summary>
        private async Task LoadBorrowings()
        {
            Application.Current.MainWindow.Cursor = Cursors.Wait;
            try
            {
                await DataHandler.RefreshData();

                LoadedBorrowings = DataHandler.Borrowings;

                #region Filter
                if (FilterDeviceComboBox.SelectedIndex != -1)
                {
                    LoadedBorrowings = LoadedBorrowings.Where(b => b.Device.Type.Equals((DeviceType)(FilterDeviceComboBox.SelectedIndex + 1))).ToList();
                }
                if (FilterFromDatePicker.SelectedDate is not null)
                {
                    LoadedBorrowings = LoadedBorrowings.Where(b => b.DateStart >= FilterFromDatePicker.SelectedDate.Value).ToList();
                }
                if (FilterToDatePicker.SelectedDate is not null)
                {
                    LoadedBorrowings = LoadedBorrowings.Where(b => b.DatePlannedEnd <= FilterToDatePicker.SelectedDate.Value).ToList();
                }
                if (!FilterContainsTextBox.Text.Equals(""))
                {
                    LoadedBorrowings = LoadedBorrowings.Where(b => b.Contains(FilterContainsTextBox.Text)).ToList();
                }
                #endregion

                BorrowingsDataGrid.ItemsSource = LoadedBorrowings;
            }
            catch (Exception x)
            {
                //TODO: Fehler anzeigen
            }
            finally
            {
                Application.Current.MainWindow.Cursor = Cursors.Arrow;
            }
        }
        #endregion
        #endregion
    }
}
