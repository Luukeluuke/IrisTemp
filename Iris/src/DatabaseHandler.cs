using Iris.Devices;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Iris.Database
{
    internal enum DeviceSelectType
    {
        Notebook = 1,
        GigaCube = 2,
        ERK_Meeting = 3,
        Special = 4,
        All = 5
    }

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
        public static async Task<Device> SelectDevice(int id)
        {
            SqliteDataReader? reader = await ExecuteReaderAsync($@"SELECT * FROM TDevices
                                                                                WHERE
                                                                                    ID == {id}");
            
            if (reader is null)
            {
                return null;
            }

            reader.Read();
            Device device = new(reader.GetInt32(0), reader.GetString(2), reader.GetString(3), reader.GetInt32(1));

            await reader.CloseAsync();
            return device;
        }

        /// <summary>
        /// Select all devices out of the database.
        /// </summary>
        /// <returns>The devices, otherwise null.</returns>
        public static async Task<List<Device>> SelectAllDevices()
        {
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
            return devices;
        }
        #endregion

        #region Private
        /// <summary>
        /// Executes a command on the database.
        /// </summary>
        /// <param name="sqlCommand">The SQL command.</param>
        private static async void ExecuteNonQueryAsync(string sqlCommand)
        {
            Global.MainWindow.Cursor = Cursors.Wait;

            SqliteCommand command = Connection.CreateCommand();
            command.CommandText = sqlCommand;
            await command.ExecuteNonQueryAsync();

            Global.MainWindow.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Executes a read command on the database.
        /// </summary>
        /// <param name="sqlCommand">The SQL command.</param>
        /// <returns>The reader.</returns>
        private static async Task<SqliteDataReader?> ExecuteReaderAsync(string sqlCommand)
        {
            Global.MainWindow.Cursor = Cursors.Wait;

            SqliteCommand command = Connection.CreateCommand();
            command.CommandText = sqlCommand;
            SqliteDataReader? reader = await command.ExecuteReaderAsync();

            Global.MainWindow.Cursor = Cursors.Arrow;

            return reader;
        }
        #endregion
        #endregion
    }
}
