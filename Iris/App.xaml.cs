using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace Iris
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //make WPF use the system’s culture. see https://stackoverflow.com/a/520334/
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            System.Windows.Documents.Run.LanguageProperty.OverrideMetadata(typeof(System.Windows.Documents.Run), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }
    }
}
