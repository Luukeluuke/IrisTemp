using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Iris
{
    static internal class Global
    {
        #region Properties and Variables
        #region Colors
        public static SolidColorBrush? MaterialDesignDarkBackground { get; private set; }
        public static SolidColorBrush? MaterialDesignDarkForeground { get; private set; }
        public static SolidColorBrush? MaterialDesignDarkSeparatorBackground { get; private set; }
        #endregion
        #endregion

        #region Constructors
        static Global()
        {
            LoadMaterialDesignColors();
        }
        #endregion

        #region Methods
        #region Private
        static private void LoadMaterialDesignColors()
        {
            MaterialDesignDarkBackground = Application.Current.FindResource("MaterialDesignDarkBackground") as SolidColorBrush;
            MaterialDesignDarkForeground = Application.Current.FindResource("MaterialDesignDarkForeground") as SolidColorBrush;
            MaterialDesignDarkSeparatorBackground = Application.Current.FindResource("MaterialDesignDarkSeparatorBackground") as SolidColorBrush;
        }
        #endregion
        #endregion
    }
}
