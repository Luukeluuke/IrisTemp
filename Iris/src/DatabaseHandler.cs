using Iris.Structures;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Iris.Database
{
    static internal class DatabaseHandler
    {
        #region Properties and Variables
        private static SqliteConnection Connection { get; set; }
        #endregion

        #region Constructors
        static DatabaseHandler()
        {
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
            Connection.Close();
        }

        #region Devices
        /// <summary>
        /// Insert a new device into the database.
        /// </summary>
        /// <param name="name">Name of the device.</param>
        /// <param name="notes">Notes of the device.</param>
        /// <param name="type">Type of the device.</param>
        public static void InsertDevice(string name, string notes, DeviceType type)
        {
            ExecuteNonQueryAsync($@"INSERT INTO TDevices 
                                                (Type, Name, Notes) 
                                            VALUES 
                                                ({(int)type}, '{name}', '{notes ?? Global.NullDBString}')");
        }

        /// <summary>
        /// Deletes a device by its ID out of the database.
        /// </summary>
        /// <param name="id">ID of the device.</param>
        public static void DeleteDevice(int id)
        {
            ExecuteNonQueryAsync($@"DELETE FROM TDevices
                                                WHERE ID == {id}");
        }

        /// <summary>
        /// Updates a device in the database.
        /// </summary>
        /// <param name="id">ID of the device.</param>
        /// <param name="name">"New" name of the device.</param>
        /// <param name="notes">"New" notes of the device.</param>
        public static void UpdateDevice(int id, string name, string notes)
        {
            ExecuteNonQueryAsync($@"UPDATE TDevices
                                                SET
                                                    Name = '{name}', Notes = '{notes ?? Global.NullDBString}'
                                                WHERE
                                                    ID == {id}");
        }

        /// <summary>
        /// Select a device by its ID out of the database.
        /// </summary>
        /// <param name="id">The ID of the device.</param>
        /// <returns>The device, otherwise null.</returns>
        //public static async Task<Device> SelectDevice(int id) //TODO: Verify if needed
        //{
        //    Global.MainWindow.Cursor = Cursors.Wait;
        //
        //    SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TDevices
        //                                                                        WHERE
        //                                                                            ID == {id}");
        //    
        //    if (reader is null)
        //    {
        //        return null;
        //    }
        //
        //    reader.Read();
        //    Device device = new(reader.GetInt32(0), reader.GetString(2), reader.GetString(3), reader.GetInt32(1));
        //
        //    await reader.CloseAsync();
        //
        //    Global.MainWindow.Cursor = Cursors.Arrow;
        //    return device;
        //}

        /// <summary>
        /// Select all devices out of the database.
        /// </summary>
        /// <returns>The devices, otherwise null.</returns>
        public static async Task<List<Device>> SelectAllDevices()
        {
            Global.MainWindow.Cursor = Cursors.Wait;

            SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TDevices");
        
            if (reader is null)
            {
                return null;
            }
        
            List<Device> devices = new();
        
            while(reader.Read())
            {
                devices.Add(new(reader.GetInt32(0), reader.GetString(2), reader.GetString(3), reader.GetInt32(1)));
            }

            await reader.CloseAsync();

            Global.MainWindow.Cursor = Cursors.Arrow;
            return devices;
        }

        /// <summary>
        /// Select all specific devices by the type out of the database.
        /// </summary>
        /// <returns>The devices, otherwise null.</returns>
        //public static async Task<List<Device>> SelectAllDevices(params DeviceType[] types) //TODO: Verify if needed
        //{
        //    #region Validation
        //    if (types.Length == 0)
        //    {
        //        return null;
        //    }
        //    #endregion
        //
        //    Global.MainWindow.Cursor = Cursors.Wait;
        //
        //    string where = string.Join(" OR ", Array.ConvertAll(types, v => $"Type == {(int)v}"));
        //
        //    SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TDevices
        //                                                                        WHERE
        //                                                                            {where}");
        //
        //    if (reader is null)
        //    {
        //        return null;
        //    }
        //
        //    List<Device> devices = new();
        //
        //    while (reader.Read())
        //    {
        //        devices.Add(new(reader.GetInt32(0), reader.GetString(2), reader.GetString(3), reader.GetInt32(1)));
        //    }
        //
        //    await reader.CloseAsync();
        //
        //    Global.MainWindow.Cursor = Cursors.Arrow;
        //    return devices;
        //}

        /// <summary>
        /// Select the last created device. 
        /// (With the highest ID)
        /// </summary>
        public static async Task<Device> SelectLastDevice()
        {
            Global.MainWindow.Cursor = Cursors.Wait;

            SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TDevices
                                                                                WHERE
                                                                                    ID == (SELECT MAX(ID) FROM TDevices)");

            if (reader is null)
            {
                return null;
            }

            reader.Read();
            Device device = new(reader.GetInt32(0), reader.GetString(2), reader.GetString(3), reader.GetInt32(1));

            await reader.CloseAsync();

            Global.MainWindow.Cursor = Cursors.Arrow;
            return device;
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
        public static void InsertBorrowing(int deviceID, string loaner, string taker, string lenderName, string lenderPhone, string lenderEmail, long dateStart, long datePlannedEnd, long dateEnd, bool isBorrowed, string notes)
        {
            ExecuteNonQueryAsync($@"INSERT INTO TBorrowings 
                                                (DeviceID, Loaner, Taker, LenderName, LenderPhone, LenderEmail, DateStart, DatePlannedEnd, DateEnd, Borrowed, Notes) 
                                            VALUES 
                                                ({deviceID}, '{loaner ?? Global.NullDBString}', '{taker ?? Global.NullDBString}', '{lenderName}', '{lenderPhone ?? Global.NullDBString}', '{lenderEmail ?? Global.NullDBString}', {dateStart}, {datePlannedEnd}, {dateEnd}, {(isBorrowed ? '1' : '0')}, '{notes ?? Global.NullDBString}')");
        }

        /// <summary>
        /// Deletes a borrowing by its ID out of the database.
        /// </summary>
        /// <param name="id">ID of the borrowing.</param>
        public static void DeleteBorrowing(int id)
        {
            ExecuteNonQueryAsync($@"DELETE FROM TBorrowings
                                                WHERE ID == {id}");
        }

        /// <summary>
        /// Select all borrowings out of the database.
        /// </summary>
        /// <returns>The borrowings, otherwise null.</returns>
        public static async Task<List<Borrowing>> SelectAllBorrowings()
        {
            Global.MainWindow.Cursor = Cursors.Wait;

            SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TBorrowings");

            if (reader is null)
            {
                return null;
            }

            List<Borrowing> borrowings = new();

            while (reader.Read())
            {
                borrowings.Add(new(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetInt64(7), reader.GetInt64(8), reader.GetInt64(9), reader.GetBoolean(10), reader.GetString(11)));
            }

            await reader.CloseAsync();

            Global.MainWindow.Cursor = Cursors.Arrow;
            return borrowings;
        }

        /// <summary>
        /// Select the last created borrowing. 
        /// (With the highest ID)
        /// </summary>
        public static async Task<Borrowing> SelectLastBorrowing()
        {
            Global.MainWindow.Cursor = Cursors.Wait;

            SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TBorrowings
                                                                                WHERE
                                                                                    ID == (SELECT MAX(ID) FROM TBorrowings)");

            if (reader is null)
            {
                return null;
            }

            reader.Read();
            Borrowing borrowing = new(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetInt64(7), reader.GetInt64(8), reader.GetInt64(9), reader.GetBoolean(10), reader.GetString(11));

            await reader.CloseAsync();

            Global.MainWindow.Cursor = Cursors.Arrow;
            return borrowing;
        }
        #endregion
        #endregion

        #region Private
        /// <summary>
        /// Executes a command on the database.
        /// </summary>
        /// <param name="sqlCommand">The SQL command.</param>
        private static async void ExecuteNonQueryAsync(string sqlCommand)
        {
            SqliteCommand command = Connection.CreateCommand();
            command.CommandText = sqlCommand;
            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Executes a read command on the database.
        /// </summary>
        /// <param name="sqlCommand">The SQL command.</param>
        /// <returns>The reader.</returns>
        private static async Task<SqliteDataReader?> ExecuteReaderAsync(string sqlCommand)
        {
            SqliteCommand command = Connection.CreateCommand();
            command.CommandText = sqlCommand;
            SqliteDataReader? reader = await command.ExecuteReaderAsync();
            
            return reader;
        }
        #endregion
        #endregion
    }
}
