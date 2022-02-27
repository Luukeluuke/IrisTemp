using System;

namespace Iris.Devices
{
    internal enum DeviceType
    {
        Notebook = 1,
        GigaCube = 2,
        ERK_Meeting = 3,
        Special = 4
    }

    internal class Device
    {
        #region Properties and Variables
        /// <summary>
        /// ID of the device.
        /// </summary>
        public int ID { get; private set; }
        /// <summary>
        /// Name of the device.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Editable notes of the device.
        /// </summary>
        public string Notes { get; private set; }
        /// <summary>
        /// The type of the device.
        /// </summary>
        public DeviceType Type { get; private set; }
        #endregion

        #region Constructors
        /// <param name="id">The ID from the database.</param>
        /// <param name="name">The name of the device.</param>
        /// <param name="notes">The notes of the device.</param>
        /// <param name="type">The device type.</param>
        public Device(int id, string name, string notes, DeviceType type)
        {
            ID = id;
            Name = name;
            Notes = notes;
            Type = type;
        }

        /// <summary>
        /// Used for database.
        /// </summary>
        /// <param name="id">The ID from the database.</param>
        /// <param name="name">The name of the device.</param>
        /// <param name="notes">The notes of the device.</param>
        /// <param name="typeId">The device typeId.</param>
        public Device(int id, string name, string notes, int typeId)
        {
            ID = id;
            Name = name;
            Notes = notes;
            Type = (DeviceType)typeId;
        }
        #endregion

        #region Methods
        #region Public
        public static Device CreateDevice(string name, string notes, DeviceType type)
        {
            //TODO: Die query erst an die Datenbank und dann die Daten wieder aus der Datenbank holen und in den cosntructor packen


            return new Device(-1, name, notes, type);
        }

        /// <summary>
        /// Checks if the given match is found in name or notes.
        /// </summary>
        /// <param name="match">The string to search for.</param>
        /// <returns>Whether the device contains the given string.</returns>
        public bool Contains(string match)
        {
            return Name.Contains(match, System.StringComparison.CurrentCultureIgnoreCase) || Notes.Contains(match, System.StringComparison.CurrentCultureIgnoreCase);
        }

        public override string ToString()
        {
            return $"ID: '{ID}', Type: '{Type}', Name: '{Name}'";
        }
        #endregion
        #endregion
    }
}
