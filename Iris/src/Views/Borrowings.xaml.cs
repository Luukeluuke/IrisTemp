using Iris.src.Windows;
using System.Windows.Controls;

namespace Iris.src.Views
{
    /// <summary>
    /// Interaktionslogik für Borrowings.xaml
    /// </summary>
    public partial class Borrowings : UserControl
    {
        #region Properties and Variables

        #endregion

        #region Constructors
        public Borrowings()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        private void AddBorrowingButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            new CreateBorrowingWindow().ShowDialog();
        }
        #endregion
    }
}
