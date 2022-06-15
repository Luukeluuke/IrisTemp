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
        public static List<Device> AvailableDevices
        {
            get
            {
                return devices.Where(d => !d.IsBlocked).ToList();
            }
        }
        public static List<Device> AllDevices
        {
            get
            {
                return devices;
            }
            private set
            {
                devices = value;
            }
        }
        private static List<Device> devices;

        /// <summary>
        /// Loaded borrowing out of the database.
        /// </summary>
        public static List<Borrowing> Borrowings { get; private set; }
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
        public static List<Loaner> Loaners { get; private set; }
        #endregion

        #region Constructors
        static DataHandler()
        {
            RefreshData();

            if (DeleteNotTookBorrowings)
            {
                List<Borrowing> notTookBorrowings = Borrowings.Where(b => !b.IsBorrowed && b.DateStart < DateTime.Now.Date - MaxNotTookBorrowingTime).ToList();

                foreach (Borrowing borrowing in notTookBorrowings)
                {
                    DatabaseHandler.DeleteBorrowing(borrowing.ID);
                }
            }
        }
        #endregion

        #region Methods
        #region Public
        /// <summary>
        /// Refresh the devices and borrowing out of the database.
        /// </summary>
        public static async void RefreshData()
        {
            AllDevices = (await DatabaseHandler.SelectAllDevices()).OrderBy(a => a.Type).ToList();
            Borrowings = (await DatabaseHandler.SelectAllBorrowings()).OrderBy(b => b.DateStart).ToList();
            Loaners = (await DatabaseHandler.SelectAllLoaners()).OrderBy(l => l.Name).ToList();
        }

        /// <summary>
        /// Get a device by its id.
        /// </summary>
        /// <param name="id">The id of the device you want to get.</param>
        /// <returns>The device, otherwise null.</returns>
        public static Device GetDeviceByID(int id)
        {
            return AllDevices.FirstOrDefault(d => d.ID.Equals(id));
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
