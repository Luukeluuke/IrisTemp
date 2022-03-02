using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
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

        /// <summary>
        /// The curred logged in user. (Windows profile)
        /// </summary>
        public static string CurrentUser { get; private set; }

        /// <summary>
        /// Represents a null value string. Used for database.
        /// </summary>
        public static string NullDBString { get; } = "$NuLL$";

        /// <summary>
        /// The main window of the application.
        /// </summary>
        public static Window MainWindow { get; internal set; }
        #endregion

        #region Constructors
        static Global()
        {
            LoadMaterialDesignColors();
            CurrentUser = GetUsersFullName();
        }
        #endregion

        #region Methods
        #region Private
        /// <summary>
        /// Loads the three primary MaterialDesing colors.
        /// </summary>
        static private void LoadMaterialDesignColors()
        {
            MaterialDesignDarkBackground = Application.Current.FindResource("MaterialDesignDarkBackground") as SolidColorBrush;
            MaterialDesignDarkForeground = Application.Current.FindResource("MaterialDesignDarkForeground") as SolidColorBrush;
            MaterialDesignDarkSeparatorBackground = Application.Current.FindResource("MaterialDesignDarkSeparatorBackground") as SolidColorBrush;
        }

        /// <summary>
        /// Get the current users fullname.
        /// In case of an error, the windows username will be returned.
        /// </summary>
        static private string GetUsersFullName()
        {
            try
            {
                using PrincipalContext? context = new(ContextType.Domain, GetCurrentDomain());
                using PrincipalSearcher? searcher = new(new UserPrincipal(context));
                UserPrincipal userPrin = new(context)
                {
                    Enabled = true,
                    SamAccountName = GetWindowsUserName()
                };
                searcher.QueryFilter = userPrin;

                return searcher.FindOne().ToString();
            }
            catch
            {
                return GetWindowsUserName();
            }
        }

        static private string GetWindowsUserName()
        {
            return Environment.UserName;
        }

        /// <summary>
        /// Get the current domain as string.
        /// </summary>
        static private string GetCurrentDomain()
        {
            return Domain.GetCurrentDomain().ToString();
        }
        #endregion
        #endregion
    }
}
