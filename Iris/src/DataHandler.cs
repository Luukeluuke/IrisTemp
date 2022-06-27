using Iris.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iris.Structures
{
    internal static class DataHandler
    {
        #region Properties and Variables
        /// <summary>
        /// Loaded devices out of the database.
        /// </summary>
        public static IEnumerable<Device> AvailableDevices => Devices!.Where(d => !d.IsBlocked);

        public static IEnumerable<Device>? Devices { get; private set; }

        /// <summary>
        /// Loaded borrowing out of the database.
        /// </summary>
        public static IEnumerable<Borrowing>? Borrowings { get; private set; }
        /// <summary>
        /// When true, deletes borrowings which are older than <see cref="MaxNotLoanTime"/>.
        /// </summary>
        private static bool DeleteNotTookBorrowings { get; } = true;
        /// <summary>
        /// Says
        /// </summary>
        public static TimeSpan MaxNotTookBorrowingTime { get; } = new TimeSpan(3, 0, 0, 0);

        /// <summary>
        /// Loaded loaners out of the database.
        /// </summary>
        public static IEnumerable<Loaner>? Loaners { get; private set; }
        #endregion

        #region Constructors
        static DataHandler()
        {
            RefreshData();

            if (DeleteNotTookBorrowings)
            {
                IEnumerable<Borrowing> notTookBorrowings = Borrowings!.Where(b => !b.IsBorrowed && b.DateStart < DateTime.Now.Date - MaxNotTookBorrowingTime);

                foreach (Borrowing borrowing in notTookBorrowings)
                {
                    DatabaseHandler.DeleteBorrowing(borrowing.ID);
                }
            }

            //Take gotomeeting licenses back
            IEnumerable<Borrowing> finishedErkMeetings = GetTooLateTakeBacks(DateTime.Now).Where(b => (int)b.Device.Type == 3);
            foreach (Borrowing erkMeetingB in finishedErkMeetings)
            {
                TakeBack(erkMeetingB, erkMeetingB.Notes, "System");
            }
        }
        #endregion

        #region Methods
        #region Public
        public static void Borrow(Borrowing borrowing, string notes, string loaner)
        {
            borrowing.Borrow();

            DatabaseHandler.UpdateBorrowing(borrowing.ID,
                borrowing.DeviceID,
                loaner,
                Global.NullDBString,
                borrowing.LenderName,
                borrowing.LenderPhone,
                borrowing.LenderEmail,
                borrowing.DateStartUnix,
                borrowing.DatePlannedEndUnix,
                -1,
                borrowing.IsBorrowed,
                notes);
        }
        public static void TakeBack(Borrowing borrowing, string notes, string taker)
        {
            borrowing.TakeBack();

            DatabaseHandler.UpdateBorrowing(borrowing.ID,
                borrowing.DeviceID,
                borrowing.Loaner,
                taker,
                borrowing.LenderName,
                borrowing.LenderPhone,
                borrowing.LenderEmail,
                borrowing.DateStartUnix,
                borrowing.DatePlannedEndUnix,
                borrowing.DateEndUnix,
                borrowing.IsBorrowed,
                notes);
        }

        public static IEnumerable<Borrowing> GetTodayLoans(DateTime now)
        {
            return Borrowings.Where(b => !b.IsBorrowed && b.DateStart.Date.Equals(now.Date));
        }

        public static IEnumerable<Borrowing> GetTodayTakeBacks(DateTime now)
        {
            return Borrowings.Where(b => (b.IsBorrowed && b.DateEndUnix == -1 && b.DatePlannedEnd.Date.Equals(now.Date)) && (int)b.Device.Type != 3); //TODO: am besten iwo konstanten einführen, für die IDs. Aber das muss eh nochmal überarbeitet werden, da custom device types
        }

        public static IEnumerable<Borrowing> GetTooLateTakeBacks(DateTime now)
        {
            return Borrowings.Where(b => (b.IsBorrowed && b.DateEndUnix == -1) && b.DatePlannedEnd.Date < now.Date);
        }

        public static IEnumerable<Borrowing> GetNotTookLoans(DateTime now)
        {
            return Borrowings.Where(b => !b.IsBorrowed && b.DateStart < now.Date);
        }

        public static List<DeviceAvailability> GetDeviceAvailabilities(DateTime start, DateTime end)
        {
            List<DeviceAvailability> availabilities = new();

            foreach (Device device in AvailableDevices)
            {
                availabilities.Add(new(device, start, end));
            }

            return availabilities;
        }

        /// <summary>
        /// Refresh the devices and borrowing out of the database.
        /// </summary>
        public static async void RefreshData()
        {
            Devices = (await DatabaseHandler.SelectAllDevices()).OrderBy(a => a.Type);
            Borrowings = (await DatabaseHandler.SelectAllBorrowings()).OrderBy(b => b.DateStart);
            Loaners = (await DatabaseHandler.SelectAllLoaners()).OrderBy(l => l.Name);
        }

        /// <summary>
        /// Get a device by its id.
        /// </summary>
        /// <param name="id">The id of the device you want to get.</param>
        /// <returns>The device, otherwise null.</returns>
        public static Device? GetDeviceByID(int id)
        {
            return Devices.FirstOrDefault(d => d.ID.Equals(id));
        }

        /// <summary>
        /// Checks if a device is available in a given timespan.
        /// </summary>
        /// <param name="device">The device to check.</param>
        /// <param name="startDate">The from date.</param>
        /// <param name="endDate">The to date.</param>
        /// <returns>Whether the device is availalbe in the given timespan.</returns>
        public static bool IsDeviceAvailable(Device device, DateTime startDate, DateTime endDate, int? ignoreBorrowingID = null)
        {
            return !Borrowings.Any(b =>
            {
                if (b.DeviceID != device.ID || b.ID == ignoreBorrowingID || (b.DatePlannedEnd.Equals(endDate) && b.DateEndUnix != -1))
                {
                    return false;
                }

                return (startDate <= b.DatePlannedEnd && endDate >= b.DateStart);
            });
        }

        /// <summary>
        /// Checks whether a device is currently borrowed and is not given back.
        /// </summary>
        /// <param name="device">Device to check.</param>
        /// <returns>Whether the device is currently borrowed somewhere.</returns>
        public static bool IsDeviceCurrentlyBorrowed(Device device)
        {
            return Borrowings.Any(b => b.Device.Equals(device) && b.IsBorrowed && b.DateEndUnix == -1);
        }
        #endregion
        #endregion
    }
}
