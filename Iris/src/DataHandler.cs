using Iris.Database;
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
        public static List<Borrowing> Borrowings { get; private set; }
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
        }

        /// <summary>
        /// Get a device by its id.
        /// </summary>
        /// <param name="id">The id of the device you want to get.</param>
        /// <returns>The device, otherwise null.</returns>
        public static Device GetDeviceByID(int id)
        {
            return Devices.Where(d => d.ID.Equals(id)).FirstOrDefault();
        }
        #endregion
        #endregion
    }
}
