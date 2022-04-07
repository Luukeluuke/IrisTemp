using Iris.Database;
using Iris.Structures;
using System.Collections.Generic;
using System.Windows;

namespace Iris.src.Windows
{
    /// <summary>
    /// Interaktionslogik für EditLoanerWindow.xaml
    /// </summary>
    public partial class EditLoanerWindow : Window
    {
        #region Properties and Variables
        private List<Loaner> LoadedLoaners => DataHandler.Loaners;

        /// <summary>
        /// The currently selected loaner in <see cref="LoanerDataGrid"/>.
        /// </summary>
        private Loaner SelectedLoaner { get; set; }
        #endregion

        #region Constructors
        public EditLoanerWindow()
        {
            Owner = Global.MainWindow;

            InitializeComponent();
        }
        #endregion

        #region Events
        #region Window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoanerDataGrid.ItemsSource = LoadedLoaners;
        }
        #endregion

        #region RefreshButton
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshLoaners();
        }
        #endregion

        #region CloseButton
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region LoanerDataGrid
        private void LoanerDataGrid_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            SelectedLoaner = LoanerDataGrid.SelectedItem as Loaner;

            DeleteLoanerButton.IsEnabled = SelectedLoaner is not null;
        }
        #endregion

        #region DeleteLoanerButton
        private async void DeleteLoanerButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLoaner is null)
            {
                MessageBox.Show("Es ist kein Herausgeber ausgewählt!", "Herausgeber löschen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Soll der Herausgeber: '{SelectedLoaner.Name}' wirklich gelöscht werden?", $"{SelectedLoaner.Name} löschen", MessageBoxButton.YesNo, MessageBoxImage.Question).Equals(MessageBoxResult.Yes))
            {
                await DatabaseHandler.DeleteLoaner(SelectedLoaner.ID);
                RefreshLoaners();
            }
        }
        #endregion

        #region AddLoanerButton
        private async void AddLoanerButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoanerNameTextBox.Text))
            {
                MessageBox.Show("Der angegebene Name ist ungültig!", "Herausgeber hinzufügen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await Loaner.CreateNewLoaner(LoanerNameTextBox.Text);
            RefreshLoaners();

            LoanerNameTextBox.Text = "";
        }
        #endregion
        #endregion

        #region Methods
        #region Private
        /// <summary>
        /// Refreshs the loaners.
        /// </summary>
        private void RefreshLoaners()
        {
            DataHandler.RefreshData();

            LoanerDataGrid.ItemsSource = LoadedLoaners;
        }
        #endregion
        #endregion
    }
}
