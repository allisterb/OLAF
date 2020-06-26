using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.ActivityDetectors
{
    public enum USBActivityEventType
    {
        Inserted = 2,
        Removed = 3
    }

    [Serializable]
    public class USBDriveActivityMessage : Message
    {
        #region Constructors
        public USBDriveActivityMessage(USBActivityEventType type, string driveLetter, DateTime time) : base()
        {
            EventType = type;
            DriveLetter = driveLetter;
            Time = time;
        }
        #endregion

        #region Properties
        public string DriveLetter { get; }
        public DateTime Time { get; }
        public USBActivityEventType EventType { get; }
        #endregion
    }
}
