﻿using Iris.Database;
using System;
using System.Threading.Tasks;

namespace Iris.Structures
{
    public enum DeviceType
    {
        Notebook = 1,
        GigaCube = 2,
        ERK_Meeting = 3,
        Special = 4
    }

    public class Device
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
        /// <summary>
        /// Blocked state of the device.
        /// </summary>
        public bool IsBlocked { get; private set; }
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
            IsBlocked = false;
        }

        /// <summary>
        /// Used for database.
        /// </summary>
        /// <param name="id">The ID from the database.</param>
        /// <param name="name">The name of the device.</param>
        /// <param name="notes">The notes of the device.</param>
        /// <param name="typeId">The device typeId.</param>
        public Device(int id, string name, string notes, int typeId, bool isBlocked)
        {
            ID = id;
            Name = name;
            Notes = notes.Equals(Global.NullDBString) ? null : notes;
            Type = (DeviceType)typeId;
            IsBlocked = isBlocked;
        }
        #endregion

        #region Methods
        #region Public
        /// <summary>
        /// Initial create of a device. Creates it in the database.
        /// </summary>
        /// <param name="name">Name of the device.</param>
        /// <param name="notes">Notes of the device.</param>
        /// <param name="type">Type of the device.</param>
        /// <returns>The created device.</returns>
        public static async Task<Device> CreateNewDevice(string name, string notes, DeviceType type)
        {
            return await DatabaseHandler.InsertDevice(name, notes, type);
        }

        /// <summary>
        /// Checks if the given match is found in name or notes.
        /// </summary>
        /// <param name="match">The string to search for.</param>
        /// <returns>Whether the device contains the given string.</returns>
        public bool Contains(string match)
        {
            return Name.Contains(match, StringComparison.CurrentCultureIgnoreCase) || Notes.Contains(match, StringComparison.CurrentCultureIgnoreCase);
        }

        public override string ToString()
        {
            return $"ID: '{ID}', Type: '{Type}', Name: '{Name}'";
        }
        #endregion
        #endregion
    }
}
