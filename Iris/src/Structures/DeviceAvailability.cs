using System;

namespace Iris.Structures
{
    public class DeviceAvailability
    {
        #region Properties and Variables
        public Device Device { get; private set; }
        public bool IsAvailable { get; private set; }
        public string IsAvailableString => IsAvailable ? "Verfügbar" : "Vergeben";
        #endregion

        #region Constructor
        public DeviceAvailability(Device device, DateTime startDate, DateTime endDate)
        {
            Device = device;
            IsAvailable = DataHandler.IsDeviceAvailable(device, startDate, endDate);
        }
        #endregion
    }
}
