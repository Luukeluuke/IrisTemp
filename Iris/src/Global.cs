using Iris.Structures;
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
        public static SolidColorBrush? MaterialDesignLightSeparatorBackground { get; private set; }
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
        #region Public
        /// <summary>
        /// Creates a E-Mail string for a borrowing and copies it to the clipboard.
        /// </summary>
        public static void CopyBorrowingEMailString(Borrowing? borrowing)
        {
            string bString = string.Empty;

            string plannedEndString = borrowing.DatePlannedEnd.Year == 2800 ? "unbestimmt" : $"{borrowing.DatePlannedEnd:d}";
            
            switch (borrowing.Device.Type)
            {
                case DeviceType.Notebook:
                case DeviceType.GigaCube:
                case DeviceType.Special:
                    {
                        bString = $"{borrowing.Device.Name} - {borrowing.LenderName} - {borrowing.DateStart:d} bis {plannedEndString}";
                        break;
                    }
                case DeviceType.ERK_Meeting:
                    { 
                        bString = $"{borrowing.Device.Notes.Split("\r\n")[0]}@en-kreis.de (Benutzername für den Login) - {borrowing.LenderName} - {borrowing.DateStart:d} bis {plannedEndString}";
                        break;
                    }
                default:
                    break;
            }

            Clipboard.SetText(bString, TextDataFormat.Text);
        }
        #endregion
        
        #region Private
        /// <summary>
        /// Loads the three primary MaterialDesing colors.
        /// </summary>
        static private void LoadMaterialDesignColors()
        {
            MaterialDesignDarkBackground = Application.Current.FindResource("MaterialDesignDarkBackground") as SolidColorBrush;
            MaterialDesignDarkForeground = Application.Current.FindResource("MaterialDesignDarkForeground") as SolidColorBrush;
            MaterialDesignDarkSeparatorBackground = Application.Current.FindResource("MaterialDesignDarkSeparatorBackground") as SolidColorBrush;
            MaterialDesignLightSeparatorBackground = Application.Current.FindResource("MaterialDesignLightSeparatorBackground") as SolidColorBrush;
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
