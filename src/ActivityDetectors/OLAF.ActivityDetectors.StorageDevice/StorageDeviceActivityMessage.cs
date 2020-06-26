using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.ActivityDetectors
{
    public enum StorageActivityEventType
    {
        Inserted = 2,
        Removed = 3
    }

    [Serializable]
    public class StorageDeviceActivityMessage : Message
    {
        #region Constructors
        public StorageDeviceActivityMessage(StorageActivityEventType type, string driveLetter, DateTime time) : base()
        {
            EventType = type;
            DriveLetter = driveLetter;
            Time = time;
        }
        #endregion

        #region Properties
        public string DriveLetter { get; }
        public DateTime Time { get; }
        public StorageActivityEventType EventType { get; }
        #endregion
    }
}
