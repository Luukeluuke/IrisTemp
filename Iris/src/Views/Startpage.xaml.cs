using Iris.src.Windows;
using Iris.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Iris.src.Views
{
    /// <summary>
    /// Interaktionslogik für Startpage.xaml
    /// </summary>
    public partial class Startpage : UserControl
    {
        #region Properties and Variables
        private List<Borrowing> TodayLoans { get; set; }
        private Borrowing TodayLoansSelectedBorrowing { get; set; }

        private List<Borrowing> TodayTakeBacks { get; set; }
        private Borrowing TodayTakeBacksSelectedBorrowing { get; set; }

        private List<Borrowing> TooLateTakeBacks { get; set; }
        private Borrowing TooLateTakeBacksSelectedBorrowing { get; set; }

        private List<Borrowing> NotTookLoans { get; set; }
        private Borrowing NotTookLoansSelectedBorrowing { get; set; }

        private List<DeviceAvailability> DeviceAvailabilities { get; set; }
        #endregion

        #region Constructors
        public Startpage()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        #region NewBorrowingButton
        private void NewBorrowingButton_Click(object sender, RoutedEventArgs e)
        {
            new CreateBorrowingWindow().ShowDialog();

            LoadBorrowingsAndDevices();
        }
        #endregion

        #region TodayLoansDataGrid
        private void TodayLoansDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new EditBorrowingWindow(TodayLoansSelectedBorrowing).ShowDialog();

            LoadBorrowingsAndDevices();
        }

        private void TodayLoansDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            TodayLoansSelectedBorrowing = TodayLoansDataGrid.SelectedItem as Borrowing;
        }
        #endregion

        #region TodayTakeBacksDataGrid
        private void TodayTakeBacksDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new EditBorrowingWindow(TodayTakeBacksSelectedBorrowing).ShowDialog();

            LoadBorrowingsAndDevices();
        }

        private void TodayTakeBacksDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            TodayTakeBacksSelectedBorrowing = TodayTakeBacksDataGrid.SelectedItem as Borrowing;
        }
        #endregion

        #region TooLateTakeBacksDataGrid
        private void TooLateTakeBacksDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new EditBorrowingWindow(TooLateTakeBacksSelectedBorrowing).ShowDialog();

            LoadBorrowingsAndDevices();
        }

        private void TooLateTakeBacksDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            TooLateTakeBacksSelectedBorrowing = TooLateTakeBacksDataGrid.SelectedItem as Borrowing;
        }
        #endregion

        #region NotTookLoansDataGrid
        private void NotTookLoansDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new EditBorrowingWindow(NotTookLoansSelectedBorrowing).ShowDialog();

            LoadBorrowingsAndDevices();
        }

        private void NotTookLoansDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            NotTookLoansSelectedBorrowing = NotTookLoansDataGrid.SelectedItem as Borrowing;
        }
        #endregion

        #region DeviceAvailabilitiesDataGrid
        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CreateBorrowingWindow createBorrowingWindow = new(((sender as DataGridRow).Item as DeviceAvailability).Device, FromDatePicker.SelectedDate.Value.Date, ToDatePicker.SelectedDate.Value.Date);
            createBorrowingWindow.ShowDialog();

            RefreshButton_Click(null, null);
        }
        #endregion

        #region UserControl
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBorrowingsAndDevices();

            NotTookLoansTextBlock.ToolTip = $"Ausleihen welche nicht abgeholt wurden. Werden Automatisch nach {DataHandler.MaxNotTookBorrowingTime.TotalDays} Tagen gelöscht";

            DateTime now = DateTime.Now;
            FromDatePicker.SelectedDate = now.Date;
            ToDatePicker.SelectedDate = now.Date;
            LoadDeviceAvailabilities();
        }
        #endregion

        #region RefreshButton
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBorrowingsAndDevices();
        }
        #endregion

        #region DatePicker
        private void FromToDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBorrowingsAndDevices();
        }
        #endregion

        #region LoanerButton
        private void EditLoanerButton_Click(object sender, RoutedEventArgs e)
        {
            EditLoanerWindow editLoanerWindow = new EditLoanerWindow();
            editLoanerWindow.ShowDialog();
        }
        #endregion
        #endregion

        #region Methods
        #region Private
        /// <summary>
        /// Refreshs the <see cref="LoadedBorrowings"/> and <see cref="LoadedDevices"/>.
        /// </summary>
        private void LoadBorrowingsAndDevices()
        {
            DataHandler.RefreshData();

            DateTime now = DateTime.Now;

            TodayLoans = DataHandler.Borrowings.Where(b => !b.IsBorrowed && b.DateStart.Date.Equals(now.Date)).ToList();
            TodayTakeBacks = DataHandler.Borrowings.Where(b => b.IsBorrowed && b.DateEndUnix == -1 && b.DatePlannedEnd.Date.Equals(now.Date)).ToList();
            TooLateTakeBacks = DataHandler.Borrowings.Where(b => (b.IsBorrowed && b.DateEndUnix == -1) && b.DatePlannedEnd.Date < now.Date).ToList();
            NotTookLoans = DataHandler.Borrowings.Where(b => !b.IsBorrowed && b.DateStart < now.Date).ToList();

            TodayLoansDataGrid.ItemsSource = TodayLoans;
            TodayTakeBacksDataGrid.ItemsSource = TodayTakeBacks;
            TooLateTakeBacksDataGrid.ItemsSource = TooLateTakeBacks;
            NotTookLoansDataGrid.ItemsSource = NotTookLoans;

            LoadDeviceAvailabilities();
        }

        /// <summary>
        /// Loads the availabilities of the devices.
        /// </summary>
        private void LoadDeviceAvailabilities()
        {
            if (FromDatePicker.SelectedDate is null || ToDatePicker.SelectedDate is null)
            {
                DeviceAvailabilitiesDataGrid.ItemsSource = null;
                return;
            }
            else if (ToDatePicker.SelectedDate.Value.Date < FromDatePicker.SelectedDate.Value.Date)
            {
                MessageBox.Show("Der angegebene Zeitraum ist ungültig.", "Ungültiger Zeitraum", MessageBoxButton.OK, MessageBoxImage.Warning);
                DeviceAvailabilitiesDataGrid.ItemsSource = null;
                return;
            }

            List<DeviceAvailability> availabilities = new();
            foreach (Device device in DataHandler.AvailableDevices)
            {
                availabilities.Add(new(device, FromDatePicker.SelectedDate.Value.Date, ToDatePicker.SelectedDate.Value.Date));
            }

            DeviceAvailabilities = availabilities;
            DeviceAvailabilitiesDataGrid.ItemsSource = DeviceAvailabilities;
        }
        #endregion
        #endregion
    }
}