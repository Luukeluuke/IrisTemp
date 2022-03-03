using Iris.Database;
using System;
using System.Threading.Tasks;

namespace Iris.Structures
{
    public class Borrowing
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
        public long DateStartUnix { get; private set; }
        public string DateStartString => DateStart.ToString("dd.MM.yyyy");
        /// <summary>
        /// Planned end of the borrowing.
        /// </summary>
        public DateTime DatePlannedEnd { get; private set; }
        public long DatePlannedEndUnix { get; private set; }
        public string DatePlannedEndString => DatePlannedEnd.ToString("dd.MM.yyyy");
        /// <summary>
        /// The actual end of the borrowing.
        /// </summary>
        public DateTime? DateEnd { get; private set; }
        public long DateEndUnix { get; private set; }
        public string DateEndString => DateEndUnix == -1 ? "" : DateEnd.Value.ToString("dd.MM.yyyy");

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
        public Borrowing(int id, int deviceID, string loaner, string taker, string lenderName, string lenderPhone, string lenderEmail, long dateStart, long datePlannedEnd, long dateEnd, bool isBorrowed, string notes)
        {
            ID = id;
            DeviceID = deviceID;
            Loaner = loaner.Equals(Global.NullDBString) ? null : loaner;
            Taker = taker.Equals(Global.NullDBString) ? null : taker;
            LenderName = lenderName;
            LenderPhone = lenderPhone.Equals(Global.NullDBString) ? null : lenderPhone;
            LenderEmail = lenderEmail.Equals(Global.NullDBString) ? null : lenderEmail;

            DateStartUnix = dateStart;
            DateStart = DateTimeOffset.FromUnixTimeSeconds(dateStart).DateTime;
            DatePlannedEndUnix = datePlannedEnd;
            DatePlannedEnd = DateTimeOffset.FromUnixTimeSeconds(datePlannedEnd).DateTime; ;
            DateEndUnix = dateEnd;
            DateEnd = DateTimeOffset.FromUnixTimeSeconds(dateEnd).DateTime; ;

            IsBorrowed = isBorrowed;
            Notes = notes.Equals(Global.NullDBString) ? null : notes;
        }
        #endregion

        #region Methods
        #region Public
        /// <summary>
        /// Initial create of a borrowing. Creates it in the database.
        /// </summary>
        /// <param name="deviceID">ID of the borrowed device.</param>
        /// <param name="loaner">Name of the loaner of the borrowing.</param>
        /// <param name="lenderName">Name of the lender.</param>
        /// <param name="lenderPhone">Phone number of the lender.</param>
        /// <param name="lenderEmail">Start date of the borrowing.</param>
        /// <param name="dateStart">Start date of the borrowing.</param>
        /// <param name="datePlannedEnd">Planned end of the borrowing.</param>
        /// <param name="isBorrowed">Whether the borrowing is active. </param>
        /// <param name="notes">Notes about the borrowing.</param>
        /// <returns>The created borrowing.</returns>
        public static async Task<Borrowing> CreateNewBorrowing(int deviceID, string loaner, string lenderName, string lenderPhone, string lenderEmail, DateTime dateStart, DateTime datePlannedEnd, bool isBorrowed, string notes)
        {
            await DatabaseHandler.InsertBorrowing(deviceID,
                                            loaner,
                                            null,
                                            lenderName,
                                            lenderPhone,
                                            lenderEmail,
                                            new DateTimeOffset(new DateTime(dateStart.Year, dateStart.Month, dateStart.Day).AddHours(1)).ToUnixTimeSeconds(),
                                            new DateTimeOffset(new DateTime(datePlannedEnd.Year, datePlannedEnd.Month, datePlannedEnd.Day).AddHours(1)).ToUnixTimeSeconds(),
                                            -1,
                                            isBorrowed,
                                            notes);

            return await DatabaseHandler.SelectLastBorrowing();
        }

        /// <summary>
        /// Checks if the given match is found in the borrowing. (all properties. Including the device)
        /// </summary>
        /// <param name="match">The string to search for.</param>
        /// <returns>Whether the borrowing contains the given string.</returns>
        public bool Contains(string match)
        {
            if (Device.Contains(match)) return true;
            if (Loaner is not null && Loaner.Contains(match, StringComparison.CurrentCultureIgnoreCase)) return true;
            if (Taker is not null && Taker.Contains(match, StringComparison.CurrentCultureIgnoreCase)) return true;
            if (LenderName.Contains(match, StringComparison.CurrentCultureIgnoreCase)) return true;
            if (LenderPhone is not null && LenderPhone.Contains(match, StringComparison.CurrentCultureIgnoreCase)) return true;
            if (LenderEmail is not null && LenderEmail.Contains(match, StringComparison.CurrentCultureIgnoreCase)) return true;
            if (DateStartString.Contains(match, StringComparison.CurrentCultureIgnoreCase)) return true;
            if (DatePlannedEndString.Contains(match, StringComparison.CurrentCultureIgnoreCase)) return true;
            if (DateEndString.Contains(match, StringComparison.CurrentCultureIgnoreCase)) return true;
            if (Notes is not null && Notes.Contains(match, StringComparison.CurrentCultureIgnoreCase)) return true;

            return false;
        }

        /// <summary>
        /// Set the borrowing to device is borrowed.
        /// </summary>
        /// <exception cref="Exception">If the device is already borrowed, then it is invalid.</exception>
        public void Borrow()
        {
            #region Validation
            if (IsBorrowed)
            {
                throw new Exception("You tried to borrow a borrowing but it is already borrowed.");
            }
            #endregion

            IsBorrowed = true;
        }

        /// <summary>
        /// Set the borrowing to device taked back.
        /// </summary>
        /// <exception cref="Exception">If the device is already taked back, then it is invalid.</exception>
        public void TakeBack()
        {
            #region Validation
            if (IsBorrowed && DateEndUnix != -1)
            {
                throw new Exception("You tried to take a device of a borrowing back, but it is already taked back.");
            }
            #endregion

            DateEnd = DateTime.Now;
            DateEndUnix = new DateTimeOffset(DateEnd.Value).ToUnixTimeSeconds();
        }

        public override string ToString()
        {
            return $"ID: '{ID}', Device: '{Device.Name}', Lender: '{LenderName}'";
        }
        #endregion
        #endregion
    }
}
