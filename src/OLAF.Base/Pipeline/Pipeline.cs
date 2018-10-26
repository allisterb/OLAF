using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class Pipeline : OLAFApi<Pipeline, Message>
    {
        #region Constructors
        public Pipeline(Profile profile)
        {
            Profile = profile;
        }
        #endregion

        #region Properties
        public abstract string Name { get; }
        public abstract string Description { get; }

        public Profile Profile { get; } 
        public List<IMonitor> Monitors { get; }
        public SortedList<int, IService> Services { get; }
        #endregion

        #region Methods
        public virtual ApiResult Init()
        {
            ThrowIfNotInitializing();
            if (!Monitors.All(m => m.Init() == ApiResult.Success))
            {
                foreach(IMonitor m in Monitors.Where(m => m.Status != ApiStatus.Initialized))
                {
                    Error("Monitor {0} did not initialize.", m.GetType().Name);
                }
                return SetErrorStatusAndReturnFailure();
            }

            for(int i = 0; i < Services.Count; i++)
            {
                if (Services[i].Init() != ApiResult.Success)
                {
                    Error("Service {0} did not initialize.", Services[i].GetType().Name);
                    return SetErrorStatusAndReturnFailure();
                }

            }
            return SetInitializedStatusAndReturnSucces();
        }

        public virtual ApiResult Start()
        {
            ThrowIfNotInitialized();
            if (!Monitors.All(m => m.Start() == ApiResult.Success))
            {
                foreach (IMonitor m in Monitors.Where(m => m.Status != ApiStatus.Ok))
                {
                    Error("Monitor {0} did not start.", m.GetType().Name);
                }
                return SetErrorStatusAndReturnFailure();
            }

            for (int i = 0; i < Services.Count; i++)
            {
                if (Services[i].Start() != ApiResult.Success)
                {
                    Error("Service {0} did not start.", Services[i].GetType().Name);
                    return SetErrorStatusAndReturnFailure();
                }
            }
            return SetOkStatusAndReturnSucces();
        }

        public virtual ApiResult Shutdown()
        {
            ThrowIfNotOk();
            if (!Monitors.All(m => m.Shutdown() == ApiResult.Success))
            {
                foreach (IMonitor m in Monitors.Where(m => m.Status != ApiStatus.Ok))
                {
                    Error("Monitor {0} did not shutdown.", m.GetType().Name);
                }
            }

            for (int i = 0; i < Services.Count; i++)
            {
                if (Services[i].Shutdown() != ApiResult.Success)
                {
                    Error("Service {0} did not shutdown.", Services[i].GetType().Name);
                }
            }

            if (Monitors.All(m => m.Status == ApiStatus.Ok) && Services.All(s => s.Value.Status == ApiStatus.Ok))
            {
                return SetOkStatusAndReturnSucces();
            }
            else
            {
                return SetErrorStatusAndReturnFailure();
            }
        }
        #endregion
    }
}
