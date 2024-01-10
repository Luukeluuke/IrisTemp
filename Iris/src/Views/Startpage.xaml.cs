using Iris.src.Windows;
using Iris.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Iris.src.Views
{
    /// <summary>
    /// Interaktionslogik für Startpage.xaml
    /// </summary>
    public partial class Startpage : UserControl
    {
        #region Properties and Variables
        private Borrowing? TodayLoansSelectedBorrowing { get; set; }

        private Borrowing? TodayTakeBacksSelectedBorrowing { get; set; }

        private Borrowing? TooLateTakeBacksSelectedBorrowing { get; set; }

        private Borrowing? NotTookLoansSelectedBorrowing { get; set; }

        /// <summary>
        /// Interval in seconds.
        /// </summary>
        private const int RefreshInterval = 30;
        private const bool DoRefresh = true;
        private DispatcherTimer RefreshTimer { get; }
        #endregion

        #region Constructors
        public Startpage()
        {
            InitializeComponent();

            RefreshTimer = new()
            {
                Interval = TimeSpan.FromSeconds(RefreshInterval),
                IsEnabled = DoRefresh
            };
            RefreshTimer.Tick += (_, _) =>
            {
                RefreshButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            };

            RefreshTimer.Start();
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
            new EditBorrowingWindow(TodayLoansSelectedBorrowing!).ShowDialog();

            LoadBorrowingsAndDevices();
        }

        private void TodayLoansDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            TodayLoansSelectedBorrowing = (TodayLoansDataGrid.SelectedItem as Borrowing)!;
        }
        #endregion

        #region TodayTakeBacksDataGrid
        private void TodayTakeBacksDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new EditBorrowingWindow(TodayTakeBacksSelectedBorrowing!).ShowDialog();

            LoadBorrowingsAndDevices();
        }

        private void TodayTakeBacksDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            TodayTakeBacksSelectedBorrowing = (TodayTakeBacksDataGrid.SelectedItem as Borrowing)!;
        }
        #endregion

        #region TooLateTakeBacksDataGrid
        private void TooLateTakeBacksDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new EditBorrowingWindow(TooLateTakeBacksSelectedBorrowing!).ShowDialog();

            LoadBorrowingsAndDevices();
        }

        private void TooLateTakeBacksDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            TooLateTakeBacksSelectedBorrowing = (TooLateTakeBacksDataGrid.SelectedItem as Borrowing)!;
        }
        #endregion

        #region NotTookLoansDataGrid
        private void NotTookLoansDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new EditBorrowingWindow(NotTookLoansSelectedBorrowing!).ShowDialog();

            LoadBorrowingsAndDevices();
        }

        private void NotTookLoansDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            NotTookLoansSelectedBorrowing = (NotTookLoansDataGrid.SelectedItem as Borrowing)!;
        }
        #endregion

        #region DeviceAvailabilitiesDataGrid
        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CreateBorrowingWindow createBorrowingWindow = new(((sender as DataGridRow)!.Item as DeviceAvailability)!.Device, FromDatePicker.SelectedDate!.Value.Date, ToDatePicker.SelectedDate!.Value.Date);
            createBorrowingWindow.ShowDialog();

            RefreshButton_Click(sender, e);
        }
        #endregion

        #region UserControl
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBorrowingsAndDevices(true);

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
            LoadBorrowingsAndDevices(true);
        }
        #endregion

        #region DatePicker
        private void FromDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ToDatePicker.SelectedDate = FromDatePicker.SelectedDate;
            LoadDeviceAvailabilities();
        }

        private void ToDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDeviceAvailabilities();
        }
        #endregion

        #region LoanerButton
        private void EditLoanerButton_Click(object sender, RoutedEventArgs e)
        {
            EditLoanerWindow editLoanerWindow = new();
            editLoanerWindow.ShowDialog();
        }
        #endregion
        #endregion

        #region Methods
        #region Private
        /// <summary>
        /// Refreshs the <see cref="LoadedBorrowings"/> and <see cref="LoadedDevices"/>.
        /// </summary>
        private void LoadBorrowingsAndDevices(bool refresh = false)
        {
            if (refresh)
            {
                DataHandler.LoadDataFromDatabase();
            }
            DateTime now = DateTime.Now;

            TodayLoansDataGrid.ItemsSource = DataHandler.GetTodayLoans(now);
            TodayTakeBacksDataGrid.ItemsSource = DataHandler.GetTodayTakeBacks(now);
            TooLateTakeBacksDataGrid.ItemsSource = DataHandler.GetTooLateTakeBacks(now);
            NotTookLoansDataGrid.ItemsSource = DataHandler.GetNotTookLoans(now);

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

            List<DeviceAvailability> availabilites = DataHandler.GetDeviceAvailabilities(FromDatePicker.SelectedDate.Value.Date, ToDatePicker.SelectedDate.Value.Date);
            availabilites = availabilites.OrderBy(a => a.Device.Type).ThenBy(a => a.Device.Name[^3..]).ThenBy(a => a.Device.Name).ToList(); //ungeil
            
            DeviceAvailabilitiesDataGrid.ItemsSource = availabilites;
        }
        #endregion

        #endregion
    }
}