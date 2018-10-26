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
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Services = new SortedList<int, IService>();
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Properties
        public abstract string Description { get; }

        public Profile Profile { get; }
        public List<IMonitor> Monitors => Profile.Monitors;
        public Type[] MonitorClients => Monitors?.Select(m => m.Type).ToArray();
        public SortedList<int, IService> Services { get; }
        #endregion

        #region Methods
        public virtual ApiResult Init()
        {
            if (Status != ApiStatus.Initializing) return ApiResult.Failure;
            if (!Monitors.All(m => m.Status == ApiStatus.Initialized))
            {
                foreach(IMonitor m in Monitors.Where(m => m.Status != ApiStatus.Ok))
                {
                    Error("Monitor {0} has errors. Not initializing pipeline {1}.", m.GetType().Name, type.Name);
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
            if (!Monitors.All(m => m.Status == ApiStatus.Ok))
            {
                foreach (IMonitor m in Monitors.Where(m => m.Status != ApiStatus.Ok))
                {
                    Error("Monitor {0} has errors. Not initializing pipeline {1}.", m.GetType().Name, type.Name);
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
                Error("Error(s) occurred during pipeline {0} shutdown.", type.Name);
                return SetErrorStatusAndReturnFailure();
            }
        }
        #endregion
    }
}
