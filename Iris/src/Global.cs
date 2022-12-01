using Iris.Structures;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        public static Window? MainWindow { get; internal set; }
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

            string plannedEndString = borrowing!.DatePlannedEnd.Year == DataHandler.permanentBorrowingYear ? "unbestimmt" : $"{borrowing.DatePlannedEnd:d}";
            
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

        public static string GetNameByPhoneNumber(string phoneNumber)
        {
            MainWindow!.Cursor = Cursors.IBeam;

            using PrincipalContext? context = new(ContextType.Domain, GetCurrentDomain());
            using PrincipalSearcher? searcher = new(new UserPrincipal(context));

            UserPrincipal userPrincipal = new(context);
            userPrincipal.Enabled = true;
            userPrincipal.VoiceTelephoneNumber = phoneNumber;

            searcher.QueryFilter = userPrincipal;

            Principal principal = searcher.FindOne();

            if (principal is not null)
            {
                DirectoryEntry user = (principal.GetUnderlyingObject() as DirectoryEntry)!;

                object? givenNameObject = user.Properties["givenName"].Value;
                string givenName = givenNameObject is null ? "" : givenNameObject!.ToString()!;

                string surname = user.Properties["sn"].Value!.ToString()!;

                return $"{surname} {givenName}";
            }

            MainWindow!.Cursor = Cursors.Arrow;

            return "";
        }
        public static void CopyMultiBorrowingEMailString(IEnumerable<Borrowing> borrowings)
        {
            //Unclean as fuck
            bool first = true;

            StringBuilder sb = new();

            foreach (Borrowing borrowing in borrowings)
            {
                if (first)
                {
                    sb.AppendLine($"{borrowing.Device.Name} - {borrowing.LenderName}");

                    if (borrowing.Device.Type.Equals(DeviceType.ERK_Meeting))
                    {
                        sb.AppendLine($"Benutzername für den Login: {borrowing.Device.Notes.Split("\r\n")[0]}@en-kreis.de");
                    }

                    first = false;
                }

                sb.AppendLine($"{borrowing.DateStart:d} bis {borrowing.DatePlannedEnd:d}");
            }

            Clipboard.SetText(sb.ToString(), TextDataFormat.Text);
        }
        #endregion

        #region Private
        /// <summary>
        /// Loads the three primary MaterialDesing colors.
        /// </summary>
        private static void LoadMaterialDesignColors()
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
        private static string GetUsersFullName()
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

        private static string GetWindowsUserName()
        {
            return Environment.UserName;
        }

        /// <summary>
        /// Get the current domain as string.
        /// </summary>
        private static string GetCurrentDomain()
        {
            return Domain.GetCurrentDomain().ToString();
        }
        #endregion
        #endregion
    }
}
