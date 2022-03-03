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
        public static List<Device> Devices { get; private set; }
        /// <summary>
        /// Loaded borrowing out of the database.
        /// </summary>
        public static List<Borrowing> Borrowings { get; private set; } = new(); //TODO: Remove this
        #endregion

        #region Constructors
        static DataHandler()
        {
            RefreshData();
        }
        #endregion

        #region Methods
        #region Public
        /// <summary>
        /// Refresh the devices and borrowing out of the database.
        /// </summary>
        public static async void RefreshData()
        {
            Devices = (await DatabaseHandler.SelectAllDevices()).OrderBy(a => a.Type).ToList();
            Borrowings = (await DatabaseHandler.SelectAllBorrowings()).OrderBy(b => b.DateStart).ToList();
        }

        /// <summary>
        /// Get a device by its id.
        /// </summary>
        /// <param name="id">The id of the device you want to get.</param>
        /// <returns>The device, otherwise null.</returns>
        public static Device GetDeviceByID(int id)
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
            List<Borrowing> relevantBorrowings = Borrowings.Where(b => b.DeviceID.Equals(device.ID)).ToList();

            if (ignoreBorrowingID is not null)
            {
                relevantBorrowings = relevantBorrowings.Where(b => !b.ID.Equals(ignoreBorrowingID)).ToList();
            }

            foreach (Borrowing borrowing in relevantBorrowings)
            {
                if ((startDate >= borrowing.DateStart) && (startDate <= borrowing.DatePlannedEnd)) return false;
                if ((endDate >= borrowing.DateStart) && (endDate <= borrowing.DatePlannedEnd)) return false;

                if ((startDate < borrowing.DateStart && endDate > borrowing.DatePlannedEnd)) return false;
            }

            return true;
        }
        #endregion
        #endregion
    }
}
