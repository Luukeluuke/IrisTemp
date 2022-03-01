using Iris.src.Windows;
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
        }
        #endregion
        #endregion
    }
}
