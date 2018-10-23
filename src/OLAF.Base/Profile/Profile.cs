using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class Profile : OLAFApi<Profile>
    {
        #region Constructors
        public Profile(string name)
        {
            Name = name;
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Abstract members
        public string Name { get; }
        public abstract ApiResult Init();
        public abstract ApiResult Start();
        #endregion

        #region Properties
        public List<Monitor> Monitors { get; protected set; }

        public static string[] ImageWildcardExtensions = {"*.bmp", "*.dib", "*.rle", "*.jpg", "*.jpeg", "*.jpe", "*.jfif", "*.gif", "*.tif",
            "*.tiff", "*.png"};
        #endregion

        #region Methods
        public virtual ApiResult Shutdown()
        {
            if (Status != ApiStatus.Ok)
            {
                throw new InvalidOperationException("This monitor is not started.");
            }
            Parallel.ForEach(Monitors, m => m.Shutdown());
            if (Monitors.All(m => m.ShutdownCompleted))
            {
                Status = ApiStatus.Ok;
                return ApiResult.Success;
            }
            else
            {
                Error("One or more of the monitors did not shutdown.");
                Status = ApiStatus.Error;
                return ApiResult.Failure;
            }
        }
        #endregion
    }
}
