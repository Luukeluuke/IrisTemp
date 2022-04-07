using Iris.Structures;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Iris.Database
{
    static internal class DatabaseHandler
    {
        #region Properties and Variables
        private static SqliteConnection Connection { get; set; }

        /// <summary>
        /// Whether has a connection to database.
        /// </summary>
        public static bool IsConnected { get; private set; } = true;
        #endregion

        #region Constructors
        static DatabaseHandler()
        {
            if (!File.Exists("Iris_devices.db"))
            {
                IsConnected = false;
                return;
            }

            Connection = new("Data Source=Iris_devices.db");
            Connection.Open();
        }
        #endregion

        #region Methods
        #region Public
        /// <summary>
        /// Closes the database connection.
        /// </summary>
        public static void CloseDBConnection()
        {
            if (IsConnected)
            {
                Connection.Close();
            }
        }

        #region Devices
        /// <summary>
        /// Insert a new device into the database.
        /// </summary>
        /// <param name="name">Name of the device.</param>
        /// <param name="notes">Notes of the device.</param>
        /// <param name="type">Type of the device.</param>
        public static async Task<Device> InsertDevice(string name, string notes, DeviceType type)
        {
            try
            {
                ExecuteNonQuery($@"INSERT INTO TDevices 
                                                    (Type, Name, Notes, IsBlocked) 
                                                VALUES 
                                                    ({(int)type}, 
                                                    @name,
                                                    @notes,
                                                    @isBlocked)",
                                                    new SqliteParameter("@name", name),
                                                    new SqliteParameter("@notes", notes ?? Global.NullDBString),
                                                    new SqliteParameter("@isBlocked", false));

                SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TDevices
                                                                                    WHERE
                                                                                        rowid == last_insert_rowid()");

                if (reader is null)
                {
                    return null;
                }

                await reader.ReadAsync();
                Device device = new(reader.GetInt32(0), reader.GetString(2), reader.GetString(3), reader.GetInt32(1), reader.GetBoolean(4));

                await reader.CloseAsync();

                return device;
            }
            catch
            {
                return null;
            }
            
        }

        /// <summary>
        /// Deletes a device by its ID out of the database.
        /// </summary>
        /// <param name="id">ID of the device.</param>
        public static async Task<bool> DeleteDevice(int id)
        {
            try
            {
                ExecuteNonQuery($@"DELETE FROM TDevices
                                                    WHERE ID == {id}");
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates a device in the database.
        /// </summary>
        /// <param name="id">ID of the device.</param>
        /// <param name="name">"New" name of the device.</param>
        /// <param name="notes">"New" notes of the device.</param>
        public static async Task<bool> UpdateDevice(int id, string name, string notes, bool isBlocked)
        {
            try
            {
                ExecuteNonQuery($@"UPDATE TDevices
                                                    SET
                                                        Name = @name, 
                                                        Notes = @notes,
                                                        IsBlocked = @isBlocked
                                                    WHERE
                                                        ID == {id}",
                                                        new SqliteParameter("@name", name),
                                                        new SqliteParameter("@notes", notes ?? Global.NullDBString),
                                                        new SqliteParameter("@isBlocked", isBlocked));
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Select a device by its ID out of the database.
        /// </summary>
        /// <param name="id">The ID of the device.</param>
        /// <returns>The device, otherwise null.</returns>
        public static async Task<Device> SelectDevice(int id)
        {
            Device device = null;

            try
            {
                Global.MainWindow.Cursor = Cursors.Wait;
        
                SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TDevices
                                                                                    WHERE
                                                                                        ID == {id}");
                
                if (reader is null)
                {
                    return null;
                }
        
                await reader.ReadAsync();
                device = new(reader.GetInt32(0), reader.GetString(2), reader.GetString(3), reader.GetInt32(1), reader.GetBoolean(4));
        
                await reader.CloseAsync();
            }
            finally
            {
                Global.MainWindow.Cursor = Cursors.Arrow;
            }
        
            return device;
        }

        /// <summary>
        /// Select all devices out of the database.
        /// </summary>
        /// <returns>The devices.</returns>
        public static async Task<List<Device>> SelectAllDevices()
        {
            List<Device> devices = new();

            try
            {
                Global.MainWindow.Cursor = Cursors.Wait;

                SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TDevices");

                if (reader is null)
                {
                    return null;
                }

                while (await reader.ReadAsync())
                {
                    devices.Add(new(reader.GetInt32(0), reader.GetString(2), reader.GetString(3), reader.GetInt32(1), reader.GetBoolean(4)));
                }

                await reader.CloseAsync();
            }
            finally
            {
                Global.MainWindow.Cursor = Cursors.Arrow;
            }

            return devices;
        }
        #endregion

        #region Borrowings
        /// <summary>
        /// Insert a new borrowing into the database.
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
        public static async Task<Borrowing> InsertBorrowing(int deviceID, string loaner, string taker, string lenderName, string lenderPhone, string lenderEmail, long dateStart, long datePlannedEnd, long dateEnd, bool isBorrowed, string notes)
        {
            try
            {
                ExecuteNonQuery($@"INSERT INTO TBorrowings 
                                                    (DeviceID, Loaner, Taker, LenderName, LenderPhone, LenderEmail, DateStart, DatePlannedEnd, DateEnd, Borrowed, Notes) 
                                                VALUES 
                                                    ({deviceID}, 
                                                    @loaner, 
                                                    @taker, 
                                                    @lenderName, 
                                                    @lenderPhone, 
                                                    @lenderEmail, 
                                                    {dateStart}, 
                                                    {datePlannedEnd}, 
                                                    {dateEnd}, 
                                                    {(isBorrowed ? '1' : '0')}, 
                                                    @notes)",
                                                    new SqliteParameter("@loaner", loaner ?? Global.NullDBString),
                                                    new SqliteParameter("@taker", taker ?? Global.NullDBString),
                                                    new SqliteParameter("@lenderName", lenderName),
                                                    new SqliteParameter("@lenderPhone", lenderPhone ?? Global.NullDBString),
                                                    new SqliteParameter("@lenderEmail", lenderEmail ?? Global.NullDBString),
                                                    new SqliteParameter("@notes", notes ?? Global.NullDBString));

                SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TBorrowings
                                                                                    WHERE
                                                                                        rowid == last_insert_rowid()");

                if (reader is null)
                {
                    return null;
                }

                reader.Read();
                Borrowing borrowing = new(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetInt64(7), reader.GetInt64(8), reader.GetInt64(9), reader.GetBoolean(10), reader.GetString(11));

                await reader.CloseAsync();
                return borrowing;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes a borrowing by its ID out of the database.
        /// </summary>
        /// <param name="id">ID of the borrowing.</param>
        public static async Task<bool> DeleteBorrowing(int id)
        {
            try
            {
                ExecuteNonQuery($@"DELETE FROM TBorrowings
                                                    WHERE ID == {id}");
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates a borrowing in the database.
        /// </summary>
        /// <param name="id">ID of the device.</param>
        public static async Task<bool> UpdateBorrowing(int id, int deviceID, string loaner, string taker, string lenderName, string lenderPhone, string lenderEmail, long dateStart, long datePlannedEnd, long dateEnd, bool isBorrowed, string notes)
        {
            try
            {
                ExecuteNonQuery($@"UPDATE TBorrowings
                                                    SET
                                                        DeviceID = {deviceID}, 
                                                        Loaner = @loaner, 
                                                        Taker = @taker, 
                                                        LenderName = @lenderName, 
                                                        LenderPhone = @lenderPhone,
                                                        LenderEmail = @lenderEmail,
                                                        DateStart = {dateStart},
                                                        DatePlannedEnd = {datePlannedEnd},
                                                        DateEnd = {dateEnd},
                                                        Borrowed = {(isBorrowed ? '1' : '0')},
                                                        Notes = @notes
                                                    WHERE
                                                        ID == {id}",
                                                        new SqliteParameter("@loaner", loaner ?? Global.NullDBString),
                                                        new SqliteParameter("@taker", taker ?? Global.NullDBString),
                                                        new SqliteParameter("@lenderName", lenderName),
                                                        new SqliteParameter("@lenderPhone", lenderPhone ?? Global.NullDBString),
                                                        new SqliteParameter("@lenderEmail", lenderEmail ?? Global.NullDBString),
                                                        new SqliteParameter("@notes", notes ?? Global.NullDBString));
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Select a borrowing by its ID out of the database.
        /// </summary>
        /// <param name="id">The ID of the borrowing.</param>
        /// <returns>The borrowing, otherwise null.</returns>
        public static async Task<Borrowing> SelectBorrowing(int id)
        {
            Borrowing borrowing = null;

            try
            {
                Global.MainWindow.Cursor = Cursors.Wait;

                SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TBorrowings
                                                                                    WHERE
                                                                                        ID == {id}");

                if (reader is null)
                {
                    return null;
                }

                await reader.ReadAsync();
                borrowing = new(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetInt64(7), reader.GetInt64(8), reader.GetInt64(9), reader.GetBoolean(10), reader.GetString(11));

                await reader.CloseAsync();
            }
            finally
            {
                Global.MainWindow.Cursor = Cursors.Arrow;
            }

            return borrowing;
        }

        /// <summary>
        /// Select all borrowings out of the database.
        /// </summary>
        /// <returns>The borrowings.</returns>
        public static async Task<List<Borrowing>> SelectAllBorrowings()
        {
            List<Borrowing> borrowings = new();

            try
            {
                Global.MainWindow.Cursor = Cursors.Wait;

                SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TBorrowings");

                if (reader is null)
                {
                    return null;
                }

                while (reader.Read())
                {
                    borrowings.Add(new(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetInt64(7), reader.GetInt64(8), reader.GetInt64(9), reader.GetBoolean(10), reader.GetString(11)));
                }

                await reader.CloseAsync();
            }
            finally
            {
                Global.MainWindow.Cursor = Cursors.Arrow;
            }

            return borrowings;
        }
        #endregion

        #region Loaner
        /// <summary>
        /// Insert a new loaner into the database.
        /// </summary>
        /// <param name="name">Name of the loaner.</param>
        public static async Task<Loaner> InsertLoaner(string name)
        {
            try
            {
                ExecuteNonQuery($@"INSERT INTO TLoaners
                                                    (Name) 
                                                VALUES 
                                                    (@name)",
                                                    new SqliteParameter("@name", name));

                SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TLoaners
                                                                                    WHERE
                                                                                        rowid == last_insert_rowid()");

                if (reader is null)
                {
                    return null;
                }

                await reader.ReadAsync();
                Loaner loaner = new(reader.GetInt32(0), reader.GetString(1));

                await reader.CloseAsync();

                return loaner;
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// Deletes a loaner by its ID out of the database.
        /// </summary>
        /// <param name="id">ID of the loaner.</param>
        public static async Task<bool> DeleteLoaner(int id)
        {
            try
            {
                ExecuteNonQuery($@"DELETE FROM TLoaners
                                                    WHERE ID == {id}");
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Select a loaner by its ID out of the database.
        /// </summary>
        /// <param name="id">The ID of the loaner.</param>
        /// <returns>The loaner, otherwise null.</returns>
        public static async Task<Loaner> SelectLoaner(int id)
        {
            Loaner loaner = null;

            try
            {
                Global.MainWindow.Cursor = Cursors.Wait;

                SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TLoaners
                                                                                    WHERE
                                                                                        ID == {id}");

                if (reader is null)
                {
                    return null;
                }

                await reader.ReadAsync();
                loaner = new(reader.GetInt32(0), reader.GetString(1));

                await reader.CloseAsync();
            }
            finally
            {
                Global.MainWindow.Cursor = Cursors.Arrow;
            }

            return loaner;
        }

        /// <summary>
        /// Select all loaners out of the database.
        /// </summary>
        /// <returns>The loaners.</returns>
        public static async Task<List<Loaner>> SelectAllLoaners()
        {
            List<Loaner> loaners = new();

            try
            {
                Global.MainWindow.Cursor = Cursors.Wait;

                SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TLoaners");

                if (reader is null)
                {
                    return null;
                }

                while (await reader.ReadAsync())
                {
                    loaners.Add(new(reader.GetInt32(0), reader.GetString(1)));
                }

                await reader.CloseAsync();
            }
            finally
            {
                Global.MainWindow.Cursor = Cursors.Arrow;
            }

            return loaners;
        }
        #endregion
        #endregion

        #region Private
        /// <summary>
        /// Executes a command on the database.
        /// </summary>
        /// <param name="sqlCommand">The SQL command.</param>
        private static async void ExecuteNonQuery(string sqlCommand, params SqliteParameter[] paramter)
        {
            SqliteCommand command = Connection.CreateCommand();
            command.CommandText = sqlCommand;
            Array.ForEach(paramter, p => command.Parameters.Add(p));
            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Executes a read command on the database.
        /// </summary>
        /// <param name="sqlCommand">The SQL command.</param>
        /// <returns>The reader.</returns>
        private static async Task<SqliteDataReader?> ExecuteReaderAsync(string sqlCommand, params SqliteParameter[] paramter)
        {
            SqliteCommand command = Connection.CreateCommand();
            command.CommandText = sqlCommand;
            Array.ForEach(paramter, p => command.Parameters.Add(p));
            SqliteDataReader? reader = await command.ExecuteReaderAsync();

            return reader;
        }
        #endregion
        #endregion
    }
}
