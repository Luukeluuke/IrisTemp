using System;
using System.Threading.Tasks;

namespace Iris.Structures
{
    internal class Borrowing
    {
        #region Properties and Variables
        /// <summary>
        /// ID of the borrowing.
        /// </summary>
        public int ID { get; private set; }
        /// <summary>
        /// ID of the borrowed device.
        /// </summary>
        public int DeviceID { get; private set; }
        /// <summary>
        /// The borrowed device.
        /// </summary>
        public Device Device => DataHandler.GetDeviceByID(DeviceID);
        /// <summary>
        /// Name of the loaner of the borrowing.
        /// (The person who gave the device to the lender)
        /// </summary>
        public string Loaner { get; private set; }
        /// <summary>
        /// Name of the taker of the borrowing.
        /// (The person who took the device back from the lender)
        /// </summary>
        public string Taker { get; private set; }
        /// <summary>
        /// Name of the lender.
        /// </summary>
        public string LenderName { get; private set; }
        /// <summary>
        /// Phone number of the lender.
        /// </summary>
        public string LenderPhone { get; private set; }
        /// <summary>
        /// Email of the lender.
        /// </summary>
        public string LenderEmail { get; private set; }
        /// <summary>
        /// Start date of the borrowing.
        /// </summary>
        public DateTime DateStart { get; private set; }
        /// <summary>
        /// Planned end of the borrowing.
        /// </summary>
        public DateTime DatePlannedEnd { get; private set; }
        /// <summary>
        /// The actual end of the borrowing.
        /// </summary>
        public DateTime DateEnd { get; private set; }
        /// <summary>
        /// Whether the borrowing is active. 
        /// (Is the device really borrowed?)
        /// </summary>
        public bool IsBorrowed { get; private set; }
        /// <summary>
        /// Notes about the borrowing.
        /// </summary>
        public string Notes { get; private set; }
        #endregion

        #region Constructors
        /// <param name="id">The ID from the database.</param>
        /// <param name="deviceID">The ID of the borrowed device.</param>
        /// <param name="loaner">Name of the loaner of the borrowing.</param>
        /// <param name="taker">Name of the taker of the borrowing.</param>
        /// <param name="lenderName">Name of the lender.</param>
        /// <param name="lenderPhone">Phone number of the lender.</param>
        /// <param name="lenderEmail">Start date of the borrowing.</param>
        /// <param name="dateStart">Start date of the borrowing.</param>
        /// <param name="datePlannedEnd">Planned end of the borrowing.</param>
        /// <param name="dateEnd">The actual end of the borrowing.</param>
        /// <param name="isBorrowed">Whether the borrowing is active. </param>
        /// <param name="notes">Notes about the borrowing.</param>
        public Borrowing(int id, int deviceID, string loaner, string taker, string lenderName, string lenderPhone, string lenderEmail, DateTime dateStart, DateTime datePlannedEnd, DateTime dateEnd, bool isBorrowed, string notes)
        {
            ID = id;
            DeviceID = deviceID;
            Loaner = loaner;
            Taker = taker;
            LenderName = lenderName;
            LenderPhone = lenderPhone;
            LenderEmail = lenderEmail;
            DateStart = dateStart;
            DatePlannedEnd = datePlannedEnd;
            DateEnd = dateEnd;
            IsBorrowed = isBorrowed;
            Notes = notes;
        }
        #endregion

        #region Methods
        #region Public
        /// <summary>
        /// Initial create of a borrowing. Creates it in the database.
        /// </summary>
        /// <param name="deviceID">The ID of the borrowed device.</param>
        /// <param name="loaner">Name of the loaner of the borrowing.</param>
        /// <param name="taker">Name of the taker of the borrowing.</param>
        /// <param name="lenderName">Name of the lender.</param>
        /// <param name="lenderPhone">Phone number of the lender.</param>
        /// <param name="lenderEmail">Start date of the borrowing.</param>
        /// <param name="dateStart">Start date of the borrowing.</param>
        /// <param name="datePlannedEnd">Planned end of the borrowing.</param>
        /// <param name="dateEnd">The actual end of the borrowing.</param>
        /// <param name="isBorrowed">Whether the borrowing is active. </param>
        /// <param name="notes">Notes about the borrowing.</param>
        /// <returns>The created borrowing.</returns>
        public static async Task<Borrowing> CreateNewBorrowing(int deviceID, string loaner, string taker, string lenderName, string lenderPhone, string lenderEmail, DateTime dateStart, DateTime datePlannedEnd, DateTime dateEnd, bool isBorrowed, string notes)
        {
            return null;

            //TODO: Database Insert Borrowing

            //TODO: Return last created borrowing
        }

        /// <summary>
        /// Checks if the given match is found in the borrowing. (all properties. Including the device)
        /// </summary>
        /// <param name="match">The string to search for.</param>
        /// <returns>Whether the borrowing contains the given string.</returns>
        public bool Contains(string match)
        {
            return Device.Contains(match) || Loaner.Contains(match, StringComparison.CurrentCultureIgnoreCase) || Taker.Contains(match, StringComparison.CurrentCultureIgnoreCase) || LenderName.Contains(match, StringComparison.CurrentCultureIgnoreCase) || LenderPhone.Contains(match, StringComparison.CurrentCultureIgnoreCase) || LenderEmail.Contains(match, StringComparison.CurrentCultureIgnoreCase) || DateStart.ToLongDateString().Contains(match, StringComparison.CurrentCultureIgnoreCase) || DatePlannedEnd.ToLongDateString().Contains(match, StringComparison.CurrentCultureIgnoreCase) || DateEnd.ToLongDateString().Contains(match, StringComparison.CurrentCultureIgnoreCase) || Notes.Contains(match, StringComparison.CurrentCultureIgnoreCase);
        }

        public override string ToString()
        {
            return $"ID: '{ID}', DeviceID: '{DeviceID}', Lender-name: '{LenderName}'";
        }
        #endregion
        #endregion
    }
}
