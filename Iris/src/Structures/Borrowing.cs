﻿using Iris.Database;
using System;
using System.Globalization;
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
        public long DateStartUnix { get; private set; }
        /// <summary>
        /// Planned end of the borrowing.
        /// </summary>
        public DateTime DatePlannedEnd { get; private set; }
        public long DatePlannedEndUnix { get; private set; }
        /// <summary>
        /// The actual end of the borrowing.
        /// </summary>
        public DateTime? DateEnd { get; private set; }
        public long DateEndUnix { get; private set; }

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
            DatabaseHandler.InsertBorrowing(deviceID,
                                            loaner,
                                            null,
                                            lenderName,
                                            lenderPhone,
                                            lenderEmail,
                                            new DateTimeOffset(DateTime.ParseExact(dateStart.Date.ToString("dd.MM.yyyy"), "dd.MM.yyyy", CultureInfo.InvariantCulture)).ToUnixTimeSeconds(), //,  new DateTime(dateStart.Year, dateStart.Month, dateStart.Day).AddHours(1)
                                            new DateTimeOffset(DateTime.ParseExact(datePlannedEnd.Date.ToString("dd.MM.yyyy"), "dd.MM.yyyy", CultureInfo.InvariantCulture)).ToUnixTimeSeconds(), //,  datePlannedEnd.Year, datePlannedEnd.Month, datePlannedEnd.Day).AddHours(1)
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
            //TODO: Fix & test

            return Device.Contains(match)
                || Loaner is null ? false : Loaner.Contains(match, StringComparison.CurrentCultureIgnoreCase)
                || Taker is null ? false : Taker.Contains(match, StringComparison.CurrentCultureIgnoreCase)
                || LenderName.Contains(match, StringComparison.CurrentCultureIgnoreCase)
                || LenderPhone is null ? false : LenderPhone.Contains(match, StringComparison.CurrentCultureIgnoreCase)
                || LenderEmail is null ? false : LenderEmail.Contains(match, StringComparison.CurrentCultureIgnoreCase)
                || DateStart.ToString("dd.MM.yyyy").Contains(match, StringComparison.CurrentCultureIgnoreCase)
                || DatePlannedEnd.ToString("dd.MM.yyyy").Contains(match, StringComparison.CurrentCultureIgnoreCase)
                || DateEnd is null ? false : DateEnd.Value.ToString("dd.MM.yyyy").Contains(match, StringComparison.CurrentCultureIgnoreCase)
                || Notes.Contains(match, StringComparison.CurrentCultureIgnoreCase);
        }

        public override string ToString()
        {
            return $"ID: '{ID}', DeviceID: '{DeviceID}', Lender-name: '{LenderName}'";
        }
        #endregion
        #endregion
    }
}